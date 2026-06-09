using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Services;

namespace MyApp.Areas.Applicant.Pages.Account;

public class ChangePasswordModel : ApplicantPageModel
{
    private readonly RegistrationDbHelper _dbHelper;
    private readonly TranslationService _translation;

    public ChangePasswordModel(RegistrationDbHelper dbHelper, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
    }

    [BindProperty] public string CurrentPassword { get; set; } = string.Empty;
    [BindProperty] public string NewPassword { get; set; } = string.Empty;
    [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;

    public bool IsFirstLogin { get; set; }

    public async Task OnGetAsync()
    {
        IsFirstLogin = User.FindFirst("IsFirstLogin")?.Value == "True";
        await Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostChangeAsync()
    {
        var registrantIdStr = User.FindFirst(MyApp.Constants.CookieConstants.SessionKeys.UserId)?.Value;
        if (!int.TryParse(registrantIdStr, out int registrantId))
            return RedirectToPage("/Login", new { area = "Applicant" });

        IsFirstLogin = User.FindFirst("IsFirstLogin")?.Value == "True";

        var registrant = await _dbHelper.GetRegistrantByDbIdAsync(registrantId);
        if (registrant == null)
            return RedirectToPage("/Login", new { area = "Applicant" });

        if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, registrant.PasswordHash))
        {
            AlertMessageContent = await _translation.GetAsync("ChangePassword.CurrentIncorrect");
            AlertMessageType    = "error";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8)
        {
            AlertMessageContent = await _translation.GetAsync("ChangePassword.TooShort");
            AlertMessageType    = "error";
            return Page();
        }

        if (NewPassword != ConfirmPassword)
        {
            AlertMessageContent = await _translation.GetAsync("ChangePassword.Mismatch");
            AlertMessageType    = "error";
            return Page();
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
        await _dbHelper.UpdatePasswordAsync(registrantId, hash);
        await _dbHelper.ClearFirstLoginAsync(registrantId);

        await HttpContext.SignOutAsync(MyApp.Constants.AuthSchemeConstants.Applicant);
        TempData["PasswordChanged"] = "1";
        return RedirectToPage("/Login", new { area = "Applicant" });
    }
}
