using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Menus;

public class EditModel : AdminPageModel
{
  private readonly MenuDbHelper       _menuDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtMenuName { get; set; } = string.Empty;
  [BindProperty] public string txtMenuUrl  { get; set; } = string.Empty;
  [BindProperty] public string txtMenuIcon { get; set; } = string.Empty;
  [BindProperty] public string ddlLevel    { get; set; } = "1";
  [BindProperty] public int?   ddlParentId { get; set; }
  [BindProperty] public int?   ddlGroupId  { get; set; }
  [BindProperty] public string ddlStatus   { get; set; } = StatusConstants.Active;

  public int      Id       { get; set; }
  public string   MenuCode { get; set; } = string.Empty;

  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }

  public List<SelectListItem> LevelOptions  { get; set; } = new();
  public List<SelectListItem> ParentOptions { get; set; } = new();
  public List<SelectListItem> GroupOptions  { get; set; } = new();
  public List<SelectListItem> StatusOptions { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(MenuDbHelper menuDbHelper, TranslationService translation)
  {
    _menuDbHelper = menuDbHelper;
    _translation  = translation;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = string.Empty;

    var menu = await _menuDbHelper.GetByIdAsync(id);
    if (menu == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminMenu);
    }

    Id          = menu.Id;
    MenuCode    = menu.MenuCode;
    txtMenuName = menu.MenuName;
    txtMenuUrl  = menu.MenuUrl  ?? string.Empty;
    txtMenuIcon = menu.MenuIcon ?? string.Empty;
    ddlLevel    = menu.Level.ToString();
    ddlStatus   = menu.Status;
    ddlParentId = menu.Level == 2 ? menu.ParentId : null;
    ddlGroupId  = menu.Level == 1 ? menu.ParentId : null;
    CreatedBy   = menu.CreatedBy;
    CreatedAt   = menu.CreatedAt;
    UpdatedBy   = menu.UpdatedBy;
    UpdatedAt   = menu.UpdatedAt;

    await BuildOptionsAsync();
    await BuildDeleteMessagesAsync(menu.MenuName);
    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(int id)
  {
    var existing = await _menuDbHelper.GetByIdAsync(id);
    if (existing == null)
      return RedirectToPage(Routes.AdminMenu);

    Id       = id;
    MenuCode = existing.MenuCode;
    await BuildOptionsAsync();

    if (string.IsNullOrWhiteSpace(txtMenuName))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      await BuildDeleteMessagesAsync(existing.MenuName);
      return Page();
    }

    var level = int.TryParse(ddlLevel, out var lvl) ? lvl : 1;
    int? parentId = level == 2 ? ddlParentId : level == 1 ? ddlGroupId : null;

    var menu = new Menu
    {
      Id       = id,
      MenuName = txtMenuName.Trim(),
      MenuUrl  = string.IsNullOrWhiteSpace(txtMenuUrl)  ? null : txtMenuUrl.Trim(),
      MenuIcon = string.IsNullOrWhiteSpace(txtMenuIcon) ? null : txtMenuIcon.Trim(),
      Level    = level,
      ParentId = parentId,
      Status   = ddlStatus
    };

    await _menuDbHelper.UpdateAsync(menu, CurrentUsername);

    TempData["AlertType"]    = MessageType.Success;
    TempData["AlertTitle"]   = MessageTitle.Success;
    TempData["AlertContent"] = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    return RedirectToPage(Routes.AdminMenu);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      if (await _menuDbHelper.HasActiveChildrenAsync(id))
      {
        var childMsg = await _translation.GetAsync("Menu.HasChildren");
        return new JsonResult(new { success = false, message = childMsg });
      }

      await _menuDbHelper.UpdateStatusAsync(id, StatusConstants.Deleted, CurrentUsername);
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
    LevelOptions  = await SelectListHelper.GetMenuLevelOptions(_translation);
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);
    var parents = await _menuDbHelper.GetParentsAsync();
    var groups  = await _menuDbHelper.GetGroupsAsync();
    ParentOptions = SelectListHelper.GetParentMenuOptions(parents, await _translation.GetAsync("Menu.SelectParent"));
    GroupOptions  = SelectListHelper.GetGroupMenuOptions(groups,   await _translation.GetAsync("Menu.SelectGroup"));
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
}
