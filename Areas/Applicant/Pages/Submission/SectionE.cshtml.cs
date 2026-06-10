using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;
using System.Text.Json;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class SectionEModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;
    private readonly TranslationService _translation;

    public SectionEModel(SubmissionDbHelper dbHelper, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;

    [BindProperty] public string KeyAchievements        { get; set; } = string.Empty;
    [BindProperty] public string AwardsRecognition      { get; set; } = string.Empty;
    [BindProperty] public string CommunityContributions { get; set; } = string.Empty;

    public ApplicationSubmission? Submission { get; set; }
    public Application? CurrentApplication { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);

        var existing = await _dbHelper.GetJsonbSectionAsync(CurrentApplication.Id, "E");
        if (existing?.SectionData != null)
        {
            var doc = JsonDocument.Parse(existing.SectionData);
            var root = doc.RootElement;
            if (root.TryGetProperty("keyAchievements", out var p1))        KeyAchievements        = p1.GetString() ?? string.Empty;
            if (root.TryGetProperty("awardsRecognition", out var p2))      AwardsRecognition      = p2.GetString() ?? string.Empty;
            if (root.TryGetProperty("communityContributions", out var p3)) CommunityContributions = p3.GetString() ?? string.Empty;
        }

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

        var json = JsonSerializer.Serialize(new
        {
            keyAchievements        = KeyAchievements,
            awardsRecognition      = AwardsRecognition,
            communityContributions = CommunityContributions
        });

        await _dbHelper.SaveJsonbSectionAsync(CurrentApplication.Id, "E", json, markComplete);

        if (redirectToNext)
            return RedirectToPage("/Submission/SectionF",
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
        ViewData["CurrentSection"]  = "SectionE";
        ViewData["Submission"]      = sub;
        ViewData["ApplicationType"] = app.ApplicationType;
    }
}
