using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Departments;

public class EditModel : AdminPageModel
{
  private readonly DepartmentDbHelper _deptDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtDeptName { get; set; } = string.Empty;
  [BindProperty] public string ddlStatus   { get; set; } = StatusConstants.Active;

  public int      Id        { get; set; }
  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }

  public List<SelectListItem> StatusOptions { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(DepartmentDbHelper deptDbHelper, TranslationService translation)
  {
    _deptDbHelper = deptDbHelper;
    _translation  = translation;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";

    var dept = await _deptDbHelper.GetByIdAsync(id);
    if (dept == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminDepartment);
    }

    Id          = dept.Id;
    txtDeptName = dept.DeptName;
    ddlStatus   = dept.Status;
    CreatedBy   = dept.CreatedBy;
    CreatedAt   = dept.CreatedAt;
    UpdatedBy   = dept.UpdatedBy;
    UpdatedAt   = dept.UpdatedAt;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    var entityName        = dept.DeptName;
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
    var dept = await _deptDbHelper.GetByIdAsync(id);
    if (dept == null)
      return RedirectToPage(Routes.AdminDepartment);

    Id = id;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    if (string.IsNullOrWhiteSpace(txtDeptName))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    if (await _deptDbHelper.IsDeptNameExistsAsync(txtDeptName.Trim(), id))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.DuplicateError);
      return Page();
    }

    var updated = new Department
    {
      Id       = id,
      DeptName = txtDeptName.Trim(),
      Status   = ddlStatus
    };

    await _deptDbHelper.UpdateAsync(updated, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminDepartment);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _deptDbHelper.UpdateStatusAsync(id, StatusConstants.Deleted, CurrentUsername);
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
