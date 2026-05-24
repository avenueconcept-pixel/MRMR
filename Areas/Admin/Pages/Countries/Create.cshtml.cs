using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Countries;

public class CreateModel : AdminPageModel
{
  private readonly CountryDbHelper  _countryDbHelper;
  private readonly LanguageDbHelper _languageDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string  txtCountryCode  { get; set; } = string.Empty;
  [BindProperty] public string? txtCurrencyCode { get; set; }
  [BindProperty] public string  ddlStatus       { get; set; } = StatusConstants.Active;
  [BindProperty] public string  ddlTimezone     { get; set; } = string.Empty;

  public List<TranslationInputDto> TranslationInputs { get; set; } = new();
  public List<SelectListItem>      StatusOptions     { get; set; } = new();
  public List<SelectListItem>      TimezoneOptions   { get; set; } = new();

  public CreateModel(CountryDbHelper countryDbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _countryDbHelper  = countryDbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TimezoneOptions   = SelectListHelper.GetTimezoneOptions();
    TranslationInputs = await BuildInputsAsync(fromForm: false);
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TimezoneOptions   = SelectListHelper.GetTimezoneOptions();
    TranslationInputs = await BuildInputsAsync(fromForm: true);

    if (string.IsNullOrWhiteSpace(txtCountryCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    var code = txtCountryCode.Trim().ToUpper();

    if (code.Length > 2)
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    if (string.IsNullOrEmpty(ddlTimezone))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    var country = new Country
    {
      CountryCode  = code,
      CurrencyCode = txtCurrencyCode?.Trim().ToUpper() ?? string.Empty,
      Status       = ddlStatus,
      Timezone     = ddlTimezone
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new CountryTranslation
        {
          CountryCode  = code,
          LanguageCode = l.LanguageCode,
          CountryName  = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.CountryName))
        .ToList();

    var result = await _countryDbHelper.AddAsync(country, translations, CurrentUsername);

    if (result == CountryAddResult.DuplicateActive)
    {
      SetError(await _translation.GetAsync(MessageConstants.DuplicateError));
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(
        result == CountryAddResult.Restored ? MessageConstants.RestoreSuccess : MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminCountry);
  }

  private async Task<List<TranslationInputDto>> BuildInputsAsync(bool fromForm)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("Country.NamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = $"{l.LanguageName}",
      Value        = fromForm ? Request.Form[$"txtName_{l.LanguageCode}"].ToString() : string.Empty,
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
