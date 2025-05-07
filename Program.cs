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


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));


builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = false)
  .AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddRazorPages();  // for Identity UI

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
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

// Seed the database using the DbInitializer
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Initializing database with DbInitializer");
        await DbInitializer.Initialize(services);
        logger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();
