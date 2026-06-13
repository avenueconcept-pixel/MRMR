using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class SummaryModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;

    public SummaryModel(SubmissionDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;

    public Application?             CurrentApplication { get; set; }
    public ApplicationSubmission?   Submission         { get; set; }
    public List<ApplicationDocument> Documents         { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null || !CurrentApplication.IsFinalSubmitted)
            return RedirectToPage("/Dashboard", new { area = "Applicant" });

        Submission = await _dbHelper.GetSubmissionAsync(CurrentApplication.Id);
        Documents  = await _dbHelper.GetApplicationDocumentsAsync(CurrentApplication.Id);

        return Page();
    }
}
