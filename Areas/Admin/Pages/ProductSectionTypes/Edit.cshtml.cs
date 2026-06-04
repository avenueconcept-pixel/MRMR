using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.ProductSectionTypes;

public class EditModel : AdminPageModel
{
  private readonly ProductSectionTypeDbHelper _dbHelper;
  private readonly LanguageDbHelper           _languageDbHelper;
  private readonly TranslationService         _translation;

  [BindProperty] public int    txtSortOrder { get; set; }
  [BindProperty] public string ddlStatus    { get; set; } = StatusConstants.Active;

  public string SectionCode { get; set; } = string.Empty;

  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }

  public List<TranslationInputDto> TranslationInputs { get; set; } = new();
  public List<SelectListItem>      StatusOptions     { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(ProductSectionTypeDbHelper dbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _dbHelper         = dbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task<IActionResult> OnGetAsync(string sectionCode)
  {
    AlertMessageType = "";

    var section = await _dbHelper.GetByCodeAsync(sectionCode);
    if (section == null || section.Status == StatusConstants.Deleted)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminProductSectionType);
    }

    SectionCode       = section.SectionCode;
    txtSortOrder      = section.SortOrder;
    ddlStatus         = section.Status;
    CreatedBy         = section.CreatedBy;
    CreatedAt         = section.CreatedAt;
    UpdatedBy         = section.UpdatedBy;
    UpdatedAt         = section.UpdatedAt;
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(section.Translations.ToList());

    var entityName        = section.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.SectionName ?? sectionCode;
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string sectionCode)
  {
    SectionCode       = sectionCode;
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(null);

    var section = new ProductSectionType
    {
      SectionCode = sectionCode,
      SortOrder   = txtSortOrder,
      Status      = ddlStatus
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new ProductSectionTypeTranslation
        {
          SectionCode  = sectionCode,
          LanguageCode = l.LanguageCode,
          SectionName  = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.SectionName))
        .ToList();

    await _dbHelper.UpdateAsync(section, translations, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminProductSectionType);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(string sectionCode)
  {
    try
    {
      await _dbHelper.UpdateStatusAsync(sectionCode, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task<List<TranslationInputDto>> BuildInputsAsync(IList<ProductSectionTypeTranslation>? existing)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("ProductSectionType.NamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = l.LanguageName,
      Value        = existing != null
          ? existing.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.SectionName ?? string.Empty
          : Request.Form[$"txtName_{l.LanguageCode}"].ToString(),
      Placeholder  = placeholder
    }).ToList();
  }
}
