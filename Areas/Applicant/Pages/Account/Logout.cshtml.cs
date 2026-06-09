using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyApp.Constants;

namespace MyApp.Areas.Applicant.Pages.Account;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        await HttpContext.SignOutAsync(AuthSchemeConstants.Applicant);
        return RedirectToPage("/Login", new { area = "Applicant" });
    }
}
