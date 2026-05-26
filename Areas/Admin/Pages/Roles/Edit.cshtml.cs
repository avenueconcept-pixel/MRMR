using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Roles;

public class EditModel : AdminPageModel
{
  private readonly RoleDbHelper       _roleDbHelper;
  private readonly MenuDbHelper       _menuDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string  txtRoleName     { get; set; } = string.Empty;
  [BindProperty] public string  txtDescription  { get; set; } = string.Empty;
  [BindProperty] public bool    chkIsSuperAdmin { get; set; } = false;
  [BindProperty] public string  ddlDataScope    { get; set; } = string.Empty;
  [BindProperty] public string  ddlStatus       { get; set; } = StatusConstants.Active;
  [BindProperty] public List<int>    SelectedMenus { get; set; } = new();
  [BindProperty] public List<string> SelectedPerms { get; set; } = new();

  public int    Id       { get; set; }
  public string RoleCode { get; set; } = string.Empty;

  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }

  public List<SelectListItem> StatusOptions    { get; set; } = new();
  public List<SelectListItem> DataScopeOptions { get; set; } = new();
  public List<Menu>           AllMenus         { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(
    RoleDbHelper       roleDbHelper,
    MenuDbHelper       menuDbHelper,
    TranslationService translation)
  {
    _roleDbHelper = roleDbHelper;
    _menuDbHelper = menuDbHelper;
    _translation  = translation;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = string.Empty;

    var role = await _roleDbHelper.GetByIdAsync(id);
    if (role == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminRole);
    }

    Id              = role.Id;
    RoleCode        = role.RoleCode;
    txtRoleName     = role.RoleName;
    txtDescription  = role.Description ?? string.Empty;
    chkIsSuperAdmin = role.IsSuperAdmin;
    ddlDataScope    = role.DataScope;
    ddlStatus       = role.Status;
    CreatedBy       = role.CreatedBy;
    CreatedAt       = role.CreatedAt;
    UpdatedBy       = role.UpdatedBy;
    UpdatedAt       = role.UpdatedAt;

    SelectedMenus = role.RoleMenus.Select(rm => rm.MenuId).ToList();
    SelectedPerms = role.RolePermissions
        .Select(rp => $"{rp.PermissionId}:{rp.IsGranted.ToString().ToLower()}")
        .ToList();

    await BuildOptionsAsync();
    await BuildDeleteMessagesAsync(role.RoleName);

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(int id)
  {
    var existing = await _roleDbHelper.GetByIdAsync(id);
    if (existing == null)
      return RedirectToPage(Routes.AdminRole);

    Id       = id;
    RoleCode = existing.RoleCode;
    await BuildOptionsAsync();

    if (string.IsNullOrWhiteSpace(txtRoleName) || string.IsNullOrWhiteSpace(ddlDataScope))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      await BuildDeleteMessagesAsync(existing.RoleName);
      return Page();
    }

    var role = new Role
    {
      Id           = id,
      RoleCode     = existing.RoleCode,
      RoleName     = txtRoleName.Trim(),
      Description  = string.IsNullOrWhiteSpace(txtDescription) ? null : txtDescription.Trim(),
      IsSuperAdmin = chkIsSuperAdmin,
      DataScope    = ddlDataScope,
      Status       = ddlStatus
    };

    var permissions = ParseSelectedPerms(SelectedPerms);
    await _roleDbHelper.UpdateAsync(role, SelectedMenus, permissions, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminRole);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _roleDbHelper.UpdateStatusAsync(id, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task BuildOptionsAsync()
  {
    StatusOptions    = await SelectListHelper.GetStatusOptions(_translation);
    DataScopeOptions = await SelectListHelper.GetDataScopeOptions(_translation);
    AllMenus         = await _menuDbHelper.GetAllActiveAsync();
  }

  private async Task BuildDeleteMessagesAsync(string entityName)
  {
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");
  }

  private static List<(int PermissionId, bool IsGranted)> ParseSelectedPerms(List<string> raw)
      => raw
          .Where(s => !string.IsNullOrEmpty(s))
          .Select(s => s.Split(':'))
          .Where(p => p.Length == 2 && int.TryParse(p[0], out _))
          .Select(p => (int.Parse(p[0]), p[1] == "true"))
          .ToList();
}
