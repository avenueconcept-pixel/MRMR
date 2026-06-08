using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Ranks;

public class IndexModel : AdminPageModel
{
  private readonly RankDbHelper      _dbHelper;
  private readonly TranslationService _translation;

  public List<Rank> Items { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;
  public string MsgToggleSuccess      { get; set; } = string.Empty;
  public string MsgToggleError        { get; set; } = string.Empty;

  public IndexModel(RankDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    Items = await _dbHelper.GetAllAsync();

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

  public async Task<IActionResult> OnPostToggleStatusAsync(string rankCode)
  {
    var rank = await _dbHelper.GetByCodeAsync(rankCode);
    if (rank == null)
      return new JsonResult(new { success = false });

    var newStatus = rank.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _dbHelper.UpdateStatusAsync(rankCode, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(string rankCode)
  {
    try
    {
      await _dbHelper.UpdateStatusAsync(rankCode, StatusConstants.Deleted, CurrentUsername);
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
