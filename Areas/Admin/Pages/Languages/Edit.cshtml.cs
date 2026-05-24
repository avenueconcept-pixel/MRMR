using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Languages;

public class EditModel : AdminPageModel
{
  private readonly LanguageDbHelper   _languageDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtLanguageName { get; set; } = string.Empty;
  [BindProperty] public string txtNativeName   { get; set; } = string.Empty;
  [BindProperty] public int    txtSortOrder    { get; set; }
  [BindProperty] public string ddlStatus       { get; set; } = StatusConstants.Active;

  public int      Id           { get; set; }
  public string   LanguageCode { get; set; } = string.Empty;
  public string   CreatedBy    { get; set; } = string.Empty;
  public DateTime CreatedAt    { get; set; }
  public string   UpdatedBy    { get; set; } = string.Empty;
  public DateTime UpdatedAt    { get; set; }

  public List<SelectListItem> StatusOptions { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";

    var language = await _languageDbHelper.GetByIdAsync(id);
    if (language == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminLanguage);
    }

    Id              = language.Id;
    LanguageCode    = language.LanguageCode;
    txtLanguageName = language.LanguageName;
    txtNativeName   = language.NativeName;
    txtSortOrder    = language.SortOrder;
    ddlStatus       = language.Status;
    CreatedBy       = language.CreatedBy;
    CreatedAt       = language.CreatedAt;
    UpdatedBy       = language.UpdatedBy;
    UpdatedAt       = language.UpdatedAt;
    StatusOptions   = await SelectListHelper.GetStatusOptions(_translation);

    var entityName        = language.LanguageName;
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(int id)
  {
    var language = await _languageDbHelper.GetByIdAsync(id);
    if (language == null)
      return RedirectToPage(Routes.AdminLanguage);

    Id           = id;
    LanguageCode = language.LanguageCode;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    if (string.IsNullOrWhiteSpace(txtLanguageName) ||
        string.IsNullOrWhiteSpace(txtNativeName))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var updated = new Language
    {
      Id           = id,
      LanguageCode = language.LanguageCode,
      LanguageName = txtLanguageName.Trim(),
      NativeName   = txtNativeName.Trim(),
      SortOrder    = txtSortOrder,
      Status       = ddlStatus
    };

    await _languageDbHelper.UpdateAsync(updated, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminLanguage);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _languageDbHelper.UpdateStatusAsync(id, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }
}
