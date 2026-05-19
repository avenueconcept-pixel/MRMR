using System;
using System.Collections.Generic;
using MyApp.Helper;
using MyApp.Services;
using MyApp.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static MyApp.Helper.AppConstants;
using MyApp.Data;

namespace MyApp.Areas.Admin.Pages
{
  public class ForgotPasswordModel : PageModel
  {
    private readonly AppDbContext _context;
    private readonly IDbLocalizer _localizer;

    private readonly EmailService _emailService;
    private readonly ILogger<ForgotPasswordModel> _logger;
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


  


    public ForgotPasswordModel(AppDbContext context, IDbLocalizer localizer, EmailService emailService, ILogger<ForgotPasswordModel> logger, SharedHelper sharedHelper)
    {
      _localizer = localizer;

      _context = context;

      _emailService = emailService;
      _logger = logger;
      _sharedhelper = sharedHelper;


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
        AlertMessageContent = _localizer.Get("EnterYourUsername");

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


      //if (adminUser == null)
      //{
      //  AlertMessageType = MessageType.Error;
      //  AlertMessageTitle = MessageTitle.Error;
      //  AlertMessageContent = _localizer.Get("InvalidUsername");

      //  return Page();

      //}

      //if (adminUser.LoginStatus != AppConstants.LoginStatus.Active)
      //{
      //  AlertMessageType = MessageType.Error;
      //  AlertMessageTitle = MessageTitle.Error;
      //  AlertMessageContent = _localizer.Get("InactiveUsername");

      //  return Page();
      //}

      string? DBEmail = adminUser.Email;
      string? DBPassword = PasswordCryptoHelper.Decrypt(adminUser.PasswordHash);

      // TODO: Verify user exists and generate token



      string EmailSubject = "";
      string EmailhtmlContent = "";    

      string currentUserLanguage = UsersHelper.GetCurrentCultureCode();// Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;


      //  var template = await _context.EmailTemplates
      //.FirstOrDefaultAsync(t => t.TemplateKey == "ForgotPassword" && t.CultureCode == currentUserLanguage);
      var template = await _sharedhelper.GetEmailTemplate(AppConstants.EmailTemplate.ForgotPassword, currentUserLanguage);

      if (template == null)
      {
        AlertMessageType = MessageType.Error;
        AlertMessageTitle = MessageTitle.Error;
        AlertMessageContent = "Email template not found for key ForgotPassword and language " + currentUserLanguage;
       
        _logger.LogWarning(AlertMessageContent);     
       
        return Page(); // Or fallback
      }

      EmailSubject = template.Subject;
      EmailhtmlContent = template.BodyHtml;
      EmailhtmlContent = EmailhtmlContent.Replace("{Username}", adminUser.Username);
      EmailhtmlContent = EmailhtmlContent.Replace("{Password}", DBPassword);

      await _emailService.SendEmailAsync(
        DBEmail,
        EmailSubject,
        EmailhtmlContent
        );
      // _logger.LogInformation($"Reset email sent to {txtUsername}: {DBEmail}");


      AlertMessageType = MessageType.Success;
      AlertMessageTitle = MessageTitle.Success;
      AlertMessageContent = _localizer.Get("PasswordEmailSend");
      return Page();
      //return RedirectToPage(AppConstants.Routes.ForgotPassword);


    }

  }
  
}
