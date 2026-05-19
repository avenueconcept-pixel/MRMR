using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyApp.Helper;
using MyApp.Models;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.VisualBasic;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;

using static MyApp.Helper.AppConstants;
using MyApp.Data;

namespace MyApp.Areas.Admin.Pages
{

  public class LoginModel : PageModel
  {

    private readonly AppDbContext _context;
    private readonly IDbLocalizer _localizer;
    private readonly SharedHelper _sharedhelper;

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




    public LoginModel(AppDbContext context, IDbLocalizer localizer, SharedHelper sharedHelper)
    {
      _localizer = localizer;
      _context = context;
      _sharedhelper = sharedHelper;

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
        AlertMessageContent = _localizer.Get("EnterYourUsername");

        return false;
      }

      if (string.IsNullOrEmpty(txtPassword))
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = _localizer.Get("EnterYourPassword");

        return false;
      }

      var adminUser = await _sharedhelper.GetAdminUserDataByUsername(txtUsername);


      if (adminUser == null)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = _localizer.Get("InvalidUsername");

        return false;

      }

      if (adminUser.LoginStatus != AppConstants.LoginStatus.Active)
      {

        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = _localizer.Get("InactiveUsername");

        return false;
      }

      string EnterPasswordHash = PasswordCryptoHelper.Encrypt(txtPassword);

      if (EnterPasswordHash != adminUser.PasswordHash)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = _localizer.Get("InvalidSignIn");

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

      var adminUser = await _sharedhelper.GetAdminUserDataByUsername(txtUsername);

      //string EnterPasswordHash = PasswordCryptoHelper.Encrypt(txtPassword);

      //if (EnterPasswordHash == adminUser.PasswordHash)
      //{
      var claims = new List<Claim>
          {
             new Claim(AppConstants.SessionKeys.UserId, adminUser.UserId.ToString()),
             new Claim(AppConstants.SessionKeys.Username, adminUser.Username.ToString()),
              new Claim(AppConstants.SessionKeys.LoginLanguage, optSelectedLanguage) // e.g. "en", "zh", "ms"

            // Add other claims if needed
          };

      var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
      var principal = new ClaimsPrincipal(identity);

      await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
          new AuthenticationProperties
          {
            IsPersistent = chkRememberMe, // ✅ This makes the cookie persistent
            ExpiresUtc = DateTime.UtcNow.AddDays(30)
          });


      // update culture code
      adminUser.CultureCode = optSelectedLanguage;
      await _context.SaveChangesAsync();

      AlertMessageType = MessageType.Success;
      AlertMessageTitle = MessageTitle.Success;
      AlertMessageContent = "";
      return RedirectToPage(AppConstants.Routes.Dashboard);
    }
    //else
    //{
    //  // ❌ Invalid password — show error

    //  AlertMessageType = MessageType.Error;
    //  AlertMessageTitle = MessageTitle.Error;
    //  AlertMessageContent = _localizer.Get("InvalidSignIn");

    //  return Page();
    //}
  //}
}




//public class CoverModel : PageModel
//{
//  public void OnGet() { }
//}
}
