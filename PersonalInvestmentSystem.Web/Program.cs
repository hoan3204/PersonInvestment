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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


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

builder.Services.Configure<MoMoSettings>(builder.Configuration.GetSection("MoMoSettings"));
var app = builder.Build();

app.MapHub<NotificationHub>("/notificationHub");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
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
        var logger = service.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, $"An error occurred while seeding the database {ex.Message}.");
    }

}
app.Run();
