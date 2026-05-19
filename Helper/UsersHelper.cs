using System.Globalization;
using System.Security.Claims;

namespace MyApp.Helper
{
  public static class UsersHelper
  {
    public static Guid GetCurrentLoginUserId(this ClaimsPrincipal user)
    {
      return VarConvert.StringToGuid(user?.FindFirstValue(AppConstants.SessionKeys.UserId));
    }

    public static string GetCurrentLoginUsername(this ClaimsPrincipal user)
    {
      return user?.FindFirstValue(AppConstants.SessionKeys.Username);
    }

    public static string GetCurrentCultureCode()
    {
      string currentCulture = CultureInfo.CurrentCulture.Name;

      if (string.IsNullOrEmpty(currentCulture))
      {
        currentCulture = AppConstants.LanguageOption.DefaultCode;
      }

      return currentCulture;
    }
  }
}
