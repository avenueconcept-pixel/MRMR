using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Categories;

public class CreateModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public CreateModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty] public AwardCategory        Category { get; set; } = new();
    [BindProperty] public List<AwardCriterion> Criteria { get; set; } = [];

    public IActionResult OnGet()
    {
        Criteria = Enumerable.Range(0, 5)
            .Select(_ => new AwardCriterion())
            .ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        Category.CreatedBy = CurrentUserId;
        Category.IsActive  = true;

        try
        {
            await _mrmrDb.CreateCategoryAsync(Category, Criteria);
            TempData["SuccessMessage"] = $"Category '{Category.Name}' created successfully.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = "error";
            return Page();
        }
    }
}
