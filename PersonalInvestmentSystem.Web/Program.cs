using PersonalInvestmentSystem.Web.Data;
using Microsoft.SqlServer;
using Microsoft.EntityFrameworkCore;
using PersonalInvestmentSystem.Web.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using PersonalInvestmentSystem.Web.UnitOfWork;
using PersonalInvestmentSystem.Web.Data.Seed;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.Services.Implementations;
using PersonalInvestmentSystem.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using PersonalInvestmentSystem.Services.Implementations;
using PersonalInvestmentSystem.Web.Models;

// Setup logging before anything else
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger("Program");

// Global exception handler for unhandled exceptions
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    var exception = args.ExceptionObject as Exception;
    logger.LogCritical(exception, "UNHANDLED EXCEPTION - App is crashing!");
    Console.WriteLine($"[CRITICAL] UNHANDLED EXCEPTION: {exception?.Message}");
    Console.WriteLine($"[CRITICAL] StackTrace: {exception?.StackTrace}");
};

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)
    ));
    


builder.Services.AddIdentity<AppUser, IdentityRole>(options
    =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
}
    ) 
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IWatchlistService, WatchListService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddMemoryCache();

//google login
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.SignInScheme = IdentityConstants.ExternalScheme;
        });
}

// facebook login
var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
{
    builder.Services.AddAuthentication()
        .AddFacebook(options =>
        {
            options.AppId = facebookAppId;
            options.AppSecret = facebookAppSecret;
            options.SignInScheme = IdentityConstants.ExternalScheme;
        });
}



builder.Services.Configure<MoMoSettings>(builder.Configuration.GetSection("MoMoSettings"));
var app = builder.Build();

var appLogger = app.Services.GetRequiredService<ILogger<Program>>();
appLogger.LogInformation("===== APPLICATION STARTING =====");

// Exception handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        appLogger.LogError(ex, "[MIDDLEWARE] Exception caught during request: {Path}", context.Request.Path);
        Console.WriteLine($"[ERROR] Exception: {ex.Message}");
        Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
        
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync($"Error: {ex.Message}\n\n{ex.StackTrace}");
        }
    }
});

// app.MapHub<NotificationHub>("/notificationHub"); // TEMPORARILY DISABLED - Testing for crash
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Disable VS Code browser link in development to avoid conflicts
    app.Use(async (context, next) =>
    {
        // Reject browser link requests
        if (context.Request.Path.ToString().Contains("browserLink") || 
            context.Request.Path.ToString().Contains("_vs/"))
        {
            context.Response.StatusCode = 404;
            return;
        }
        await next();
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "area",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


//seed data
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        await DbInitializer.InitializeAsync(services);
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while seeding the database");
//    }
//}

using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    try
    {
        var context = service.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.InitializeAsync(service);
        await SeedData.InitializeAsync(context);

    }
    catch (Exception ex)
    {
        var seedLogger = service.GetRequiredService<ILogger<Program>>();
        seedLogger.LogError(ex, $"An error occurred while seeding the database {ex.Message}.");
    }

}

try
{
    appLogger.LogInformation("===== RUNNING APPLICATION =====");
    app.Run();
}
catch (Exception ex)
{
    appLogger.LogCritical(ex, "[FATAL] Application crashed with exception!");
    Console.WriteLine($"[FATAL] Application crashed: {ex.Message}");
    Console.WriteLine($"[FATAL] StackTrace: {ex.StackTrace}");
    Environment.ExitCode = 1;
}
