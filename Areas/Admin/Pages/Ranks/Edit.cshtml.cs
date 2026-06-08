using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Ranks;

public class EditModel : AdminPageModel
{
  private readonly RankDbHelper       _dbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtRankName  { get; set; } = string.Empty;
  [BindProperty] public int    txtSortOrder { get; set; } = 0;
  [BindProperty] public string ddlStatus    { get; set; } = StatusConstants.Active;

  public string               RankCode      { get; set; } = string.Empty;
  public List<SelectListItem> StatusOptions { get; set; } = new();

  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(RankDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task<IActionResult> OnGetAsync(string rankCode)
  {
    AlertMessageType = "";

    var rank = await _dbHelper.GetByCodeAsync(rankCode);
    if (rank == null || rank.Status == StatusConstants.Deleted)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminRank);
    }

    RankCode      = rank.RankCode;
    txtRankName   = rank.RankName;
    txtSortOrder  = rank.SortOrder;
    ddlStatus     = rank.Status;
    CreatedBy     = rank.CreatedBy;
    CreatedAt     = rank.CreatedAt;
    UpdatedBy     = rank.UpdatedBy;
    UpdatedAt     = rank.UpdatedAt;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {rank.RankName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string rankCode)
  {
    RankCode      = rankCode;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    var rank = new Rank
    {
      RankCode  = rankCode,
      RankName  = txtRankName.Trim(),
      SortOrder = txtSortOrder,
      Status    = ddlStatus
    };

    await _dbHelper.UpdateAsync(rank, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminRank);
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
