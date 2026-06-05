using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Announcements;

public class IndexModel : AdminPageModel
{
  private readonly AnnouncementDbHelper _dbHelper;
  private readonly TranslationService   _translation;

  public List<Announcement> Items { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string MsgToggleSuccess      { get; set; } = string.Empty;
  public string MsgToggleError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public IndexModel(AnnouncementDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? AppConstants.DefaultLanguage : CurrentLangCode;
    Items = await _dbHelper.GetAllAsync(langCode);

    MsgDeleteConfirmTitle = await _translation.GetAsync("Confirm.DeleteTitle");
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    MsgToggleSuccess      = await _translation.GetAsync(MessageConstants.SaveSuccess);
    MsgToggleError        = await _translation.GetAsync(MessageConstants.SaveError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] string announcementCode)
  {
    var ann = await _dbHelper.GetByCodeAsync(announcementCode);
    if (ann == null) return new JsonResult(new { success = false });

    var newStatus = ann.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _dbHelper.UpdateStatusAsync(announcementCode, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync([FromForm] string announcementCode)
  {
    try
    {
      await _dbHelper.UpdateStatusAsync(announcementCode, StatusConstants.Deleted, CurrentUsername);
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
