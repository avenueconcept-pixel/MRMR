using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;
using System.Security.Claims;

namespace MyApp.Areas.Admin.Pages.Account;

public class ForceChangePasswordModel : AdminPageModel
{
  private readonly AdminUserDbHelper  _dbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtNewPassword     { get; set; } = string.Empty;
  [BindProperty] public string txtConfirmPassword { get; set; } = string.Empty;

  public string MsgErrorTitle            { get; set; } = string.Empty;
  public string MsgSuccessTitle          { get; set; } = string.Empty;
  public string MsgPasswordMismatch      { get; set; } = string.Empty;
  public string MsgPasswordWeak          { get; set; } = string.Empty;
  public string MsgSameAsCurrentPassword { get; set; } = string.Empty;

  public ForceChangePasswordModel(AdminUserDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task<IActionResult> OnGetAsync()
  {
    var isForceChange = User.FindFirstValue(CookieConstants.SessionKeys.IsForceChangePassword);
    if (isForceChange != "true")
      return RedirectToPage(Routes.AdminDashboard);

    await LoadMessagesAsync();
    return Page();
  }

  public async Task<IActionResult> OnPostAsync()
  {
    await LoadMessagesAsync();

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

    var hashed = PasswordCryptoHelper.Encrypt(txtNewPassword);
    await _dbHelper.ForceChangePasswordAsync(CurrentUserId, hashed, CurrentUsername);

    // Re-issue the auth cookie with IsForceChangePassword = false so the
    // guard on this page won't redirect the user back here again.
    var claims = User.Claims.ToList();
    var old    = claims.FirstOrDefault(c => c.Type == CookieConstants.SessionKeys.IsForceChangePassword);
    if (old != null) claims.Remove(old);
    claims.Add(new Claim(CookieConstants.SessionKeys.IsForceChangePassword, "false"));
    var identity  = new ClaimsIdentity(claims, AuthSchemeConstants.Admin);
    var principal = new ClaimsPrincipal(identity);
    await HttpContext.SignInAsync(AuthSchemeConstants.Admin, principal);

    return RedirectToPage(Routes.AdminDashboard);
  }

  private async Task LoadMessagesAsync()
  {
    MsgErrorTitle            = await _translation.GetAsync("Error");
    MsgSuccessTitle          = await _translation.GetAsync("Msg.ChangePasswordSuccess");
    MsgPasswordMismatch      = await _translation.GetAsync("Msg.PasswordMismatch");
    MsgPasswordWeak          = await _translation.GetAsync("Msg.PasswordWeak");
    MsgSameAsCurrentPassword = await _translation.GetAsync("ChangePwd.SameAsCurrentPassword");
  }

  private static bool IsPasswordStrong(string password)
      => password.Length >= 8
      && password.Any(char.IsUpper)
      && password.Any(char.IsLower)
      && password.Any(char.IsDigit);
}
