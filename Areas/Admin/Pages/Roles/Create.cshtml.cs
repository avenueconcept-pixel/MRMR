using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Roles;

public class CreateModel : AdminPageModel
{
  private readonly RoleDbHelper       _roleDbHelper;
  private readonly MenuDbHelper       _menuDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string  txtRoleCode     { get; set; } = string.Empty;
  [BindProperty] public string  txtRoleName     { get; set; } = string.Empty;
  [BindProperty] public string  txtDescription  { get; set; } = string.Empty;
  [BindProperty] public bool    chkIsSuperAdmin { get; set; } = false;
  [BindProperty] public string  ddlDataScope    { get; set; } = string.Empty;
  [BindProperty] public string  ddlStatus       { get; set; } = StatusConstants.Active;
  [BindProperty] public List<int>    SelectedMenus { get; set; } = new();
  [BindProperty] public List<string> SelectedPerms { get; set; } = new();

  public List<SelectListItem> StatusOptions    { get; set; } = new();
  public List<SelectListItem> DataScopeOptions { get; set; } = new();
  public List<Menu>           AllMenus         { get; set; } = new();

  public CreateModel(
    RoleDbHelper       roleDbHelper,
    MenuDbHelper       menuDbHelper,
    TranslationService translation)
  {
    _roleDbHelper = roleDbHelper;
    _menuDbHelper = menuDbHelper;
    _translation  = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = string.Empty;
    await BuildOptionsAsync();
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    await BuildOptionsAsync();

    if (string.IsNullOrWhiteSpace(txtRoleCode)
        || string.IsNullOrWhiteSpace(txtRoleName)
        || string.IsNullOrWhiteSpace(ddlDataScope))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var role = new Role
    {
      RoleCode     = txtRoleCode.Trim().ToUpper(),
      RoleName     = txtRoleName.Trim(),
      Description  = string.IsNullOrWhiteSpace(txtDescription) ? null : txtDescription.Trim(),
      IsSuperAdmin = chkIsSuperAdmin,
      DataScope    = ddlDataScope,
      Status       = ddlStatus
    };

    var permissions = ParseSelectedPerms(SelectedPerms);
    var result = await _roleDbHelper.CreateAsync(role, SelectedMenus, permissions, CurrentUsername);

    switch (result)
    {
      case RoleAddResult.DuplicateActive:
        AlertMessageType    = MessageType.Error;
        AlertMessageTitle   = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("MsgRoleDuplicate");
        return Page();

      case RoleAddResult.Restored:
        AlertMessageType    = MessageType.Warning;
        AlertMessageTitle   = MessageTitle.Warning;
        AlertMessageContent = await _translation.GetAsync("MsgRoleRestored");
        break;

      default:
        AlertMessageType    = MessageType.Success;
        AlertMessageTitle   = MessageTitle.Success;
        AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveSuccess);
        break;
    }

    return RedirectToPage(Routes.AdminRole);
  }

  private async Task BuildOptionsAsync()
  {
    StatusOptions    = await SelectListHelper.GetStatusOptions(_translation);
    DataScopeOptions = await SelectListHelper.GetDataScopeOptions(_translation);
    AllMenus         = await _menuDbHelper.GetAllActiveAsync();
  }

  private static List<(int PermissionId, bool IsGranted)> ParseSelectedPerms(List<string> raw)
      => raw
          .Select(s => s.Split(':'))
          .Where(p => p.Length == 2 && int.TryParse(p[0], out _))
          .Select(p => (int.Parse(p[0]), p[1] == "true"))
          .ToList();
}
