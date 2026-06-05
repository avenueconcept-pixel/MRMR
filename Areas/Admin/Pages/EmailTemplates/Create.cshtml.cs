using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.EmailTemplates;

public class CreateModel : AdminPageModel
{
  private readonly EmailTemplateDbHelper _dbHelper;
  private readonly LanguageDbHelper      _languageDbHelper;
  private readonly TranslationService    _translation;

  [BindProperty] public string txtTemplateKey  { get; set; } = string.Empty;
  [BindProperty] public string ddlLanguageCode { get; set; } = string.Empty;
  [BindProperty] public string txtSubject      { get; set; } = string.Empty;
  [BindProperty] public string txtBodyHtml     { get; set; } = string.Empty;

  public List<SelectListItem> LanguageOptions { get; set; } = new();
  public List<string>         ExistingKeys    { get; set; } = new();

  public CreateModel(EmailTemplateDbHelper dbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _dbHelper         = dbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync(string? templateKey = null)
  {
    AlertMessageType = "";
    await BuildFormDataAsync();
    if (!string.IsNullOrEmpty(templateKey))
      txtTemplateKey = templateKey;
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    await BuildFormDataAsync();

    if (string.IsNullOrWhiteSpace(txtTemplateKey)  ||
        string.IsNullOrWhiteSpace(ddlLanguageCode) ||
        string.IsNullOrWhiteSpace(txtSubject)      ||
        string.IsNullOrWhiteSpace(txtBodyHtml))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var template = new EmailTemplate
    {
      TemplateKey  = txtTemplateKey.Trim(),
      LanguageCode = ddlLanguageCode,
      Subject      = txtSubject.Trim(),
      BodyHtml     = txtBodyHtml,
      Status       = StatusConstants.Active
    };

    var result = await _dbHelper.AddAsync(template, CurrentUsername);

    if (result == EmailTemplateAddResult.Duplicate)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync("EmailTemplate.DuplicateKey");
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveSuccess);
    return RedirectToPage(Routes.AdminEmailTemplate);
  }

  private async Task BuildFormDataAsync()
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    LanguageOptions = languages
        .Select(l => new SelectListItem(l.LanguageName, l.LanguageCode))
        .ToList();
    ExistingKeys = await _dbHelper.GetAllKeysAsync();
  }
}
