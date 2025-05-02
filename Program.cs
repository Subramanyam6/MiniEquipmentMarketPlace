using Microsoft.EntityFrameworkCore;
using MiniEquipmentMarketplace.Data;
using Microsoft.Extensions.DependencyInjection;
using MiniEquipmentMarketplace.Models;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApplicationCookie(options =>
{
    // Force cookie on HTTPS and HTTP for dev
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;    
    options.Cookie.SameSite     = SameSiteMode.None;           
    options.Cookie.Path         = "/";                         
    // optional: explicitly scope to localhost
    options.Cookie.Domain       = "localhost";
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


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = false)
  .AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddRazorPages();  // for Identity UI

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
    db.Database.Migrate();

    if (!db.Vendors.Any())
    {
        var acme = new Vendor { Name = "Acme Corp", Email = "info@acme.com" };
        var global = new Vendor { Name = "Global Equip", Email = "sales@global.com" };

        db.Vendors.AddRange(acme, global);
        db.Equipment.AddRange(
            new Equipment { Title = "Bulldozer", Description = "Heavy duty", Price = 150000m, Vendor = acme },
            new Equipment { Title = "Excavator", Description = "Hydraulic arm",    Price = 200000m, Vendor = global }
        );
        db.SaveChanges();
    }
}

// Seed roles and default admin user
using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "Vendor" })
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));

    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    const string adminEmail = "admin@demo.com", adminPass = "P@ssw0rd!";
    if (await userMgr.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userMgr.CreateAsync(admin, adminPass);
        await userMgr.AddToRoleAsync(admin, "Admin");
    }
}


app.Run();
