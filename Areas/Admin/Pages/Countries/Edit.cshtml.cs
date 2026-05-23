using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Countries;

public class EditModel : AdminPageModel
{
  private readonly CountryDbHelper _countryDbHelper;
  private readonly TranslationService _translation;

  [BindProperty]
  public string? txtCurrencyCode { get; set; }

  [BindProperty]
  public string ddlStatus { get; set; } = UserStatusConstants.Active;

  [BindProperty]
  public string? txtNameEn { get; set; }

  [BindProperty]
  public string? txtNameZhHans { get; set; }

  [BindProperty]
  public string? txtNameZhHant { get; set; }

  [BindProperty]
  public string? txtNameTh { get; set; }

  public string CountryCode { get; set; } = string.Empty;
  public List<SelectListItem> StatusOptions { get; set; } = new();

  public EditModel(CountryDbHelper countryDbHelper, TranslationService translation)
  {
    _countryDbHelper = countryDbHelper;
    _translation = translation;
  }

  public async Task<IActionResult> OnGetAsync(string countryCode)
  {
    var country = await _countryDbHelper.GetCountryByCodeAsync(countryCode);
    if (country == null)
    {
      AlertMessageType = MessageType.Error;
      AlertMessageTitle = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminCountry);
    }

    CountryCode     = country.CountryCode;
    txtCurrencyCode = country.CurrencyCode;
    ddlStatus       = country.Status;
    txtNameEn      = country.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.CountryName;
    txtNameZhHans  = country.Translations.FirstOrDefault(t => t.LanguageCode == "zh-Hans")?.CountryName;
    txtNameZhHant  = country.Translations.FirstOrDefault(t => t.LanguageCode == "zh-Hant")?.CountryName;
    txtNameTh      = country.Translations.FirstOrDefault(t => t.LanguageCode == "th")?.CountryName;
    StatusOptions  = await SelectListHelper.GetStatusOptions(_translation);

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string countryCode)
  {
    CountryCode   = countryCode;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    var country = new Country
    {
      CountryCode  = countryCode,
      CurrencyCode = txtCurrencyCode?.Trim().ToUpper() ?? string.Empty,
      Status       = ddlStatus
    };

    var translations = new List<CountryTranslation>();
    if (!string.IsNullOrWhiteSpace(txtNameEn))
      translations.Add(new CountryTranslation { CountryCode = countryCode, LanguageCode = "en",      CountryName = txtNameEn.Trim() });
    if (!string.IsNullOrWhiteSpace(txtNameZhHans))
      translations.Add(new CountryTranslation { CountryCode = countryCode, LanguageCode = "zh-Hans", CountryName = txtNameZhHans.Trim() });
    if (!string.IsNullOrWhiteSpace(txtNameZhHant))
      translations.Add(new CountryTranslation { CountryCode = countryCode, LanguageCode = "zh-Hant", CountryName = txtNameZhHant.Trim() });
    if (!string.IsNullOrWhiteSpace(txtNameTh))
      translations.Add(new CountryTranslation { CountryCode = countryCode, LanguageCode = "th",       CountryName = txtNameTh.Trim() });

    await _countryDbHelper.UpdateCountryAsync(country, translations, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminCountry);
  }
}
