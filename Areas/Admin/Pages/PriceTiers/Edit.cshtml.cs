using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.PriceTiers;

public class EditModel : AdminPageModel
{
  private readonly PriceTierDbHelper  _dbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtTierName  { get; set; } = string.Empty;
  [BindProperty] public int    txtSortOrder { get; set; } = 0;
  [BindProperty] public string ddlStatus    { get; set; } = StatusConstants.Active;

  public string              TierCode      { get; set; } = string.Empty;
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

  public EditModel(PriceTierDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task<IActionResult> OnGetAsync(string tierCode)
  {
    AlertMessageType = "";

    var tier = await _dbHelper.GetByCodeAsync(tierCode);
    if (tier == null || tier.Status == StatusConstants.Deleted)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminPriceTier);
    }

    TierCode      = tier.TierCode;
    txtTierName   = tier.TierName;
    txtSortOrder  = tier.SortOrder;
    ddlStatus     = tier.Status;
    CreatedBy     = tier.CreatedBy;
    CreatedAt     = tier.CreatedAt;
    UpdatedBy     = tier.UpdatedBy;
    UpdatedAt     = tier.UpdatedAt;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {tier.TierName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string tierCode)
  {
    TierCode      = tierCode;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    var tier = new PriceTier
    {
      TierCode  = tierCode,
      TierName  = txtTierName.Trim(),
      SortOrder = txtSortOrder,
      Status    = ddlStatus
    };

    await _dbHelper.UpdateAsync(tier, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminPriceTier);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(string tierCode)
  {
    try
    {
      await _dbHelper.UpdateStatusAsync(tierCode, StatusConstants.Deleted, CurrentUsername);
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
