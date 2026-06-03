using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.UnitsOfMeasure;

public class EditModel : AdminPageModel
{
  private readonly UomDbHelper      _uomDbHelper;
  private readonly LanguageDbHelper _languageDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtUomName { get; set; } = string.Empty;
  [BindProperty] public string ddlStatus  { get; set; } = StatusConstants.Active;

  public string                    UomCode           { get; set; } = string.Empty;
  public string                    CreatedBy         { get; set; } = string.Empty;
  public DateTime                  CreatedAt         { get; set; }
  public string                    UpdatedBy         { get; set; } = string.Empty;
  public DateTime                  UpdatedAt         { get; set; }
  public List<TranslationInputDto> TranslationInputs { get; set; } = new();
  public List<SelectListItem>      StatusOptions     { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(UomDbHelper uomDbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _uomDbHelper      = uomDbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task<IActionResult> OnGetAsync(string uomCode)
  {
    AlertMessageType = "";

    var uom = await _uomDbHelper.GetByCodeAsync(uomCode);
    if (uom == null || uom.Status == StatusConstants.Deleted)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminUnitOfMeasure);
    }

    UomCode           = uom.UomCode;
    txtUomName        = uom.UomName;
    ddlStatus         = uom.Status;
    CreatedBy         = uom.CreatedBy;
    CreatedAt         = uom.CreatedAt;
    UpdatedBy         = uom.UpdatedBy;
    UpdatedAt         = uom.UpdatedAt;
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(uom.Translations.ToList());

    var entityName        = uom.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.UomName ?? uomCode;
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string uomCode)
  {
    UomCode           = uomCode;
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(null);

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new UomTranslation
        {
          UomCode      = uomCode,
          LanguageCode = l.LanguageCode,
          UomName      = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.UomName))
        .ToList();

    await _uomDbHelper.UpdateAsync(uomCode, txtUomName.Trim(), ddlStatus, translations, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminUnitOfMeasure);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(string uomCode)
  {
    try
    {
      await _uomDbHelper.UpdateStatusAsync(uomCode, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task<List<TranslationInputDto>> BuildInputsAsync(IList<UomTranslation>? existing)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("UnitOfMeasure.NamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = l.LanguageName,
      Value        = existing != null
          ? existing.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.UomName ?? string.Empty
          : Request.Form[$"txtName_{l.LanguageCode}"].ToString(),
      Placeholder  = placeholder
    }).ToList();
  }
}
