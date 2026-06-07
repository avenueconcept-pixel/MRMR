//using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Constants;
using MyApp.Helper.DB;
using MyApp.Middleware;
using MyApp.Services;
using MyApp.Services.Logging;
// using MyApp.Data;
// using MyApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuditConnection")));

builder.Services.AddScoped<AuditHelper>();

builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromMinutes(45);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TranslationService>();

builder.Services.AddScoped<AdminDbHelper>();
builder.Services.AddScoped<CustomerDbHelper>();
builder.Services.AddScoped<EmailTemplateDbHelper>();
builder.Services.AddScoped<LanguageDbHelper>();
builder.Services.AddScoped<PasswordResetTokenDbHelper>();
builder.Services.AddScoped<LogDbHelper>();
builder.Services.AddScoped<CountryDbHelper>();
builder.Services.AddScoped<DepartmentDbHelper>();
builder.Services.AddScoped<PaymentMethodDbHelper>();
builder.Services.AddScoped<ProductCategoryDbHelper>();
builder.Services.AddScoped<AuditLogDbHelper>();
builder.Services.AddScoped<StateDbHelper>();
builder.Services.AddScoped<RegionDbHelper>();
builder.Services.AddScoped<LocationDbHelper>();
builder.Services.AddScoped<MenuDbHelper>();
builder.Services.AddScoped<PermissionDbHelper>();
builder.Services.AddScoped<RoleDbHelper>();
builder.Services.AddScoped<BankDbHelper>();
builder.Services.AddScoped<AdminUserDbHelper>();
builder.Services.AddScoped<TranslationDbHelper>();
builder.Services.AddScoped<UserSessionDbHelper>();
builder.Services.AddScoped<DashboardDbHelper>();
builder.Services.AddScoped<PageAccessDbHelper>();
builder.Services.AddScoped<UomDbHelper>();
builder.Services.AddScoped<PriceTierDbHelper>();
builder.Services.AddScoped<ProductSectionTypeDbHelper>();
builder.Services.AddScoped<AnnouncementDbHelper>();
builder.Services.AddScoped<SystemDbHelper>();
builder.Services.AddScoped<MaintenanceDbHelper>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<AppSettingsDbHelper>();
builder.Services.AddScoped<AppSettingsService>();
builder.Services.AddScoped<ProductDbHelper>();
builder.Services.AddScoped<MemberDbHelper>();
builder.Services.AddScoped<ExchangeRateDbHelper>();
builder.Services.AddScoped<WalletDbHelper>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Logging.AddProvider(new DbLoggerProvider(connectionString));



builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<EmailService>();
builder.Services.AddHostedService<LogCleanupService>();

builder.Services.AddAuthentication()
    .AddCookie(AuthSchemeConstants.Admin, options =>
    {
      options.LoginPath = "/Admin/Login";
      options.ExpireTimeSpan = TimeSpan.FromDays(30);
      options.SlidingExpiration = true;
    })
    .AddCookie(AuthSchemeConstants.Customer, options =>
    {
      options.LoginPath = "/Customer/Login";
      options.ExpireTimeSpan = TimeSpan.FromDays(30);
      options.SlidingExpiration = true;
    });

//builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");


builder.Services.Configure<RequestLocalizationOptions>(options =>
{
  var supportedCultures = new[] { "en", "zh", "ms" };

  options.DefaultRequestCulture = new RequestCulture("en"); // 🌍 Default culture
  options.SetDefaultCulture("en")
         .AddSupportedCultures(supportedCultures)
         .AddSupportedUICultures(supportedCultures);

  // Optional: prioritize cookie and query string over browser settings
  options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});


builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromMinutes(120); // Matches the authentication cookie
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});
var app = builder.Build();




app.UseSession(); // Must be before routing and Razor Pages

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{

  app.UseExceptionHandler("/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseStaticFiles();

app.UseRequestLocalization(); // ✅ Must come before routing

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<MaintenanceMiddleware>();
app.UseMiddleware<SessionTrackingMiddleware>();

// 👇 This sets the default page
//app.MapFallbackToPage("/Jobsheets/AddJobSheet"); // or "/Index", "/Dashboard", etc.


app.MapGet("/", () => Results.Redirect("/Admin/Login"));
app.MapRazorPages();

PasswordCryptoHelper.Configure(app.Configuration);

app.Run();
