using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.ProductCategories;

public class EditModel : AdminPageModel
{
  private readonly ProductCategoryDbHelper _dbHelper;
  private readonly LanguageDbHelper        _languageDbHelper;
  private readonly TranslationService      _translation;

  [BindProperty] public string ddlStatus { get; set; } = StatusConstants.Active;

  public string                    CategoryCode          { get; set; } = string.Empty;
  public string                    CreatedBy             { get; set; } = string.Empty;
  public DateTime                  CreatedAt             { get; set; }
  public string                    UpdatedBy             { get; set; } = string.Empty;
  public DateTime                  UpdatedAt             { get; set; }
  public List<TranslationInputDto> TranslationInputs     { get; set; } = new();
  public List<SelectListItem>      StatusOptions         { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(ProductCategoryDbHelper dbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _dbHelper         = dbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task<IActionResult> OnGetAsync(string categoryCode)
  {
    AlertMessageType = "";

    var category = await _dbHelper.GetByCodeAsync(categoryCode);
    if (category == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminProductCategory);
    }

    CategoryCode      = category.CategoryCode;
    ddlStatus         = category.Status;
    CreatedBy         = category.CreatedBy;
    CreatedAt         = category.CreatedAt;
    UpdatedBy         = category.UpdatedBy;
    UpdatedAt         = category.UpdatedAt;
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(category.Translations.ToList());

    var entityName        = category.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.CategoryName ?? categoryCode;
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string categoryCode)
  {
    CategoryCode      = categoryCode;
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(null);

    var category = new ProductCategory
    {
      CategoryCode = categoryCode,
      Status       = ddlStatus
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new ProductCategoryTranslation
        {
          CategoryCode = categoryCode,
          LanguageCode = l.LanguageCode,
          CategoryName = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.CategoryName))
        .ToList();

    await _dbHelper.UpdateAsync(category, translations, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminProductCategory);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(string categoryCode)
  {
    try
    {
      await _dbHelper.UpdateStatusAsync(categoryCode, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  // existingTranslations = null means read from Request.Form (POST error case)
  private async Task<List<TranslationInputDto>> BuildInputsAsync(IList<ProductCategoryTranslation>? existingTranslations)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("ProductCategory.NamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = l.LanguageName,
      Value        = existingTranslations != null
          ? existingTranslations.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.CategoryName ?? string.Empty
          : Request.Form[$"txtName_{l.LanguageCode}"].ToString(),
      Placeholder  = placeholder
    }).ToList();
  }
}
