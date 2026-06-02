using Microsoft.AspNetCore.Authentication;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Areas.Admin.Pages
{
  public class LoginModel : BasePageModel
  {
    private readonly TranslationService _translation;
    private readonly AdminDbHelper      _adminDbHelper;
    private readonly LanguageDbHelper   _languageDbHelper;
    private readonly AuditHelper        _audit;

    public List<Language> Languages { get; set; } = new();
    public string CurrentLang { get; set; } = AppConstants.DefaultLanguage;

    [TempData]
    public string? DefaultUsername { get; set; }

    [BindProperty]
    public string? txtUsername { get; set; }

    [BindProperty]
    public string? txtPassword { get; set; }

    [BindProperty]
    public string? ddlLanguage { get; set; }


    [BindProperty]
    public bool chkRememberMe { get; set; }




    public LoginModel(TranslationService translation, AdminDbHelper adminDbHelper, LanguageDbHelper languageDbHelper, AuditHelper audit)
    {
      _translation      = translation;
      _adminDbHelper    = adminDbHelper;
      _languageDbHelper = languageDbHelper;
      _audit            = audit;
    }

    public async Task OnGetAsync(string? username = null)
    {
      AlertMessageType    = null;
      AlertMessageContent = null;
      AlertMessageTitle   = null;
      CurrentLang         = Request.Cookies["lang"] ?? AppConstants.DefaultLanguage;
      Languages           = await _languageDbHelper.GetAllActiveAsync();

      if (!string.IsNullOrEmpty(username))
        DefaultUsername = username;
    }


    public async Task<AdminUser?> ValidateLoginAsync()
    {
      if (string.IsNullOrEmpty(txtUsername))
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("EnterYourUsername");
        return null;
      }

      if (string.IsNullOrEmpty(txtPassword))
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("EnterYourPassword");
        return null;
      }

      var adminUser = await _adminDbHelper.GetByUsernameAsync(txtUsername);

      if (adminUser == null)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("InvalidUsername");
        return null;
      }

      if (adminUser.Status != StatusConstants.Active)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("InactiveUsername");
        return null;
      }

      if (PasswordCryptoHelper.Encrypt(txtPassword) != adminUser.PasswordHash)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("InvalidSignIn");
        return null;
      }

      return adminUser;
    }

    public async Task<IActionResult> OnPostAsync()
    {
      DefaultUsername = txtUsername;
      Languages = await _languageDbHelper.GetAllActiveAsync();

      var adminUser = await ValidateLoginAsync();

      if (adminUser == null)
      {
        return Page();
      }

      var selectedLang = ddlLanguage ?? AppConstants.DefaultLanguage;

      var claims = new List<Claim>
      {
        new Claim(CookieConstants.SessionKeys.UserId,       adminUser.Id.ToString()),
        new Claim(CookieConstants.SessionKeys.Username,     adminUser.Username),
        new Claim(CookieConstants.SessionKeys.FullName,     adminUser.FullName),
        new Claim(CookieConstants.SessionKeys.LoginLanguage, selectedLang),
        new Claim(CookieConstants.SessionKeys.Timezone,     adminUser.Country?.Timezone ?? "UTC"),
        new Claim(CookieConstants.SessionKeys.RoleId,       adminUser.RoleId.ToString()),
        new Claim(CookieConstants.SessionKeys.IsSuperAdmin, (adminUser.Role?.IsSuperAdmin ?? false) ? "true" : "false")
      };

      var identity = new ClaimsIdentity(claims, AuthSchemeConstants.Admin);
      var principal = new ClaimsPrincipal(identity);

      await HttpContext.SignInAsync(AuthSchemeConstants.Admin, principal,
          new AuthenticationProperties
          {
            IsPersistent = chkRememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
          });

      await _audit.LogLoginAsync(adminUser.Username, AuditConstants.Actions.Login);
      await _adminDbHelper.UpdateLoginInfoAsync(adminUser.Username, selectedLang);

      return RedirectToPage(Routes.AdminDashboard);
    }

  }


}
