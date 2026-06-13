using Microsoft.AspNetCore.Mvc;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Evaluation;

public class FinalizeModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public FinalizeModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty(SupportsGet = true)] public int CategoryId { get; set; }

    public AwardCategory?                   Category  { get; set; }
    public List<ApplicationScoreSummaryDto> Summaries { get; set; } = [];

    [BindProperty] public List<FinalizeDecisionDto> Decisions { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        Category = await _mrmrDb.GetCategoryAsync(CategoryId);
        if (Category == null) return RedirectToPage("Index");

        Summaries = await _mrmrDb.GetEvaluationSummaryAsync(CategoryId);

        Decisions = Summaries.Select(s => new FinalizeDecisionDto
        {
            ApplicationId    = s.Application.Id,
            IsApprovedWinner = s.Ranking?.IsApprovedWinner ?? false,
            CommitteeRemarks = s.Ranking?.CommitteeRemarks
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostFinalizeAsync()
    {
        try
        {
            var decisionTuples = Decisions
                .Select(d => (d.ApplicationId, d.IsApprovedWinner, d.CommitteeRemarks))
                .ToList();

            await _mrmrDb.FinalizeRankingsAsync(CategoryId, decisionTuples, CurrentUserId);
            TempData["SuccessMessage"] = "Rankings finalized successfully.";
            return RedirectToPage("Index", new { CategoryId });
        }
        catch (Exception ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = "error";
            Category  = await _mrmrDb.GetCategoryAsync(CategoryId);
            Summaries = await _mrmrDb.GetEvaluationSummaryAsync(CategoryId);
            return Page();
        }
    }
}
