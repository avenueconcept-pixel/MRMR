using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyApp.Constants;

namespace MyApp.Areas.Admin.Pages
{
  public class LogoutModel : PageModel
  {
    public async Task<IActionResult> OnGetAsync()
    {
      await HttpContext.SignOutAsync(AuthSchemeConstants.Admin);
      return RedirectToPage(Routes.AdminLogin);
    }
  }
}
