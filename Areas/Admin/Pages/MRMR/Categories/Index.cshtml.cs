using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Categories;

public class IndexModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public IndexModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    public List<AwardCategory> Categories { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        AlertMessageType = string.Empty;
        Categories = await _mrmrDb.GetCategoryListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        try
        {
            await _mrmrDb.DeactivateCategoryAsync(id);
            TempData["SuccessMessage"] = "Category deactivated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToPage();
    }
}
