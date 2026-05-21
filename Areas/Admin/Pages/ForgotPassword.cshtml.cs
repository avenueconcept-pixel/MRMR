using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Areas.Admin.Pages
{
  public class ForgotPasswordModel : BasePageModel
  {
    private readonly TranslationService _translation;
    private readonly EmailService _emailService;
    private readonly ILogger<ForgotPasswordModel> _logger;
    private readonly AdminDbHelper _adminDbHelper;
    private readonly PasswordResetTokenDbHelper _tokenDbHelper;

    [TempData]
    public string? DefaultUsername { get; set; }

    [BindProperty]
    public string? txtUsername { get; set; }

    public ForgotPasswordModel(TranslationService translation, EmailService emailService, ILogger<ForgotPasswordModel> logger, AdminDbHelper adminDbHelper, PasswordResetTokenDbHelper tokenDbHelper)
    {
      _translation = translation;
      _emailService = emailService;
      _logger = logger;
      _adminDbHelper = adminDbHelper;
      _tokenDbHelper = tokenDbHelper;
    }

    public void OnGet() { }

    public async Task<AdminUser?> ValidateAsync()
    {
      if (string.IsNullOrEmpty(txtUsername))
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("EnterYourUsername");
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

      if (adminUser.Status != UserStatusConstants.Active)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync("InactiveUsername");
        return null;
      }

      return adminUser;
    }

    public async Task<IActionResult> OnPostAsync()
    {
      var adminUser = await ValidateAsync();

      if (adminUser == null)
        return Page();

      DefaultUsername = txtUsername;

      var langCode = string.IsNullOrEmpty(adminUser.LastLoginLangCode)
          ? AppConstants.DefaultLanguage
          : adminUser.LastLoginLangCode;

      var resetToken = await _tokenDbHelper.CreateAsync(UserTypeConstants.Admin, adminUser.Id);

      var resetLink = Url.Page(
          "/ResetPassword",
          null,
          new { area = "Admin", token = resetToken.Token },
          Request.Scheme);

      bool sent = await _emailService.SendAdminForgotPasswordAsync(adminUser.Email, adminUser.FullName, resetLink ?? string.Empty, langCode);

      if (!sent)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveError);
        _logger.LogWarning("Forgot password email template missing for key {Key}, language {Lang}", EmailTemplateConstants.AdminForgotPassword, langCode);
        return Page();
      }

      AlertMessageType = MessageType.Success;
      AlertMessageTitle = MessageTitle.Success;
      AlertMessageContent = await _translation.GetAsync("PasswordEmailSend");
      return Page();
    }
  }
}
