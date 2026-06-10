using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;
using System.Text.Json;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class SectionIModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;
    private readonly TranslationService _translation;

    public SectionIModel(SubmissionDbHelper dbHelper, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;

    [BindProperty] public string Revenue2023 { get; set; } = string.Empty;
    [BindProperty] public string Revenue2024 { get; set; } = string.Empty;
    [BindProperty] public string Revenue2025 { get; set; } = string.Empty;

    [BindProperty] public string NetProfit2023 { get; set; } = string.Empty;
    [BindProperty] public string NetProfit2024 { get; set; } = string.Empty;
    [BindProperty] public string NetProfit2025 { get; set; } = string.Empty;

    [BindProperty] public string EmployeeCount       { get; set; } = string.Empty;
    [BindProperty] public string ExportRevenuePct    { get; set; } = string.Empty;
    [BindProperty] public string FinancialHighlights { get; set; } = string.Empty;

    public ApplicationSubmission? Submission        { get; set; }
    public Application?           CurrentApplication { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        if (CurrentApplication.ApplicationType != "Corporate")
            return RedirectToPage("/Submission/SectionJ",
                new { area = "Applicant", applicationId = ApplicationId });

        Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);

        var existing = await _dbHelper.GetJsonbSectionAsync(CurrentApplication.Id, "I");
        if (existing?.SectionData != null)
            LoadFromJson(existing.SectionData);

        SetViewData(CurrentApplication, Submission);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveDraftAsync() => await SaveAsync(false, false);
    public async Task<IActionResult> OnPostSaveNextAsync()  => await SaveAsync(true,  true);

    private async Task<IActionResult> SaveAsync(bool markComplete, bool redirectToNext)
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        if (CurrentApplication.ApplicationType != "Corporate")
            return RedirectToPage("/Submission/SectionJ",
                new { area = "Applicant", applicationId = ApplicationId });

        var json = JsonSerializer.Serialize(new
        {
            revenue = new[]
            {
                new { year = "2023", amount = Revenue2023 },
                new { year = "2024", amount = Revenue2024 },
                new { year = "2025", amount = Revenue2025 }
            },
            netProfit = new[]
            {
                new { year = "2023", amount = NetProfit2023 },
                new { year = "2024", amount = NetProfit2024 },
                new { year = "2025", amount = NetProfit2025 }
            },
            employeeCount       = EmployeeCount,
            exportRevenuePct    = ExportRevenuePct,
            financialHighlights = FinancialHighlights
        });

        await _dbHelper.SaveJsonbSectionAsync(CurrentApplication.Id, "I", json, markComplete);

        if (redirectToNext)
            return RedirectToPage("/Submission/SectionJ",
                new { area = "Applicant", applicationId = ApplicationId });

        Submission = await _dbHelper.GetSubmissionAsync(CurrentApplication.Id);
        SetViewData(CurrentApplication, Submission);
        AlertMessageContent = await _translation.GetAsync("Submission.DraftSaved");
        AlertMessageType    = "success";
        return Page();
    }

    private void LoadFromJson(string jsonData)
    {
        try
        {
            var doc  = JsonDocument.Parse(jsonData);
            var root = doc.RootElement;

            if (root.TryGetProperty("revenue", out var rev))
            {
                Revenue2023 = GetYearAmount(rev, "2023");
                Revenue2024 = GetYearAmount(rev, "2024");
                Revenue2025 = GetYearAmount(rev, "2025");
            }
            if (root.TryGetProperty("netProfit", out var np))
            {
                NetProfit2023 = GetYearAmount(np, "2023");
                NetProfit2024 = GetYearAmount(np, "2024");
                NetProfit2025 = GetYearAmount(np, "2025");
            }
            if (root.TryGetProperty("employeeCount",       out var ec)) EmployeeCount       = ec.GetString() ?? string.Empty;
            if (root.TryGetProperty("exportRevenuePct",    out var ep)) ExportRevenuePct    = ep.GetString() ?? string.Empty;
            if (root.TryGetProperty("financialHighlights", out var fh)) FinancialHighlights = fh.GetString() ?? string.Empty;
        }
        catch { /* ignore parse errors — start blank */ }
    }

    private static string GetYearAmount(JsonElement arr, string year)
    {
        foreach (var item in arr.EnumerateArray())
            if (item.TryGetProperty("year", out var y) && y.GetString() == year)
                if (item.TryGetProperty("amount", out var a)) return a.GetString() ?? string.Empty;
        return string.Empty;
    }

    private void SetViewData(Application app, ApplicationSubmission? sub)
    {
        ViewData["ApplicationId"]   = ApplicationId;
        ViewData["CurrentSection"]  = "SectionI";
        ViewData["Submission"]      = sub;
        ViewData["ApplicationType"] = app.ApplicationType;
    }
}
