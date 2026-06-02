using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.AdminUsers;

public class IndexModel : AdminPageModel
{
  private readonly AdminUserDbHelper  _adminUserDbHelper;
  private readonly TranslationService _translation;

  public List<AdminUser> Items { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string MsgToggleError        { get; set; } = string.Empty;
  public string MsgResetError         { get; set; } = string.Empty;

  public IndexModel(AdminUserDbHelper adminUserDbHelper, TranslationService translation)
  {
    _adminUserDbHelper = adminUserDbHelper;
    _translation       = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    Items = await _adminUserDbHelper.GetAllAsync();

    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {await _translation.GetAsync("Menu.AdminUser")}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    MsgToggleError        = await _translation.GetAsync("ToggleError");
    MsgResetError         = await _translation.GetAsync("AdminUser.ResetPasswordError");
  }

  public async Task<IActionResult> OnPostToggleStatusAsync(int id)
  {
    var user = await _adminUserDbHelper.GetByIdAsync(id);
    if (user == null)
      return new JsonResult(new { success = false });

    var newStatus = user.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _adminUserDbHelper.UpdateStatusAsync(id, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }

  public async Task<IActionResult> OnPostResetPasswordAsync(int id)
  {
    try
    {
      const string defaultPassword = "Admin@1234";
      var hashed = PasswordCryptoHelper.Encrypt(defaultPassword);
      await _adminUserDbHelper.UpdatePasswordAsync(id, hashed, CurrentUsername);
      var msg = await _translation.GetAsync("AdminUser.ResetPasswordSuccess");
      return new JsonResult(new { success = true, message = msg, defaultPassword });
    }
    catch
    {
      var msg = await _translation.GetAsync("AdminUser.ResetPasswordError");
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _adminUserDbHelper.SoftDeleteAsync(id, CurrentUsername);
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
