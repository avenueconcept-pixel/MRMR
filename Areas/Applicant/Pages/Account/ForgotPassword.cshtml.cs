using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Services;

namespace MyApp.Areas.Applicant.Pages.Account;

public class ForgotPasswordModel : BasePageModel
{
    private readonly RegistrationDbHelper _dbHelper;
    private readonly EmailService _email;
    private readonly TranslationService _translation;
    private readonly IConfiguration _config;

    public ForgotPasswordModel(RegistrationDbHelper dbHelper, EmailService email,
        TranslationService translation, IConfiguration config)
    {
        _dbHelper    = dbHelper;
        _email       = email;
        _translation = translation;
        _config      = config;
    }

    [BindProperty] public string Email { get; set; } = string.Empty;
    public bool EmailSent { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostSendLinkAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            AlertMessageContent = await _translation.GetAsync("ForgotPassword.EmailRequired");
            AlertMessageType    = "error";
            return Page();
        }

        var registrant = await _dbHelper.GetRegistrantByEmailAsync(Email.Trim().ToLower());
        if (registrant != null)
        {
            var token     = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            var expiresAt = DateTime.UtcNow.AddMinutes(60);
            await _dbHelper.SetResetTokenAsync(registrant.Id, token, expiresAt);

            var baseUrl   = _config["App:BaseUrl"] ?? "https://yourdomain.com";
            var resetLink = $"{baseUrl}/Applicant/Account/ResetPassword?token={token}";
            var lang      = registrant.PreferredLang ?? "en";

            await _email.SendApplicantForgotPasswordAsync(
                registrant.Email, registrant.FullName, resetLink, lang);
        }

        EmailSent = true;
        return Page();
    }
}
