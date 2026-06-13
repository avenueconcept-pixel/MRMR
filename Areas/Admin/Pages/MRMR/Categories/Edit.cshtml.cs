using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Categories;

public class EditModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public EditModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty(SupportsGet = true)] public int                 Id       { get; set; }
    [BindProperty]                     public AwardCategory        Category { get; set; } = new();
    [BindProperty]                     public List<AwardCriterion> Criteria { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        AlertMessageType = string.Empty;
        var cat = await _mrmrDb.GetCategoryAsync(Id);
        if (cat == null) return RedirectToPage("Index");

        Category = cat;
        Criteria = cat.Criteria
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToList();

        while (Criteria.Count < 5)
            Criteria.Add(new AwardCriterion());

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        Category.Id = Id;

        try
        {
            await _mrmrDb.UpdateCategoryAsync(Category, Criteria);
            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = "error";

            var cat = await _mrmrDb.GetCategoryAsync(Id);
            if (cat != null) Category.CriteriaLocked = cat.CriteriaLocked;
            return Page();
        }
    }
}
