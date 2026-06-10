using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class SectionBModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;
    private readonly TranslationService _translation;

    public SectionBModel(SubmissionDbHelper dbHelper, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;
    [BindProperty] public SubmissionSectionB SectionB { get; set; } = new();

    public ApplicationSubmission? Submission { get; set; }
    public Application? CurrentApplication { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);

        SectionB = await _dbHelper.GetSectionBAsync(CurrentApplication.Id)
            ?? PreFillFromRegistrant(CurrentApplication.Registrant, CurrentApplication.Id);

        SetViewData(CurrentApplication, Submission);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveDraftAsync()
        => await SaveAsync(markComplete: false, redirectToNext: false);

    public async Task<IActionResult> OnPostSaveNextAsync()
        => await SaveAsync(markComplete: true, redirectToNext: true);

    private async Task<IActionResult> SaveAsync(bool markComplete, bool redirectToNext)
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        SectionB.ApplicationId = CurrentApplication.Id;
        await _dbHelper.SaveSectionBAsync(SectionB, markComplete);

        if (redirectToNext)
            return RedirectToPage("/Submission/SectionC",
                new { area = "Applicant", applicationId = ApplicationId });

        Submission = await _dbHelper.GetSubmissionAsync(CurrentApplication.Id);
        SetViewData(CurrentApplication, Submission);
        AlertMessageContent = await _translation.GetAsync("Submission.DraftSaved");
        AlertMessageType    = "success";
        return Page();
    }

    private void SetViewData(Application app, ApplicationSubmission? sub)
    {
        ViewData["ApplicationId"]   = ApplicationId;
        ViewData["CurrentSection"]  = "SectionB";
        ViewData["Submission"]      = sub;
        ViewData["ApplicationType"] = app.ApplicationType;
    }

    private static SubmissionSectionB PreFillFromRegistrant(Registrant reg, int applicationId) => new()
    {
        ApplicationId  = applicationId,
        CompanyName    = reg.CompanyName,
        SsmRegNo       = reg.SsmRegNo,
        Website        = reg.Website,
        Industry       = reg.Industry,
        BusinessNature = reg.BusinessNature,
        Country        = "Malaysia"
    };
}
