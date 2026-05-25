using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.ProductCategories;

public class IndexModel : AdminPageModel
{
  private readonly ProductCategoryDbHelper _dbHelper;
  private readonly TranslationService      _translation;

  public List<ProductCategory> ProductCategories { get; set; } = new();

  public IndexModel(ProductCategoryDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType  = "";
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    ProductCategories = await _dbHelper.GetAllAsync(langCode);
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] string categoryCode)
  {
    var category = await _dbHelper.GetByCodeAsync(categoryCode);
    if (category == null)
      return new JsonResult(new { success = false });

    var newStatus = category.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _dbHelper.UpdateStatusAsync(categoryCode, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
