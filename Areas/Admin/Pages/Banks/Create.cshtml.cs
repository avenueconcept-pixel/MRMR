using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Banks;

public class CreateModel : AdminPageModel
{
  private readonly BankDbHelper    _bankDbHelper;
  private readonly CountryDbHelper _countryDbHelper;
  private readonly LanguageDbHelper _languageDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtBankCode    { get; set; } = string.Empty;
  [BindProperty] public string ddlCountryCode { get; set; } = string.Empty;
  [BindProperty] public string txtSwiftCode   { get; set; } = string.Empty;
  [BindProperty] public string txtLocalCode   { get; set; } = string.Empty;
  [BindProperty] public string txtWebsite     { get; set; } = string.Empty;
  [BindProperty] public string txtLogo        { get; set; } = string.Empty;
  [BindProperty] public string ddlStatus      { get; set; } = StatusConstants.Active;

  public List<SelectListItem>      CountryOptions    { get; set; } = new();
  public List<TranslationInputDto> TranslationInputs { get; set; } = new();
  public List<TranslationInputDto> ShortNameInputs   { get; set; } = new();
  public List<SelectListItem>      StatusOptions     { get; set; } = new();

  public CreateModel(BankDbHelper bankDbHelper, CountryDbHelper countryDbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _bankDbHelper     = bankDbHelper;
    _countryDbHelper  = countryDbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType  = "";
    var langCode      = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    CountryOptions    = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(existing: null);
    ShortNameInputs   = await BuildShortNameInputsAsync(existing: null);
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    var langCode      = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    CountryOptions    = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(existing: null);
    ShortNameInputs   = await BuildShortNameInputsAsync(existing: null);

    if (string.IsNullOrWhiteSpace(txtBankCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    if (string.IsNullOrWhiteSpace(ddlCountryCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    var code = txtBankCode.Trim().ToUpper();

    var bank = new Bank
    {
      BankCode    = code,
      CountryCode = ddlCountryCode,
      SwiftCode   = string.IsNullOrWhiteSpace(txtSwiftCode) ? null : txtSwiftCode.Trim(),
      LocalCode   = string.IsNullOrWhiteSpace(txtLocalCode) ? null : txtLocalCode.Trim(),
      Website     = string.IsNullOrWhiteSpace(txtWebsite)   ? null : txtWebsite.Trim(),
      Logo        = string.IsNullOrWhiteSpace(txtLogo)       ? null : txtLogo.Trim(),
      Status      = ddlStatus
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new BankTranslation
        {
          BankCode     = code,
          LanguageCode = l.LanguageCode,
          BankName     = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim(),
          ShortName    = Request.Form[$"txtShortName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.BankName))
        .ToList();

    var result = await _bankDbHelper.AddAsync(bank, translations, CurrentUsername);

    if (result == BankAddResult.DuplicateActive)
    {
      SetError(await _translation.GetAsync(MessageConstants.DuplicateError));
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(
        result == BankAddResult.Restored ? MessageConstants.RestoreSuccess : MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminBank);
  }

  private async Task<List<TranslationInputDto>> BuildInputsAsync(IList<BankTranslation>? existing)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("Bank.NamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = l.LanguageName,
      Value        = existing != null
          ? existing.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.BankName ?? string.Empty
          : Request.Form[$"txtName_{l.LanguageCode}"].ToString(),
      Placeholder  = placeholder
    }).ToList();
  }

  private async Task<List<TranslationInputDto>> BuildShortNameInputsAsync(IList<BankTranslation>? existing)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("Bank.ShortNamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = l.LanguageName,
      Value        = existing != null
          ? existing.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.ShortName ?? string.Empty
          : Request.Form[$"txtShortName_{l.LanguageCode}"].ToString(),
      Placeholder  = placeholder
    }).ToList();
  }

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
