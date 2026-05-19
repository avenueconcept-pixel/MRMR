using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Areas.Admin.Pages.Account;

public class SetLanguageModel : PageModel
{
  public IActionResult OnPost(string languageCode, string returnUrl)
  {
    // Save language code to cookie
    Response.Cookies.Append("lang", languageCode, new CookieOptions
    {
      Expires = DateTimeOffset.UtcNow.AddYears(1),
      IsEssential = true
    });

    return LocalRedirect(returnUrl ?? "/");
  }
}
