using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Systems;

public class IndexModel : AdminPageModel
{
  private readonly SystemDbHelper  _dbHelper;
  private readonly TranslationService _translation;

  public List<AppSystem> Items { get; set; } = new();

  public string MsgToggleConfirmTitle { get; set; } = string.Empty;
  public string MsgToggleConfirmText  { get; set; } = string.Empty;
  public string MsgToggleConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgToggleSuccess      { get; set; } = string.Empty;
  public string MsgToggleError        { get; set; } = string.Empty;

  public IndexModel(SystemDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType      = "";
    Items                 = await _dbHelper.GetAllAsync();
    MsgToggleConfirmTitle = await _translation.GetAsync("ToggleStatusTitle");
    MsgToggleConfirmText  = await _translation.GetAsync("ToggleStatusConfirm");
    MsgToggleConfirmBtn   = await _translation.GetAsync("ToggleStatusYes");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgToggleSuccess      = await _translation.GetAsync(MessageConstants.SaveSuccess);
    MsgToggleError        = await _translation.GetAsync(MessageConstants.SaveError);
  }

  public async Task<IActionResult> OnPostToggleStatusAsync(string systemCode)
  {
    var system = await _dbHelper.GetByCodeAsync(systemCode);
    if (system == null)
      return new JsonResult(new { success = false });

    var newStatus = system.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _dbHelper.UpdateStatusAsync(systemCode, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
