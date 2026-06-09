using Microsoft.AspNetCore.Authentication;
using MyApp.Constants;
using MyApp.Services;
using System.Security.Claims;

namespace MyApp.Middleware;

public class MaintenanceMiddleware
{
  private readonly RequestDelegate _next;

  private static readonly string[] StaticPrefixes =
    { "/vendor", "/css", "/js", "/images", "/uploads", "/favicon" };

  public MaintenanceMiddleware(RequestDelegate next)
  {
    _next = next;
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

    if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
    {
      systemCode    = AppConstants.SystemTypeAdmin;
      loginPage     = "/Admin/Login";
      signOutScheme = AuthSchemeConstants.Admin;
    }
    else if (path.StartsWith("/Applicant", StringComparison.OrdinalIgnoreCase))
    {
      systemCode    = AppConstants.SystemTypeCustomer;
      loginPage     = "/Applicant/Login";
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
