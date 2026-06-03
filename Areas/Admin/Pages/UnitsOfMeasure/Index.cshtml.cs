using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.UnitsOfMeasure;

public class IndexModel : AdminPageModel
{
  private readonly UomDbHelper      _uomDbHelper;
  private readonly TranslationService _translation;

  public List<UnitOfMeasure> Items { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;
  public string MsgToggleSuccess      { get; set; } = string.Empty;
  public string MsgToggleError        { get; set; } = string.Empty;

  public IndexModel(UomDbHelper uomDbHelper, TranslationService translation)
  {
    _uomDbHelper = uomDbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    Items = await _uomDbHelper.GetAllAsync(langCode);

    MsgDeleteConfirmTitle = await _translation.GetAsync("Confirm.DeleteTitle");
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");
    MsgToggleSuccess      = await _translation.GetAsync(MessageConstants.SaveSuccess);
    MsgToggleError        = await _translation.GetAsync(MessageConstants.SaveError);
  }

  public async Task<IActionResult> OnPostToggleStatusAsync(string uomCode)
  {
    var uom = await _uomDbHelper.GetByCodeAsync(uomCode);
    if (uom == null)
      return new JsonResult(new { success = false });

    var newStatus = uom.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _uomDbHelper.UpdateStatusAsync(uomCode, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
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
}
