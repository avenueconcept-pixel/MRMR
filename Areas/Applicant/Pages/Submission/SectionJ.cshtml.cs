using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;
using System.Text.Json;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class SectionJModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;
    private readonly TranslationService _translation;

    public SectionJModel(SubmissionDbHelper dbHelper, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;

    [BindProperty] public string ProposerFullName     { get; set; } = string.Empty;
    [BindProperty] public string ProposerNric         { get; set; } = string.Empty;
    [BindProperty] public string ProposerContactNo    { get; set; } = string.Empty;
    [BindProperty] public string ProposerEmail        { get; set; } = string.Empty;
    [BindProperty] public string ProposerMembershipNo { get; set; } = string.Empty;
    [BindProperty] public string ProposerDesignation  { get; set; } = string.Empty;
    [BindProperty] public string ProposerOrganization { get; set; } = string.Empty;

    [BindProperty] public string SeconderFullName     { get; set; } = string.Empty;
    [BindProperty] public string SeconderNric         { get; set; } = string.Empty;
    [BindProperty] public string SeconderContactNo    { get; set; } = string.Empty;
    [BindProperty] public string SeconderEmail        { get; set; } = string.Empty;
    [BindProperty] public string SeconderMembershipNo { get; set; } = string.Empty;
    [BindProperty] public string SeconderDesignation  { get; set; } = string.Empty;
    [BindProperty] public string SeconderOrganization { get; set; } = string.Empty;

    public ApplicationSubmission? Submission        { get; set; }
    public Application?           CurrentApplication { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);

        var existing = await _dbHelper.GetJsonbSectionAsync(CurrentApplication.Id, "J");
        if (existing?.SectionData != null) LoadFromJson(existing.SectionData);

        SetViewData(CurrentApplication, Submission);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveDraftAsync() => await SaveAsync(false, false);
    public async Task<IActionResult> OnPostSaveNextAsync()  => await SaveAsync(true,  true);

    private async Task<IActionResult> SaveAsync(bool markComplete, bool redirectToNext)
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        bool validationFailed = false;
        if (markComplete)
        {
            if (string.IsNullOrWhiteSpace(ProposerFullName) || string.IsNullOrWhiteSpace(ProposerNric)
             || string.IsNullOrWhiteSpace(SeconderFullName) || string.IsNullOrWhiteSpace(SeconderNric))
            {
                validationFailed = true;
                markComplete     = false;
                redirectToNext   = false;
            }
        }

        var json = JsonSerializer.Serialize(new
        {
            proposer = new
            {
                fullName     = ProposerFullName,
                nricPassport = ProposerNric,
                contactNo    = ProposerContactNo,
                email        = ProposerEmail,
                membershipNo = ProposerMembershipNo,
                designation  = ProposerDesignation,
                organization = ProposerOrganization
            },
            seconder = new
            {
                fullName     = SeconderFullName,
                nricPassport = SeconderNric,
                contactNo    = SeconderContactNo,
                email        = SeconderEmail,
                membershipNo = SeconderMembershipNo,
                designation  = SeconderDesignation,
                organization = SeconderOrganization
            }
        });

        await _dbHelper.SaveJsonbSectionAsync(CurrentApplication.Id, "J", json, markComplete);

        if (redirectToNext)
            return RedirectToPage("/Submission/SectionK",
                new { area = "Applicant", applicationId = ApplicationId });

        Submission = await _dbHelper.GetSubmissionAsync(CurrentApplication.Id);
        SetViewData(CurrentApplication, Submission);

        if (validationFailed)
        {
            AlertMessageContent = await _translation.GetAsync("Submission.SectionJ.RequiredError");
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

            if (root.TryGetProperty("proposer", out var p))
            {
                ProposerFullName     = p.GetStringOrEmpty("fullName");
                ProposerNric         = p.GetStringOrEmpty("nricPassport");
                ProposerContactNo    = p.GetStringOrEmpty("contactNo");
                ProposerEmail        = p.GetStringOrEmpty("email");
                ProposerMembershipNo = p.GetStringOrEmpty("membershipNo");
                ProposerDesignation  = p.GetStringOrEmpty("designation");
                ProposerOrganization = p.GetStringOrEmpty("organization");
            }
            if (root.TryGetProperty("seconder", out var s))
            {
                SeconderFullName     = s.GetStringOrEmpty("fullName");
                SeconderNric         = s.GetStringOrEmpty("nricPassport");
                SeconderContactNo    = s.GetStringOrEmpty("contactNo");
                SeconderEmail        = s.GetStringOrEmpty("email");
                SeconderMembershipNo = s.GetStringOrEmpty("membershipNo");
                SeconderDesignation  = s.GetStringOrEmpty("designation");
                SeconderOrganization = s.GetStringOrEmpty("organization");
            }
        }
        catch { /* start blank on parse error */ }
    }

    private void SetViewData(Application app, ApplicationSubmission? sub)
    {
        ViewData["ApplicationId"]   = ApplicationId;
        ViewData["CurrentSection"]  = "SectionJ";
        ViewData["Submission"]      = sub;
        ViewData["ApplicationType"] = app.ApplicationType;
    }
}

internal static class JsonElementExtensions
{
    public static string GetStringOrEmpty(this JsonElement el, string propertyName)
        => el.TryGetProperty(propertyName, out var prop) ? prop.GetString() ?? string.Empty : string.Empty;
}
