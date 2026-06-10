using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;
using System.Text.Json;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class SectionHModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;
    private readonly TranslationService _translation;

    public SectionHModel(SubmissionDbHelper dbHelper, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId      { get; set; } = string.Empty;
    [BindProperty] public string EsgInitiatives     { get; set; } = string.Empty;
    [BindProperty] public string SocialContribution { get; set; } = string.Empty;

    public ApplicationSubmission? Submission        { get; set; }
    public Application?           CurrentApplication { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);

        var existing = await _dbHelper.GetJsonbSectionAsync(CurrentApplication.Id, "H");
        if (existing?.SectionData != null)
        {
            var doc  = JsonDocument.Parse(existing.SectionData);
            var root = doc.RootElement;
            if (root.TryGetProperty("esgInitiatives",     out var p1)) EsgInitiatives     = p1.GetString() ?? string.Empty;
            if (root.TryGetProperty("socialContribution", out var p2)) SocialContribution = p2.GetString() ?? string.Empty;
        }

        SetViewData(CurrentApplication, Submission);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveDraftAsync() => await SaveAsync(false, false);
    public async Task<IActionResult> OnPostSaveNextAsync()  => await SaveAsync(true,  true);

    private async Task<IActionResult> SaveAsync(bool markComplete, bool redirectToNext)
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        var json = JsonSerializer.Serialize(new
        {
            esgInitiatives     = EsgInitiatives,
            socialContribution = SocialContribution
        });

        await _dbHelper.SaveJsonbSectionAsync(CurrentApplication.Id, "H", json, markComplete);

        // Auto-complete Section I for Individual applicants (Section I is Corporate-only)
        if (CurrentApplication.ApplicationType == "Individual")
        {
            await _dbHelper.SaveJsonbSectionAsync(
                CurrentApplication.Id, "I",
                JsonSerializer.Serialize(new { skipped = true, reason = "Individual application" }),
                markComplete: true);
        }

        if (redirectToNext)
        {
            var nextPage = CurrentApplication.ApplicationType == "Corporate"
                ? "/Submission/SectionI"
                : "/Submission/SectionJ";
            return RedirectToPage(nextPage,
                new { area = "Applicant", applicationId = ApplicationId });
        }

        Submission = await _dbHelper.GetSubmissionAsync(CurrentApplication.Id);
        SetViewData(CurrentApplication, Submission);
        AlertMessageContent = await _translation.GetAsync("Submission.DraftSaved");
        AlertMessageType    = "success";
        return Page();
    }

    private void SetViewData(Application app, ApplicationSubmission? sub)
    {
        ViewData["ApplicationId"]   = ApplicationId;
        ViewData["CurrentSection"]  = "SectionH";
        ViewData["Submission"]      = sub;
        ViewData["ApplicationType"] = app.ApplicationType;
    }
}
