using Microsoft.AspNetCore.Authorization;
using MyApp.Constants;
using System.Security.Claims;

namespace MyApp.Helper;

[Authorize(AuthenticationSchemes = AuthSchemeConstants.Admin)]
public class AdminPageModel : BasePageModel
{
  public int CurrentUserId
      => int.TryParse(User.FindFirstValue(CookieConstants.SessionKeys.UserId), out var id) ? id : 0;

  public string CurrentUsername
      => User.FindFirstValue(CookieConstants.SessionKeys.Username) ?? string.Empty;

  public string CurrentFullName
      => User.FindFirstValue(CookieConstants.SessionKeys.FullName) ?? string.Empty;

  public string CurrentLangCode
      => User.FindFirstValue(CookieConstants.SessionKeys.LoginLanguage) ?? string.Empty;

  public string UserTimezone
      => TimezoneHelper.GetTimezone(User);
}
