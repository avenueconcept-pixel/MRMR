using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;
using MyApp.Constants;

namespace MyApp.Areas.Admin.Pages
{
  public class ForgotPasswordModel : PageModel
  {
    private readonly TranslationService _translation;

    private readonly EmailService _emailService;
    private readonly ILogger<ForgotPasswordModel> _logger;
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


  


    public ForgotPasswordModel(TranslationService translation, EmailService emailService, ILogger<ForgotPasswordModel> logger, AdminDbHelper adminDbHelper)
    {
      _translation = translation;
      _adminDbHelper = adminDbHelper;

      _emailService = emailService;
      _logger = logger;
    


    }
    public void OnGet()
    {
      //AlertMessageType = null;
      //AlertMessageContent = null;
      //AlertMessageTitle = null;

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

      string? userEmail = "";
      string? fullName = "";
      string? DBPassword = "";
      string currentUserLanguage ="";
      if (adminUser != null)
      {
        userEmail = adminUser.Email;
        fullName = adminUser.FullName;
        DBPassword = PasswordCryptoHelper.Decrypt(adminUser.PasswordHash);
        currentUserLanguage = adminUser.LastLoginLangCode;
      }

      string resetLink = "";// Url.Page("/Admin/ResetPassword", null, new { username = txtUsername }, Request.Scheme);

      bool sent = await _emailService.SendAdminForgotPasswordAsync(userEmail, fullName, resetLink, currentUserLanguage);

      if (!sent)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = "Email template not found for key admin_forgot_password and language " + currentUserLanguage;
        _logger.LogWarning(AlertMessageContent);
        return Page();
      }


      AlertMessageType = MessageType.Success;
      AlertMessageTitle = MessageTitle.Success;
      AlertMessageContent = await _translation.GetAsync("PasswordEmailSend");
      return Page();
      //return RedirectToPage(AppConstants.Routes.ForgotPassword);


    }

  }
  
}
