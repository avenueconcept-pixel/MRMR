using MyApp.Constants;
using MyApp.Helper.DB;
using System.Security.Claims;

namespace MyApp.Middleware;

public class SessionTrackingMiddleware
{
  private readonly RequestDelegate _next;

  public SessionTrackingMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context, UserSessionDbHelper sessionDbHelper)
  {
    if (context.User.Identity?.IsAuthenticated == true)
    {
      var sessionToken = context.User.FindFirstValue(CookieConstants.SessionKeys.SessionToken);
      var currentPage  = context.Request.Path.Value ?? string.Empty;

      if (!string.IsNullOrEmpty(sessionToken) &&
          !currentPage.StartsWith("/vendor")  &&
          !currentPage.StartsWith("/js")      &&
          !currentPage.StartsWith("/css")     &&
          !currentPage.StartsWith("/images")  &&
          !currentPage.StartsWith("/uploads"))
      {
        await sessionDbHelper.UpdateCurrentPageAsync(sessionToken, currentPage);
      }
    }

    await _next(context);
  }
}
