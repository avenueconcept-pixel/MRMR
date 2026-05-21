using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;
using System.Security.Claims;

namespace MyApp.Areas.Admin.Pages
{

  public class LoginModel : PageModel
  {
    private readonly TranslationService _translation;
    private readonly AdminDbHelper _adminDbHelper;

    [TempData]
    public string? AlertMessageType { get; set; } = null;

    [TempData]
    public string? AlertMessageTitle { get; set; } = null;

    [TempData]
    public string? AlertMessageContent { get; set; } = null;

    [TempData]
    public string? DefaultUsername { get; set; }

    [BindProperty]
    public string? txtUsername { get; set; }

    [BindProperty]
    public string? txtPassword { get; set; }

    [BindProperty]
    public string? optSelectedLanguage { get; set; }


    [BindProperty]
    public bool chkRememberMe { get; set; }




    public LoginModel(TranslationService translation, AdminDbHelper adminDbHelper)
    {
      _translation = translation;
      _adminDbHelper = adminDbHelper;
    }

    public void OnGet()
    {
      AlertMessageType = null;
      AlertMessageContent = null;
      AlertMessageTitle = null;
      //
    }


    public async Task<bool> ErrorChecking()
    {
      if (string.IsNullOrEmpty(txtUsername))
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("EnterYourUsername");

        return false;
      }

      if (string.IsNullOrEmpty(txtPassword))
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("EnterYourPassword");

        return false;
      }

      var adminUser = await _adminDbHelper.GetByUsernameAsync(txtUsername);


      if (adminUser == null)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("InvalidUsername");

        return false;

      }

      if (adminUser.Status != UserStatusConstants.Active)
      {

        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("InactiveUsername");

        return false;
      }

      string EnterPasswordHash = PasswordCryptoHelper.Encrypt(txtPassword);

      if (EnterPasswordHash != adminUser.PasswordHash)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("InvalidSignIn");

        return false;

      }


      return true;

    }

    public async Task<IActionResult> OnPostAsync()
    {

      bool CheckStatus = await ErrorChecking();

      if (CheckStatus == false)
      {
        return Page();
      }

      DefaultUsername = txtUsername;

      var adminUser = await _adminDbHelper.GetByUsernameAsync(txtUsername);

      //string EnterPasswordHash = PasswordCryptoHelper.Encrypt(txtPassword);

      //if (EnterPasswordHash == adminUser.PasswordHash)
      //{
      var claims = new List<Claim>
      {
        new Claim(CookieConstants.SessionKeys.UserId, adminUser.Id.ToString()),
        new Claim(CookieConstants.SessionKeys.Username, adminUser.Username),
        new Claim(CookieConstants.SessionKeys.LoginLanguage, optSelectedLanguage ?? AppConstants.DefaultLanguage)
      };

      var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
      var principal = new ClaimsPrincipal(identity);

      await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
          new AuthenticationProperties
          {
            IsPersistent = chkRememberMe,
            ExpiresUtc = DateTime.Now.AddDays(30)
          });

      await _adminDbHelper.UpdateLoginInfoAsync(adminUser.Username, optSelectedLanguage ?? AppConstants.DefaultLanguage);


      AlertMessageType = MessageType.Success;
      AlertMessageTitle = MessageTitle.Success;
      AlertMessageContent = "";
      return RedirectToPage(Routes.AdminDashboard);
    }

  }


}
