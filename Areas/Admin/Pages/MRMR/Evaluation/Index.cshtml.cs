using Microsoft.AspNetCore.Mvc;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Evaluation;

public class IndexModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public IndexModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty(SupportsGet = true)] public int CategoryId { get; set; }

    public AwardCategory?                   Category   { get; set; }
    public List<AwardCategory>              Categories { get; set; } = [];
    public List<ApplicationScoreSummaryDto> Summaries  { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        AlertMessageType = string.Empty;
        Categories = await _mrmrDb.GetActiveCategoriesAsync();

        if (CategoryId > 0)
        {
            Category  = await _mrmrDb.GetCategoryAsync(CategoryId);
            Summaries = await _mrmrDb.GetEvaluationSummaryAsync(CategoryId);
        }

        return Page();
    }
}
