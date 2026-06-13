using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;
using System.Security.Claims;
//using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Areas.Admin.Pages
{
  public class LoginModel : BasePageModel
  {
    private readonly TranslationService  _translation;
    private readonly AdminDbHelper       _adminDbHelper;
    private readonly LanguageDbHelper    _languageDbHelper;
    private readonly AuditHelper         _audit;
    private readonly UserSessionDbHelper _sessionDbHelper;
    private readonly MaintenanceService  _maintenanceService;
    private readonly AppDbContext        _db;

    public List<Language> Languages            { get; set; } = new();
    public string         CurrentLang          { get; set; } = AppConstants.DefaultLanguage;
    public bool           IsUnderMaintenance   { get; set; }
    public string         MaintenanceMessage   { get; set; } = string.Empty;

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




    public LoginModel(
        TranslationService  translation,
        AdminDbHelper       adminDbHelper,
        LanguageDbHelper    languageDbHelper,
        AuditHelper         audit,
        UserSessionDbHelper sessionDbHelper,
        MaintenanceService  maintenanceService,
        AppDbContext        db)
    {
      _translation        = translation;
      _adminDbHelper      = adminDbHelper;
      _languageDbHelper   = languageDbHelper;
      _audit              = audit;
      _sessionDbHelper    = sessionDbHelper;
      _maintenanceService = maintenanceService;
      _db                 = db;
    }

    public async Task OnGetAsync(string? username = null)
    {
      AlertMessageType    = null;
      AlertMessageContent = null;
      AlertMessageTitle   = null;
      CurrentLang         = Request.Cookies["lang"] ?? AppConstants.DefaultLanguage;
      Languages           = await _languageDbHelper.GetAllActiveAsync();

      var mStatus          = await _maintenanceService.GetStatusAsync(AppConstants.SystemTypeAdmin, CurrentLang);
      IsUnderMaintenance   = mStatus.IsUnderMaintenance;
      MaintenanceMessage   = mStatus.Message;

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
      Languages       = await _languageDbHelper.GetAllActiveAsync();

      var langCode = ddlLanguage ?? AppConstants.DefaultLanguage;
      var mStatus  = await _maintenanceService.GetStatusAsync(AppConstants.SystemTypeAdmin, langCode);
      if (mStatus.IsUnderMaintenance)
      {
        IsUnderMaintenance = true;
        MaintenanceMessage = mStatus.Message;

        var testUser    = await _adminDbHelper.GetByUsernameAsync(txtUsername ?? string.Empty);
        bool isSuperAdmin = testUser?.Role?.IsSuperAdmin ?? false;
        if (!isSuperAdmin)
        {
          AlertMessageType    = MessageType.Error;
          AlertMessageTitle   = MessageTitle.Error;
          AlertMessageContent = await _translation.GetAsync("Maintenance.LoginBlocked");
          return Page();
        }
      }

      var adminUser = await ValidateLoginAsync();

      if (adminUser == null)
      {
        return Page();
      }

      var selectedLang = ddlLanguage ?? AppConstants.DefaultLanguage;

      var sessionToken = Guid.NewGuid().ToString("N");
      var userAgent    = Request.Headers["User-Agent"].ToString();
      var ipAddress    = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

      await _sessionDbHelper.CreateSessionAsync(new UserSession
      {
        SystemType   = AppConstants.SystemTypeAdmin,
        UserId       = adminUser.Id,
        Username     = adminUser.Username,
        FullName     = adminUser.FullName,
        CountryCode  = adminUser.CountryCode,
        SessionToken = sessionToken,
        IpAddress    = ipAddress,
        Browser      = ParseBrowser(userAgent),
        Os           = ParseOs(userAgent),
        DeviceType   = ParseDeviceType(userAgent),
        CurrentPage  = Url.Page(Routes.AdminDashboard, new { area = "Admin" }) ?? string.Empty,
        LastActiveAt = DateTime.UtcNow,
        LoginAt      = DateTime.UtcNow,
        IsActive     = true
      });

      var claims = new List<Claim>
      {
        new Claim(CookieConstants.SessionKeys.UserId,                 adminUser.Id.ToString()),
        new Claim(CookieConstants.SessionKeys.Username,               adminUser.Username),
        new Claim(CookieConstants.SessionKeys.FullName,               adminUser.FullName),
        new Claim(CookieConstants.SessionKeys.LoginLanguage,          selectedLang),
        new Claim(CookieConstants.SessionKeys.Timezone,               adminUser.Country?.Timezone ?? "UTC"),
        new Claim(CookieConstants.SessionKeys.RoleId,                 adminUser.RoleId.ToString()),
        new Claim(CookieConstants.SessionKeys.IsSuperAdmin,           (adminUser.Role?.IsSuperAdmin ?? false) ? "true" : "false"),
        new Claim(CookieConstants.SessionKeys.IsForceChangePassword,  adminUser.IsForceChangePassword ? "true" : "false"),
        new Claim(CookieConstants.SessionKeys.SessionToken,           sessionToken)
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

      if (adminUser.IsForceChangePassword)
        return RedirectToPage(Routes.AdminForceChangePassword);

      var judgeRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleCode == "JUDGE");
      if (judgeRole != null && adminUser.RoleId == judgeRole.Id)
        return RedirectToPage(Routes.AdminMrmrJudgeDashboard);

      return RedirectToPage(Routes.AdminDashboard);
    }

    private static string ParseBrowser(string userAgent)
    {
      if (userAgent.Contains("Edg/"))    return "Edge";
      if (userAgent.Contains("Chrome"))  return "Chrome";
      if (userAgent.Contains("Firefox")) return "Firefox";
      if (userAgent.Contains("Safari"))  return "Safari";
      if (userAgent.Contains("MSIE") || userAgent.Contains("Trident")) return "Internet Explorer";
      return "Unknown";
    }

    private static string ParseOs(string userAgent)
    {
      if (userAgent.Contains("Windows NT")) return "Windows";
      if (userAgent.Contains("Mac OS X"))   return "macOS";
      if (userAgent.Contains("Linux"))      return "Linux";
      if (userAgent.Contains("Android"))    return "Android";
      if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
      return "Unknown";
    }

    private static string ParseDeviceType(string userAgent)
    {
      if (userAgent.Contains("Mobi")) return AppConstants.DeviceTypeMobile;
      if (userAgent.Contains("iPad")) return AppConstants.DeviceTypeTablet;
      return AppConstants.DeviceTypeDesktop;
    }
  }
}
