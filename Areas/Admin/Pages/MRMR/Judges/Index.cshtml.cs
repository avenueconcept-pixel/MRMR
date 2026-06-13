using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Judges;

public class IndexModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public IndexModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    public List<AdminUser>                                    Judges      { get; set; } = [];
    public Dictionary<int, List<JudgeCategoryAssignment>>    Assignments { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        AlertMessageType = string.Empty;
        Judges = await _mrmrDb.GetJudgeListAsync();

        foreach (var judge in Judges)
            Assignments[judge.Id] = await _mrmrDb.GetJudgeAssignmentsAsync(judge.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostDeactivateJudgeAsync(int judgeId)
    {
        try
        {
            await _mrmrDb.DeactivateJudgeAsync(judgeId);
            TempData["SuccessMessage"] = "Judge account deactivated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveAssignmentAsync(int assignmentId)
    {
        try
        {
            await _mrmrDb.DeactivateJudgeAssignmentAsync(assignmentId);
            TempData["SuccessMessage"] = "Category assignment removed.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToPage();
    }
}
