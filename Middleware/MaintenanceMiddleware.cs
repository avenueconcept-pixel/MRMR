using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using MyApp.Constants;
using MyApp.Services;
using System.Security.Claims;

namespace MyApp.Middleware;

public class MaintenanceMiddleware
{
  private readonly RequestDelegate _next;
  private readonly string          _adminUrlPrefix;
  private readonly string          _applicantUrlPrefix;
  private readonly string          _adminLoginPath;
  private readonly string          _applicantLoginPath;

  private static readonly string[] StaticPrefixes =
    { "/vendor", "/css", "/js", "/images", "/uploads", "/favicon" };

  public MaintenanceMiddleware(RequestDelegate next, IConfiguration config)
  {
    _next               = next;
    var adminPrefix     = config[AppConstants.AdminUrlPrefixConfigKey]     ?? "admin";
    var applicantPrefix = config[AppConstants.ApplicantUrlPrefixConfigKey] ?? "applicant";
    _adminUrlPrefix     = $"/{adminPrefix}";
    _applicantUrlPrefix = $"/{applicantPrefix}";
    _adminLoginPath     = $"/{adminPrefix}/Login";
    _applicantLoginPath = $"/{applicantPrefix}/Login";
  }

  public async Task InvokeAsync(HttpContext context, MaintenanceService maintenanceService)
  {
    var path = context.Request.Path.Value ?? string.Empty;

    if (IsStaticFile(path))
    {
      await _next(context);
      return;
    }

    string systemCode;
    string loginPage;
    string signOutScheme;

    if (path.StartsWith(_adminUrlPrefix, StringComparison.OrdinalIgnoreCase))
    {
      systemCode    = AppConstants.SystemTypeAdmin;
      loginPage     = _adminLoginPath;
      signOutScheme = AuthSchemeConstants.Admin;
    }
    else if (path.StartsWith(_applicantUrlPrefix, StringComparison.OrdinalIgnoreCase))
    {
      systemCode    = AppConstants.SystemTypeCustomer;
      loginPage     = _applicantLoginPath;
      signOutScheme = AuthSchemeConstants.Applicant;
    }
    else
    {
      await _next(context);
      return;
    }

    if (context.User.Identity?.IsAuthenticated != true)
    {
      await _next(context);
      return;
    }

    var langCode = context.Request.Cookies["lang"] ?? AppConstants.DefaultLanguage;
    var status   = await maintenanceService.GetStatusAsync(systemCode, langCode);

    if (!status.IsUnderMaintenance)
    {
      await _next(context);
      return;
    }

    bool isSuperAdmin = context.User.FindFirstValue(CookieConstants.SessionKeys.IsSuperAdmin) == "true";
    if (isSuperAdmin && systemCode == AppConstants.SystemTypeAdmin)
    {
      await _next(context);
      return;
    }

    await context.SignOutAsync(signOutScheme);
    context.Response.Redirect($"{loginPage}?maintenance=1");
  }

  private static bool IsStaticFile(string path)
  {
    if (StaticPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
      return true;

    var ext = Path.GetExtension(path);
    return !string.IsNullOrEmpty(ext);
  }
}
