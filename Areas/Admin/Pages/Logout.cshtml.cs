using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyApp.Constants;
using MyApp.Helper.DB;
using System.Security.Claims;

namespace MyApp.Areas.Admin.Pages
{
  public class LogoutModel : PageModel
  {
    private readonly UserSessionDbHelper _sessionDbHelper;

    public LogoutModel(UserSessionDbHelper sessionDbHelper)
    {
      _sessionDbHelper = sessionDbHelper;
    }

    public async Task<IActionResult> OnGetAsync()
    {
      var sessionToken = User.FindFirstValue(CookieConstants.SessionKeys.SessionToken);
      if (!string.IsNullOrEmpty(sessionToken))
        await _sessionDbHelper.EndSessionAsync(sessionToken);

      await HttpContext.SignOutAsync(AuthSchemeConstants.Admin);
      return RedirectToPage(Routes.AdminLogin);
    }
  }
}
