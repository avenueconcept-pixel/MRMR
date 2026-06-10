using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class SectionSummary
{
    public string Label      { get; init; } = string.Empty;
    public bool   IsComplete { get; init; }
    public string PagePath   { get; init; } = string.Empty;
    public bool   Hidden     { get; init; }
}

public class SectionMModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;
    private readonly TranslationService _translation;

    public SectionMModel(SubmissionDbHelper dbHelper, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;

    public ApplicationSubmission? Submission        { get; set; }
    public Application?           CurrentApplication { get; set; }
    public List<SectionSummary>   Sections          { get; set; } = [];
    public bool                   AllComplete       { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);
        await BuildSummaryAsync();
        SetViewData();
        return Page();
    }

    public async Task<IActionResult> OnPostFinalSubmitAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        if (CurrentApplication.IsFinalSubmitted)
            return RedirectToPage("/Submission/Complete",
                new { area = "Applicant", applicationId = ApplicationId });

        Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);
        await BuildSummaryAsync();

        if (!AllComplete)
        {
            SetViewData();
            AlertMessageContent = await _translation.GetAsync("Submission.SectionM.IncompleteError");
            AlertMessageType    = "error";
            return Page();
        }

        await _dbHelper.FinalSubmitAsync(CurrentApplication.Id);

        return RedirectToPage("/Submission/Complete",
            new { area = "Applicant", applicationId = ApplicationId });
    }

    private async Task BuildSummaryAsync()
    {
        if (Submission == null || CurrentApplication == null) return;

        bool isCorporate = CurrentApplication.ApplicationType == "Corporate";

        Sections =
        [
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionA"), IsComplete = Submission.SectionAComplete, PagePath = "/Submission/SectionA" },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionB"), IsComplete = Submission.SectionBComplete, PagePath = "/Submission/SectionB" },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionC"), IsComplete = Submission.SectionCComplete, PagePath = "/Submission/SectionC" },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionD"), IsComplete = Submission.SectionDComplete, PagePath = "/Submission/SectionD" },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionE"), IsComplete = Submission.SectionEComplete, PagePath = "/Submission/SectionE" },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionF"), IsComplete = Submission.SectionFComplete, PagePath = "/Submission/SectionF" },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionG"), IsComplete = Submission.SectionGComplete, PagePath = "/Submission/SectionG" },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionH"), IsComplete = Submission.SectionHComplete, PagePath = "/Submission/SectionH" },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionI"), IsComplete = Submission.SectionIComplete, PagePath = "/Submission/SectionI",
                    Hidden = !isCorporate },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionJ"), IsComplete = Submission.SectionJComplete, PagePath = "/Submission/SectionJ" },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionK"), IsComplete = Submission.SectionKComplete, PagePath = "/Submission/SectionK" },
            new() { Label = await _translation.GetAsync("Submission.Nav.SectionL"), IsComplete = Submission.SectionLComplete, PagePath = "/Submission/SectionL" },
        ];

        AllComplete = Sections.Where(s => !s.Hidden).All(s => s.IsComplete);
    }

    private void SetViewData()
    {
        ViewData["ApplicationId"]   = ApplicationId;
        ViewData["CurrentSection"]  = "SectionM";
        ViewData["Submission"]      = Submission;
        ViewData["ApplicationType"] = CurrentApplication?.ApplicationType;
    }
}
