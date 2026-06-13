using Microsoft.AspNetCore.Mvc;
using MyApp.Constants.MRMR;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Judge;

public class EvaluateModel : JudgePageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public EvaluateModel(AppDbContext db, AdminMrmrDbHelper mrmrDb)
        : base(db)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty(SupportsGet = true)] public int                          ApplicationId { get; set; }
    [BindProperty(SupportsGet = true)] public int                          CategoryId    { get; set; }
    [BindProperty]                     public List<CriterionScoreInputDto> Scores        { get; set; } = [];
    [BindProperty]                     public string?                      OverallComment { get; set; }
    [BindProperty]                     public string?                      Recommendation { get; set; }

    public Application?     Application { get; set; }
    public JudgeEvaluation? Evaluation  { get; set; }
    public bool             IsSubmitted => Evaluation?.Status == nameof(EvaluationStatus.Submitted);

    public async Task<IActionResult> OnGetAsync()
    {
        var isAssigned = await _mrmrDb.IsJudgeAssignedToCategoryAsync(CurrentUserId, CategoryId);
        if (!isAssigned)
            return RedirectToPage("Dashboard");

        Application = await _mrmrDb.GetApplicationForJudgeAsync(ApplicationId);
        if (Application == null)
            return RedirectToPage("Applications", new { CategoryId });

        Evaluation = await _mrmrDb.GetJudgeEvaluationAsync(ApplicationId, CurrentUserId);

        if (Evaluation != null)
        {
            OverallComment = Evaluation.OverallComment;
            Recommendation = Evaluation.Recommendation;
            Scores = Evaluation.Scores.Select(s => new CriterionScoreInputDto
            {
                CriterionId = s.AwardCriterionId,
                Score       = s.Score,
                Comment     = s.Comment
            }).ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveDraftAsync()
    {
        Application = await _mrmrDb.GetApplicationForJudgeAsync(ApplicationId);
        Evaluation  = await _mrmrDb.GetJudgeEvaluationAsync(ApplicationId, CurrentUserId);

        if (IsSubmitted)
        {
            AlertMessageContent = "Evaluation already submitted and cannot be modified.";
            AlertMessageType    = "error";
            return Page();
        }

        var scoreList = Scores
            .Where(s => s.CriterionId > 0)
            .Select(s => (s.CriterionId, s.Score, s.Comment))
            .ToList();

        await _mrmrDb.SaveEvaluationDraftAsync(
            ApplicationId, CurrentUserId, scoreList, OverallComment, Recommendation);

        TempData["SuccessMessage"] = "Draft saved successfully.";
        return RedirectToPage(new { ApplicationId, CategoryId });
    }

    public async Task<IActionResult> OnPostSubmitAsync()
    {
        Application = await _mrmrDb.GetApplicationForJudgeAsync(ApplicationId);
        Evaluation  = await _mrmrDb.GetJudgeEvaluationAsync(ApplicationId, CurrentUserId);

        if (IsSubmitted)
        {
            AlertMessageContent = "Evaluation already submitted.";
            AlertMessageType    = "error";
            return Page();
        }

        var criteria = Application?.AwardCategory?.Criteria
            .Where(c => c.IsActive).ToList() ?? [];

        foreach (var c in criteria)
        {
            var input = Scores.FirstOrDefault(s => s.CriterionId == c.Id);
            if (input == null || input.Score < 0 || input.Score > 100)
            {
                AlertMessageContent = $"Please enter a valid score (0–100) for: {c.CriterionName}";
                AlertMessageType    = "error";
                return Page();
            }
        }

        var scoreList = Scores
            .Where(s => s.CriterionId > 0)
            .Select(s => (s.CriterionId, s.Score, s.Comment))
            .ToList();

        try
        {
            await _mrmrDb.SubmitEvaluationAsync(
                ApplicationId, CurrentUserId, scoreList, OverallComment, Recommendation);

            TempData["SuccessMessage"] = "Evaluation submitted successfully.";
            return RedirectToPage("Applications", new { CategoryId });
        }
        catch (InvalidOperationException ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = "error";
            return Page();
        }
    }
}
