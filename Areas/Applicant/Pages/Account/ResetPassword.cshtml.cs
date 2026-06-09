using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Services;

namespace MyApp.Areas.Applicant.Pages.Account;

public class ResetPasswordModel : BasePageModel
{
    private readonly RegistrationDbHelper _dbHelper;
    private readonly TranslationService _translation;

    public ResetPasswordModel(RegistrationDbHelper dbHelper, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
    }

    [BindProperty(SupportsGet = true)] public string Token { get; set; } = string.Empty;
    [BindProperty] public string NewPassword { get; set; } = string.Empty;
    [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;
    public bool TokenInvalid { get; set; }
    public bool ResetSuccess { get; set; }

    public async Task OnGetAsync()
    {
        if (string.IsNullOrEmpty(Token))
        {
            TokenInvalid = true;
            return;
        }
        var registrant = await _dbHelper.GetRegistrantByResetTokenAsync(Token);
        TokenInvalid = registrant == null;
    }

    public async Task<IActionResult> OnPostResetAsync()
    {
        if (string.IsNullOrEmpty(Token))
            return RedirectToPage("/Account/ForgotPassword", new { area = "Applicant" });

        var registrant = await _dbHelper.GetRegistrantByResetTokenAsync(Token);
        if (registrant == null)
        {
            TokenInvalid = true;
            return Page();
        }

        if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8)
        {
            AlertMessageContent = await _translation.GetAsync("ResetPassword.PasswordTooShort");
            AlertMessageType    = "error";
            return Page();
        }

        if (NewPassword != ConfirmPassword)
        {
            AlertMessageContent = await _translation.GetAsync("ResetPassword.PasswordMismatch");
            AlertMessageType    = "error";
            return Page();
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
        await _dbHelper.ResetPasswordAsync(registrant.Id, hash);

        ResetSuccess = true;
        return Page();
    }
}
