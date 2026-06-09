using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Applicant.Pages;

public class DashboardModel : ApplicantPageModel
{
    private readonly RegistrationDbHelper _dbHelper;

    public DashboardModel(RegistrationDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public Application? CurrentApplication { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var registrantIdStr = User.FindFirst(MyApp.Constants.CookieConstants.SessionKeys.UserId)?.Value;
        if (!int.TryParse(registrantIdStr, out int registrantId))
            return RedirectToPage("/Login", new { area = "Applicant" });

        CurrentApplication = await _dbHelper.GetActiveApplicationAsync(registrantId);
        return Page();
    }
}
