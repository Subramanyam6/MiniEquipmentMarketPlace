using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using MiniEquipmentMarketplace.Data;
using Microsoft.Extensions.DependencyInjection;
using MiniEquipmentMarketplace.Models;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.AspNetCore.Identity.UI.Services;
using MiniEquipmentMarketplace.Services;
using MiniEquipmentMarketplace.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApplicationCookie(options =>
{
    // Force cookie on HTTPS and HTTP for dev
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;    
    options.Cookie.SameSite     = SameSiteMode.None;           
    options.Cookie.Path         = "/";                         
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
    );
// Swagger for testing:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MiniEquipmentMarketplace API", Version = "v1" });
});

// Build connection string with environment variable substitution for production
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (builder.Environment.IsProduction() && !string.IsNullOrEmpty(connectionString))
{
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    if (!string.IsNullOrEmpty(dbPassword))
    {
        connectionString = connectionString.Replace("DB_PASSWORD_PLACEHOLDER", dbPassword);
    }
    else
    {
        // Log warning but continue - this will help identify if env var is missing
        var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
        logger.LogWarning("DB_PASSWORD environment variable not found. Using placeholder.");
    }
}

// Configure email settings with SendGrid API key substitution
builder.Services.Configure<EmailSettings>(options =>
{
    builder.Configuration.GetSection("EmailSettings").Bind(options);
    
    // Replace SendGrid API key placeholder in production
    if (builder.Environment.IsProduction())
    {
        var sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        if (!string.IsNullOrEmpty(sendGridApiKey))
        {
            options.Password = sendGridApiKey;
        }
        else
        {
            var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
            logger.LogWarning("SENDGRID_API_KEY environment variable not found. Email functionality may not work.");
        }
    }
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        }));


builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false; // Keep this false for easier testing
    options.SignIn.RequireConfirmedEmail = false;   // But enable emails for notifications
    options.User.RequireUniqueEmail = true;
})
  .AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddRazorPages();  // for Identity UI

builder.Services.AddTransient<IEmailSender, EmailSender>();

// Add HttpContextAccessor for accessing the HttpContext in views
builder.Services.AddHttpContextAccessor();

builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics();


var app = builder.Build();

// error-handling & HTTPS  
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 1) Authenticate before anything secured
app.UseAuthentication();

// 2) Authorize after authentication
app.UseAuthorization();

// 3) Swagger (optional) and API endpoints
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

