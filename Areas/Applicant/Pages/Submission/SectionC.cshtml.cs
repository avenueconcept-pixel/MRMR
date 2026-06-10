using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class SectionCModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;

    public SectionCModel(SubmissionDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;

    public Application? CurrentApplication { get; set; }
    public ApplicationSubmission? Submission { get; set; }
    public AwardCategory? Category { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);
        Category   = await _dbHelper.GetCategoryWithCriteriaAsync(CurrentApplication.AwardCategoryId);

        if (Submission.SectionCComplete == false)
        {
            await _dbHelper.SaveJsonbSectionAsync(
                CurrentApplication.Id, "C",
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    categoryId   = CurrentApplication.AwardCategoryId,
                    categoryName = Category?.Name,
                    confirmedAt  = DateTime.UtcNow
                }),
                markComplete: true);

            Submission = await _dbHelper.GetSubmissionAsync(CurrentApplication.Id);
        }

        SetViewData(CurrentApplication, Submission);
        return Page();
    }

    public IActionResult OnPostNextAsync()
    {
        return RedirectToPage("/Submission/SectionD",
            new { area = "Applicant", applicationId = ApplicationId });
    }

    private void SetViewData(Application app, ApplicationSubmission? sub)
    {
        ViewData["ApplicationId"]   = ApplicationId;
        ViewData["CurrentSection"]  = "SectionC";
        ViewData["Submission"]      = sub;
        ViewData["ApplicationType"] = app.ApplicationType;
    }
}
