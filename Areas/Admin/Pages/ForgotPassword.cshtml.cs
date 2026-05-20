using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;
using System;
using System.Collections.Generic;
using MyApp.Constants;

namespace MyApp.Areas.Admin.Pages
{
  public class ForgotPasswordModel : PageModel
  {
    private readonly AppDbContext _context;
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


  


    public ForgotPasswordModel(AppDbContext context, TranslationService translation, EmailService emailService, ILogger<ForgotPasswordModel> logger,  AdminDbHelper adminDbHelper)
    {
      _translation = translation;
      _context = context;
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

      string? DBEmail = "";
      string? DBPassword = "";
      string currentUserLanguage ="";
      if (adminUser != null)
      {
        DBEmail = adminUser.Email;
        DBPassword = PasswordCryptoHelper.Decrypt(adminUser.PasswordHash);
        currentUserLanguage = adminUser.LastLoginLangCode;
      }

      string EmailSubject = "";
      string EmailhtmlContent = "";    

     

      //  var template = await _context.EmailTemplates
      //.FirstOrDefaultAsync(t => t.TemplateKey == "ForgotPassword" && t.CultureCode == currentUserLanguage);
      var template = await _adminDbHelper.GetEmailTemplateAsync(AppConstants.EmailTemplate.ForgotPassword, currentUserLanguage);

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
      AlertMessageContent = await _translation.GetAsync("PasswordEmailSend");
      return Page();
      //return RedirectToPage(AppConstants.Routes.ForgotPassword);


    }

  }
  
}