// 4) MVC & Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed sample data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Ensuring database exists and is up to date");
        logger.LogInformation($"Connection string (masked): {connectionString?.Substring(0, Math.Min(50, connectionString.Length))}...");
        
        // Test database connection
        var canConnect = await db.Database.CanConnectAsync();
        logger.LogInformation($"Database connection test: {(canConnect ? "SUCCESS" : "FAILED")}");
        
        if (!canConnect)
        {
            logger.LogError("Cannot connect to database. Skipping seeding.");
        }
        else
        {
            // For production, ensure the database exists and run migrations
            if (app.Environment.IsProduction())
            {
                db.Database.EnsureCreated();
                logger.LogInformation("Database ensured to exist in production");
            }
            
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Applying {pendingMigrations.Count()} pending migrations");
                await db.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully");
            }
            else
            {
                logger.LogInformation("No pending migrations");
            }

            logger.LogInformation("Database migration completed successfully. Checking for seed data.");
            
            if (!db.Vendors.Any())
            {
                logger.LogInformation("Seeding vendors and equipment data");
                
                // Create vendors
                var acme = new Vendor { Name = "Acme Corp", Email = "info@acme.com" };
                var global = new Vendor { Name = "Global Equip", Email = "sales@global.com" };
                var caterpillar = new Vendor { Name = "Caterpillar Inc.", Email = "contact@caterpillar.com" };
                var johnDeere = new Vendor { Name = "John Deere", Email = "support@johndeere.com" };
                var komatsu = new Vendor { Name = "Komatsu Ltd", Email = "sales@komatsu.com" };
                var volvo = new Vendor { Name = "Volvo Construction", Email = "info@volvo-ce.com" };
                var hitachi = new Vendor { Name = "Hitachi Construction", Email = "service@hitachi-construction.com" };

                db.Vendors.AddRange(acme, global, caterpillar, johnDeere, komatsu, volvo, hitachi);
                
                // Create equipment
                db.Equipment.AddRange(
                    new Equipment { Title = "Bulldozer", Description = "Heavy duty earthmover with blade", Price = 150000m, Vendor = acme },
                    new Equipment { Title = "Excavator", Description = "Hydraulic arm with bucket attachment", Price = 200000m, Vendor = global },
                    new Equipment { Title = "Wheel Loader", Description = "Front-loading shovel machine", Price = 175000m, Vendor = caterpillar },
                    new Equipment { Title = "Backhoe", Description = "Digging machine with front shovel", Price = 120000m, Vendor = johnDeere },
                    new Equipment { Title = "Motor Grader", Description = "Road construction and maintenance", Price = 220000m, Vendor = komatsu },
                    new Equipment { Title = "Dump Truck", Description = "Large-capacity material transport", Price = 180000m, Vendor = volvo },
                    new Equipment { Title = "Crane", Description = "Mobile heavy lifting equipment", Price = 350000m, Vendor = hitachi },
                    new Equipment { Title = "Skid Steer", Description = "Compact and maneuverable loader", Price = 85000m, Vendor = acme },
                    new Equipment { Title = "Trencher", Description = "Digging trenches for piping", Price = 95000m, Vendor = global },
                    new Equipment { Title = "Compactor", Description = "Soil and asphalt compaction", Price = 110000m, Vendor = caterpillar },
                    new Equipment { Title = "Paver", Description = "Asphalt laying machine", Price = 140000m, Vendor = johnDeere },
                    new Equipment { Title = "Concrete Mixer", Description = "Mobile concrete batching", Price = 75000m, Vendor = komatsu },
                    new Equipment { Title = "Telehandler", Description = "Versatile lifting and placing", Price = 130000m, Vendor = volvo },
                    new Equipment { Title = "Articulated Hauler", Description = "Off-road material transport", Price = 240000m, Vendor = hitachi },
                    new Equipment { Title = "Scraper", Description = "Earthmoving and leveling", Price = 280000m, Vendor = acme }
                );
                
                await db.SaveChangesAsync();
                logger.LogInformation("Seed data added successfully");
            }
            else
            {
                logger.LogInformation($"Vendors already exist ({await db.Vendors.CountAsync()} vendors, {await db.Equipment.CountAsync()} equipment items)");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Seed roles and default admin user with enhanced error handling
using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Starting Identity seeding process");
        
        // Ensure roles exist
        var requiredRoles = new[] { "Admin", "Vendor", "Shopper" };
        foreach (var role in requiredRoles)
        {
            if (!await roleMgr.RoleExistsAsync(role))
            {
                var result = await roleMgr.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                {
                    logger.LogInformation($"Created role: {role}");
                }
                else
                {
                    logger.LogError($"Failed to create role {role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        // Create default admin user (always attempt, not tied to vendor seeding)
        const string adminEmail = "admin@demo.com", adminPass = "P@ssw0rd!";
        var existingAdmin = await userMgr.FindByEmailAsync(adminEmail);
        
        if (existingAdmin == null)
        {
            logger.LogInformation($"Creating admin user: {adminEmail}");
            var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var createResult = await userMgr.CreateAsync(admin, adminPass);
            
            if (createResult.Succeeded)
            {
                var roleResult = await userMgr.AddToRoleAsync(admin, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Admin user created and assigned to Admin role successfully");
                }
                else
                {
                    logger.LogError($"Failed to assign Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogError($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            logger.LogInformation($"Admin user already exists: {adminEmail}");
            // Ensure admin user has the Admin role
            if (!await userMgr.IsInRoleAsync(existingAdmin, "Admin"))
            {
                await userMgr.AddToRoleAsync(existingAdmin, "Admin");
                logger.LogInformation("Added Admin role to existing admin user");
            }
        }
        
        // Create test users with multiple roles for demonstration
        const string vendorShopperEmail = "dual@demo.com", dualPass = "P@ssw0rd!";
        if (await userMgr.FindByEmailAsync(vendorShopperEmail) == null)
        {
            logger.LogInformation("Creating a dual-role user (Vendor and Shopper)");
            var dualUser = new IdentityUser { UserName = vendorShopperEmail, Email = vendorShopperEmail, EmailConfirmed = true };
            var result = await userMgr.CreateAsync(dualUser, dualPass);
            
            if (result.Succeeded)
            {
                await userMgr.AddToRoleAsync(dualUser, "Vendor");
                await userMgr.AddToRoleAsync(dualUser, "Shopper");
                logger.LogInformation("Dual-role user created successfully");
            }
            else
            {
                logger.LogWarning("Failed to create dual-role user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        
        // Log summary
        var totalUsers = await userMgr.Users.CountAsync();
        var totalRoles = await roleMgr.Roles.CountAsync();
        logger.LogInformation($"Identity seeding completed. Total users: {totalUsers}, Total roles: {totalRoles}");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the identity data.");
    }
}


app.Run();
