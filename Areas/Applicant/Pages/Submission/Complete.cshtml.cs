using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class CompleteModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;

    public CompleteModel(SubmissionDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;

    public Application? CurrentApplication { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);

        if (CurrentApplication == null || !CurrentApplication.IsFinalSubmitted)
            return RedirectToPage("/Dashboard", new { area = "Applicant" });

        ViewData["HideSideNav"] = true;
        return Page();
    }
}
