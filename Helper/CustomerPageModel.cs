using Microsoft.AspNetCore.Authorization;
using MyApp.Constants;
using System.Security.Claims;

namespace MyApp.Helper;

[Authorize(AuthenticationSchemes = AuthSchemeConstants.Customer)]
public class CustomerPageModel : BasePageModel
{
  public string CurrentLangCode
      => User.FindFirstValue(CookieConstants.SessionKeys.LoginLanguage) ?? string.Empty;

  public string UserTimezone
      => TimezoneHelper.GetTimezone(User);
}
