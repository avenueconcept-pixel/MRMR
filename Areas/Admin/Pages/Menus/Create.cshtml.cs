using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Menus;

public class CreateModel : AdminPageModel
{
  private readonly MenuDbHelper       _menuDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtMenuCode { get; set; } = string.Empty;
  [BindProperty] public string txtMenuName { get; set; } = string.Empty;
  [BindProperty] public string txtMenuUrl  { get; set; } = string.Empty;
  [BindProperty] public string txtMenuIcon { get; set; } = string.Empty;
  [BindProperty] public string ddlLevel    { get; set; } = "1";
  [BindProperty] public int?   ddlParentId { get; set; }
  [BindProperty] public int?   ddlGroupId  { get; set; }
  [BindProperty] public string ddlStatus   { get; set; } = StatusConstants.Active;

  public List<SelectListItem> LevelOptions  { get; set; } = new();
  public List<SelectListItem> ParentOptions { get; set; } = new();
  public List<SelectListItem> GroupOptions  { get; set; } = new();
  public List<SelectListItem> StatusOptions { get; set; } = new();

  public CreateModel(MenuDbHelper menuDbHelper, TranslationService translation)
  {
    _menuDbHelper = menuDbHelper;
    _translation  = translation;
  }

  public async Task OnGetAsync()
  {
    await BuildOptionsAsync();
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    await BuildOptionsAsync();

    if (string.IsNullOrWhiteSpace(txtMenuCode) || string.IsNullOrWhiteSpace(txtMenuName))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var level = int.TryParse(ddlLevel, out var lvl) ? lvl : 1;

    if (level == 1 && ddlGroupId == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync("Menu.GroupRequired");
      return Page();
    }
    if (level == 2 && ddlParentId == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync("Menu.ParentRequired");
      return Page();
    }

    int? parentId = level == 2 ? ddlParentId : level == 1 ? ddlGroupId : null;

    var menu = new Menu
    {
      MenuCode = txtMenuCode.Trim().ToUpper(),
      MenuName = txtMenuName.Trim(),
      MenuUrl  = string.IsNullOrWhiteSpace(txtMenuUrl)  ? null : txtMenuUrl.Trim(),
      MenuIcon = string.IsNullOrWhiteSpace(txtMenuIcon) ? null : txtMenuIcon.Trim(),
      Level    = level,
      ParentId = parentId,
      Status   = ddlStatus
    };

    var result = await _menuDbHelper.CreateAsync(menu, CurrentUsername);

    if (result == MenuAddResult.DuplicateActive)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync("MsgMenuDuplicate");
      return Page();
    }

    TempData["AlertType"]    = MessageType.Success;
    TempData["AlertTitle"]   = MessageTitle.Success;
    TempData["AlertContent"] = result == MenuAddResult.Restored
        ? await _translation.GetAsync("MsgMenuRestored")
        : await _translation.GetAsync(MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminMenu);
  }

  private async Task BuildOptionsAsync()
  {
    LevelOptions  = await SelectListHelper.GetMenuLevelOptions(_translation);
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);
    var parents      = await _menuDbHelper.GetParentsAsync();
    var groups       = await _menuDbHelper.GetGroupsAsync();
    ParentOptions = SelectListHelper.GetParentMenuOptions(parents, await _translation.GetAsync("Menu.SelectParent"));
    GroupOptions  = SelectListHelper.GetGroupMenuOptions(groups,   await _translation.GetAsync("Menu.SelectGroup"));
  }
}
