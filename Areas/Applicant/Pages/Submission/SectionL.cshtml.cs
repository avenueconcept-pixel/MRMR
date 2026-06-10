using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;
using System.Text.Json;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class SectionLModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;
    private readonly TranslationService _translation;

    public SectionLModel(SubmissionDbHelper dbHelper, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;

    [BindProperty] public bool Box1 { get; set; }
    [BindProperty] public bool Box2 { get; set; }
    [BindProperty] public bool Box3 { get; set; }
    [BindProperty] public bool Box4 { get; set; }
    [BindProperty] public bool Box5 { get; set; }
    [BindProperty] public bool Box6 { get; set; }

    [BindProperty] public string DeclarantName   { get; set; } = string.Empty;
    [BindProperty] public string Designation     { get; set; } = string.Empty;
    [BindProperty] public string DeclarationDate { get; set; } = string.Empty;

    public ApplicationSubmission? Submission        { get; set; }
    public Application?           CurrentApplication { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);

        var existing = await _dbHelper.GetJsonbSectionAsync(CurrentApplication.Id, "L");
        if (existing?.SectionData != null)
            LoadFromJson(existing.SectionData);
        else
            DeclarationDate = DateTime.Today.ToString("yyyy-MM-dd");

        SetViewData(CurrentApplication, Submission);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveDraftAsync() => await SaveAsync(false);
    public async Task<IActionResult> OnPostSaveNextAsync()  => await SaveAsync(true);

    private async Task<IActionResult> SaveAsync(bool markComplete)
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        bool validationFailed = false;
        if (markComplete)
        {
            bool allChecked = Box1 && Box2 && Box3 && Box4 && Box5 && Box6;
            if (!allChecked || string.IsNullOrWhiteSpace(DeclarantName))
            {
                validationFailed = true;
                markComplete     = false;
            }
        }

        var json = JsonSerializer.Serialize(new
        {
            box1            = Box1,
            box2            = Box2,
            box3            = Box3,
            box4            = Box4,
            box5            = Box5,
            box6            = Box6,
            declarantName   = DeclarantName,
            designation     = Designation,
            declarationDate = DeclarationDate
        });

        await _dbHelper.SaveJsonbSectionAsync(CurrentApplication.Id, "L", json, markComplete);

        if (!validationFailed && markComplete)
            return RedirectToPage("/Submission/SectionM",
                new { area = "Applicant", applicationId = ApplicationId });

        Submission = await _dbHelper.GetSubmissionAsync(CurrentApplication.Id);
        SetViewData(CurrentApplication, Submission);

        if (validationFailed)
        {
            AlertMessageContent = await _translation.GetAsync("Submission.SectionL.RequiredError");
            AlertMessageType    = "error";
        }
        else
        {
            AlertMessageContent = await _translation.GetAsync("Submission.DraftSaved");
            AlertMessageType    = "success";
        }
        return Page();
    }

    private void LoadFromJson(string jsonData)
    {
        try
        {
            var doc  = JsonDocument.Parse(jsonData);
            var root = doc.RootElement;

            Box1 = root.TryGetProperty("box1", out var b1) && b1.GetBoolean();
            Box2 = root.TryGetProperty("box2", out var b2) && b2.GetBoolean();
            Box3 = root.TryGetProperty("box3", out var b3) && b3.GetBoolean();
            Box4 = root.TryGetProperty("box4", out var b4) && b4.GetBoolean();
            Box5 = root.TryGetProperty("box5", out var b5) && b5.GetBoolean();
            Box6 = root.TryGetProperty("box6", out var b6) && b6.GetBoolean();

            DeclarantName   = root.TryGetProperty("declarantName",   out var n)  ? n.GetString()  ?? string.Empty : string.Empty;
            Designation     = root.TryGetProperty("designation",     out var d)  ? d.GetString()  ?? string.Empty : string.Empty;
            DeclarationDate = root.TryGetProperty("declarationDate", out var dt) ? dt.GetString() ?? string.Empty : string.Empty;
        }
        catch { /* start blank on parse error */ }
    }

    private void SetViewData(Application app, ApplicationSubmission? sub)
    {
        ViewData["ApplicationId"]   = ApplicationId;
        ViewData["CurrentSection"]  = "SectionL";
        ViewData["Submission"]      = sub;
        ViewData["ApplicationType"] = app.ApplicationType;
    }
}
