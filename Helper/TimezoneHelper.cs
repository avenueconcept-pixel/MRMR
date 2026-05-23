using MyApp.Constants;
using System.Security.Claims;

namespace MyApp.Helper;

public static class TimezoneHelper
{
  public static string GetTimezone(ClaimsPrincipal user)
      => user.FindFirstValue(CookieConstants.SessionKeys.Timezone) ?? "UTC";
}
