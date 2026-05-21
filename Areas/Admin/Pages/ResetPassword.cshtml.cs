using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Areas.Admin.Pages
{
  public class ResetPasswordModel : BasePageModel
  {
    private readonly TranslationService _translation;
    private readonly AdminDbHelper _adminDbHelper;
    private readonly PasswordResetTokenDbHelper _tokenDbHelper;

    [BindProperty]
    public string? txtNewPassword { get; set; }

    [BindProperty]
    public string? txtConfirmPassword { get; set; }

    [BindProperty]
    public string? Token { get; set; }

    public ResetPasswordModel(TranslationService translation, AdminDbHelper adminDbHelper, PasswordResetTokenDbHelper tokenDbHelper)
    {
      _translation = translation;
      _adminDbHelper = adminDbHelper;
      _tokenDbHelper = tokenDbHelper;
    }

    public async Task<IActionResult> OnGetAsync(string token)
    {
      var resetToken = await _tokenDbHelper.GetValidTokenAsync(token);

      if (resetToken == null)
      {
        TempData[nameof(AlertMessageType)] = MessageType.Error;
        TempData[nameof(AlertMessageTitle)] = MessageTitle.Error;
        TempData[nameof(AlertMessageContent)] = await _translation.GetAsync("InvalidResetLink");
        return RedirectToPage(Routes.AdminForgotPassword);
      }

      Token = token;
      return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (string.IsNullOrEmpty(txtNewPassword))
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("EnterNewPassword");
        return Page();
      }

      if (txtNewPassword != txtConfirmPassword)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("PasswordMismatch");
        return Page();
      }

      var resetToken = await _tokenDbHelper.GetValidTokenAsync(Token ?? string.Empty);

      if (resetToken == null)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("InvalidResetLink");
        return Page();
      }

      var adminUser = await _adminDbHelper.GetByIdAsync(resetToken.UserId);

      if (adminUser == null)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("InvalidResetLink");
        return Page();
      }

      adminUser.PasswordHash = PasswordCryptoHelper.Encrypt(txtNewPassword);
      await _adminDbHelper.UpdateAsync(adminUser);
      await _tokenDbHelper.MarkUsedAsync(resetToken.Id);

      TempData[nameof(AlertMessageType)] = MessageType.Success;
      TempData[nameof(AlertMessageTitle)] = MessageTitle.Success;
      TempData[nameof(AlertMessageContent)] = await _translation.GetAsync("PasswordResetSuccess");
      return RedirectToPage(Routes.AdminLogin);
    }
  }
}
