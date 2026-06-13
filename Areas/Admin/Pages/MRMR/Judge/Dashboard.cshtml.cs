using Microsoft.AspNetCore.Mvc;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Judge;

public class DashboardModel : JudgePageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public DashboardModel(AppDbContext db, AdminMrmrDbHelper mrmrDb)
        : base(db)
    {
        _mrmrDb = mrmrDb;
    }

    public List<JudgeCategoryAssignment> Assignments { get; set; } = [];
    public Dictionary<int, int>          AppCounts   { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        Assignments = await _mrmrDb.GetJudgeAssignmentsAsync(CurrentUserId);

        foreach (var a in Assignments)
            AppCounts[a.AwardCategoryId] =
                await _mrmrDb.GetCategoryApplicationCountAsync(a.AwardCategoryId);

        return Page();
    }
}
