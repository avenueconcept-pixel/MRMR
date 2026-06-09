using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Constants;
using MyApp.Services;
using System.Security.Claims;

namespace MyApp.Areas.Applicant.Pages;

public class LoginModel : BasePageModel
{
    private readonly RegistrationDbHelper _dbHelper;
    private readonly TranslationService _translation;

    public LoginModel(RegistrationDbHelper dbHelper, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
    }

    [BindProperty] public string Username { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            AlertMessageContent = await _translation.GetAsync("Login.FieldsRequired");
            AlertMessageType    = "error";
            return Page();
        }

        var registrant = await _dbHelper.GetRegistrantByUsernameAsync(Username);
        if (registrant == null)
        {
            AlertMessageContent = await _translation.GetAsync("Login.InvalidCredentials");
            AlertMessageType    = "error";
            return Page();
        }

        if (!BCrypt.Net.BCrypt.Verify(Password, registrant.PasswordHash))
        {
            AlertMessageContent = await _translation.GetAsync("Login.InvalidCredentials");
            AlertMessageType    = "error";
            return Page();
        }

        var application = await _dbHelper.GetActiveApplicationAsync(registrant.Id);

        var claims = new List<Claim>
        {
            new(CookieConstants.SessionKeys.UserId,        registrant.Id.ToString()),
            new(CookieConstants.SessionKeys.Username,      registrant.Username ?? registrant.Email),
            new(CookieConstants.SessionKeys.FullName,      registrant.FullName),
            new(CookieConstants.SessionKeys.Email,         registrant.Email),
            new(CookieConstants.SessionKeys.LoginLanguage, registrant.PreferredLang ?? "en"),
        };

        if (application != null)
            claims.Add(new("ApplicationId", application.ApplicationId));

        var identity  = new ClaimsIdentity(claims, AuthSchemeConstants.Applicant);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(AuthSchemeConstants.Applicant, principal,
            new AuthenticationProperties { IsPersistent = true });

        return RedirectToPage("/Dashboard", new { area = "Applicant" });
    }
}
