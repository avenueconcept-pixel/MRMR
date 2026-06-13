using Microsoft.AspNetCore.Mvc;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Judge;

public class ApplicationsModel : JudgePageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public ApplicationsModel(AppDbContext db, AdminMrmrDbHelper mrmrDb)
        : base(db)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty(SupportsGet = true)] public int CategoryId { get; set; }

    public AwardCategory?                    Category     { get; set; }
    public List<Application>                 Applications { get; set; } = [];
    public Dictionary<int, JudgeEvaluation?> Evaluations  { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var isAssigned = await _mrmrDb.IsJudgeAssignedToCategoryAsync(CurrentUserId, CategoryId);
        if (!isAssigned)
            return RedirectToPage("Dashboard");

        Applications = await _mrmrDb.GetCategoryApplicationsForJudgeAsync(CategoryId);
        Category     = Applications.FirstOrDefault()?.AwardCategory
                       ?? await _mrmrDb.GetCategoryAsync(CategoryId);

        foreach (var app in Applications)
            Evaluations[app.Id] = await _mrmrDb.GetJudgeEvaluationAsync(app.Id, CurrentUserId);

        return Page();
    }
}
