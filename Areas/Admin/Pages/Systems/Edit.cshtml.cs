using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Systems;

public class EditModel : AdminPageModel
{
  private readonly SystemDbHelper  _dbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtSystemName { get; set; } = string.Empty;
  [BindProperty] public int    txtSortOrder  { get; set; } = 0;
  [BindProperty] public string ddlStatus     { get; set; } = StatusConstants.Active;

  public string   SystemCode    { get; set; } = string.Empty;
  public string   CreatedBy     { get; set; } = string.Empty;
  public DateTime CreatedAt     { get; set; }
  public string   UpdatedBy     { get; set; } = string.Empty;
  public DateTime UpdatedAt     { get; set; }

  public List<SelectListItem> StatusOptions { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(SystemDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task<IActionResult> OnGetAsync(string systemCode)
  {
    AlertMessageType = "";

    var sys = await _dbHelper.GetByCodeAsync(systemCode);
    if (sys == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminSystems);
    }

    SystemCode    = sys.SystemCode;
    txtSystemName = sys.SystemName;
    txtSortOrder  = sys.SortOrder;
    ddlStatus     = sys.Status;
    CreatedBy     = sys.CreatedBy;
    CreatedAt     = sys.CreatedAt;
    UpdatedBy     = sys.UpdatedBy;
    UpdatedAt     = sys.UpdatedAt;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    await LoadDeleteMessagesAsync(sys.SystemName);
    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string systemCode)
  {
    SystemCode    = systemCode;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    if (string.IsNullOrWhiteSpace(txtSystemName))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);

      var existing = await _dbHelper.GetByCodeAsync(systemCode);
      if (existing != null) await LoadDeleteMessagesAsync(existing.SystemName);
      return Page();
    }

    var system = new AppSystem
    {
      SystemCode = systemCode,
      SystemName = txtSystemName.Trim(),
      SortOrder  = txtSortOrder,
      Status     = ddlStatus
    };

    await _dbHelper.UpdateAsync(system, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    return RedirectToPage(Routes.AdminSystems);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(string systemCode)
  {
    try
    {
      await _dbHelper.UpdateStatusAsync(systemCode, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task LoadDeleteMessagesAsync(string entityName)
  {
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");
  }
}
