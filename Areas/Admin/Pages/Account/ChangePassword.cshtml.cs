using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Account;

public class ChangePasswordModel : AdminPageModel
{
  private readonly AdminUserDbHelper  _dbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtCurrentPassword { get; set; } = string.Empty;
  [BindProperty] public string txtNewPassword     { get; set; } = string.Empty;
  [BindProperty] public string txtConfirmPassword { get; set; } = string.Empty;

  public string MsgErrorTitle            { get; set; } = string.Empty;
  public string MsgSuccessTitle          { get; set; } = string.Empty;
  public string MsgPasswordMismatch      { get; set; } = string.Empty;
  public string MsgPasswordWeak          { get; set; } = string.Empty;
  public string MsgSameAsCurrentPassword { get; set; } = string.Empty;
  public string MsgWrongCurrentPassword  { get; set; } = string.Empty;

  public ChangePasswordModel(AdminUserDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    await LoadMessagesAsync();
  }

  public async Task<IActionResult> OnPostAsync()
  {
    await LoadMessagesAsync();

    var storedHash = await _dbHelper.GetPasswordHashAsync(CurrentUserId);
    if (storedHash == null || PasswordCryptoHelper.Encrypt(txtCurrentPassword) != storedHash)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MsgErrorTitle;
      AlertMessageContent = MsgWrongCurrentPassword;
      return Page();
    }

    if (!IsPasswordStrong(txtNewPassword))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MsgErrorTitle;
      AlertMessageContent = MsgPasswordWeak;
      return Page();
    }

    if (txtNewPassword != txtConfirmPassword)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MsgErrorTitle;
      AlertMessageContent = MsgPasswordMismatch;
      return Page();
    }

    if (PasswordCryptoHelper.Encrypt(txtNewPassword) == storedHash)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MsgErrorTitle;
      AlertMessageContent = MsgSameAsCurrentPassword;
      return Page();
    }

    var hashed = PasswordCryptoHelper.Encrypt(txtNewPassword);
    await _dbHelper.ChangePasswordAsync(CurrentUserId, hashed, CurrentUsername);

    return RedirectToPage(Routes.AdminDashboard);
  }

  private async Task LoadMessagesAsync()
  {
    MsgErrorTitle            = await _translation.GetAsync("Error");
    MsgSuccessTitle          = await _translation.GetAsync("Msg.ChangePasswordSuccess");
    MsgPasswordMismatch      = await _translation.GetAsync("Msg.PasswordMismatch");
    MsgPasswordWeak          = await _translation.GetAsync("Msg.PasswordWeak");
    MsgSameAsCurrentPassword = await _translation.GetAsync("ChangePwd.SameAsCurrentPassword");
    MsgWrongCurrentPassword  = await _translation.GetAsync("ChangePwd.WrongCurrentPassword");
  }

  private static bool IsPasswordStrong(string password)
      => password.Length >= 8
      && password.Any(char.IsUpper)
      && password.Any(char.IsLower)
      && password.Any(char.IsDigit);
}
