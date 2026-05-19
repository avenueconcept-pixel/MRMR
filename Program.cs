//using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;
// using MyApp.Data;
// using MyApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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



builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<EmailService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
      options.LoginPath = "/Login";
      options.ExpireTimeSpan = TimeSpan.FromDays(30); // Persistent for 30 days
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

// 👇 This sets the default page
//app.MapFallbackToPage("/Jobsheets/AddJobSheet"); // or "/Index", "/Dashboard", etc.


app.MapRazorPages();


app.Run();
