using MyApp.Constants;
using MyApp.Helper.DB;
using MyApp.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace MyApp.Middleware;

public class SessionTrackingMiddleware
{
  private readonly RequestDelegate _next;

  private static readonly string[] ExcludedPrefixes =
  {
    "/vendor", "/js", "/css", "/images", "/uploads", "/favicon"
  };

  private static bool ShouldLog(string path) =>
      !ExcludedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

  public SessionTrackingMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(
      HttpContext           context,
      UserSessionDbHelper   sessionDbHelper,
      PageAccessDbHelper    pageAccessDbHelper)
  {
    var currentPage = context.Request.Path.Value ?? string.Empty;

    if (context.User.Identity?.IsAuthenticated == true)
    {
      var sessionToken = context.User.FindFirstValue(CookieConstants.SessionKeys.SessionToken);

      if (!string.IsNullOrEmpty(sessionToken) && ShouldLog(currentPage))
      {
        await sessionDbHelper.UpdateCurrentPageAsync(sessionToken, currentPage);
      }
    }

    var stopwatch = Stopwatch.StartNew();
    await _next(context);
    stopwatch.Stop();
    var responseTimeMs = (int)stopwatch.ElapsedMilliseconds;

    if (context.User.Identity?.IsAuthenticated == true && ShouldLog(currentPage))
    {
      var username     = context.User.FindFirstValue(CookieConstants.SessionKeys.Username)     ?? string.Empty;
      var fullName     = context.User.FindFirstValue(CookieConstants.SessionKeys.FullName)     ?? string.Empty;
      var userId       = int.TryParse(context.User.FindFirstValue(CookieConstants.SessionKeys.UserId), out var uid) ? uid : 0;
      var sessionToken = context.User.FindFirstValue(CookieConstants.SessionKeys.SessionToken) ?? string.Empty;
      var systemType   = context.User.FindFirstValue(CookieConstants.SessionKeys.SystemType)   ?? AppConstants.SystemTypeAdmin;
      var queryString  = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null;

      await pageAccessDbHelper.LogAsync(new PageAccessHistory
      {
        SystemType   = systemType,
        UserId       = userId,
        Username     = username,
        FullName     = fullName,
        SessionToken = sessionToken,
        PageUrl      = currentPage,
        HttpMethod   = context.Request.Method,
        QueryString  = queryString,
        ResponseTime = responseTimeMs,
        AccessedAt   = DateTime.UtcNow
      });
    }
  }
}
