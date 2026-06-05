using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Menus;

public class IndexModel : AdminPageModel
{
  private readonly MenuDbHelper       _menuDbHelper;
  private readonly TranslationService _translation;

  public List<Menu> AllMenus { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string MsgSortSuccess        { get; set; } = string.Empty;
  public string MsgSortError          { get; set; } = string.Empty;
  public string MsgToggleConfirmTitle { get; set; } = string.Empty;
  public string MsgToggleConfirmText  { get; set; } = string.Empty;
  public string MsgToggleConfirmBtn   { get; set; } = string.Empty;
  public string MsgToggleSuccess      { get; set; } = string.Empty;
  public string MsgToggleError        { get; set; } = string.Empty;
  public string LblDelete             { get; set; } = string.Empty;
  public string LblSaveOrder          { get; set; } = string.Empty;

  public IndexModel(MenuDbHelper menuDbHelper, TranslationService translation)
  {
    _menuDbHelper = menuDbHelper;
    _translation  = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = string.Empty;
    AllMenus = await _menuDbHelper.GetFlatListAsync();
    await LoadMessagesAsync();
  }

  public async Task<IActionResult> OnPostSaveSortAsync([FromBody] List<MenuSortItem> items)
  {
    if (items == null || items.Count == 0)
      return new JsonResult(new { success = false });

    try
    {
      await _menuDbHelper.SaveSortOrderAsync(items, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostToggleStatusAsync(int id)
  {
    try
    {
      var menu = await _menuDbHelper.GetByIdAsync(id);
      if (menu == null)
        return new JsonResult(new { success = false, message = MsgToggleError });

      var newStatus = menu.Status == StatusConstants.Active
          ? StatusConstants.Inactive
          : StatusConstants.Active;

      await _menuDbHelper.UpdateStatusAsync(id, newStatus, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
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

  private async Task LoadMessagesAsync()
  {
    MsgDeleteConfirmTitle = await _translation.GetAsync("Confirm.DeleteTitle");
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    MsgSortSuccess        = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    MsgSortError          = await _translation.GetAsync(MessageConstants.SaveError);
    MsgToggleConfirmTitle = await _translation.GetAsync("ToggleStatusTitle");
    MsgToggleConfirmText  = await _translation.GetAsync("ToggleStatusConfirm");
    MsgToggleConfirmBtn   = await _translation.GetAsync("ToggleStatusYes");
    MsgToggleSuccess      = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    MsgToggleError        = await _translation.GetAsync(MessageConstants.SaveError);
    LblDelete             = await _translation.GetAsync("Btn.Delete");
    LblSaveOrder          = await _translation.GetAsync("Menu.SaveOrder");
  }
}
