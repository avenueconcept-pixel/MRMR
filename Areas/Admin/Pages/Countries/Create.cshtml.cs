using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Countries;

public class CreateModel : AdminPageModel
{
  private readonly CountryDbHelper _countryDbHelper;
  private readonly TranslationService _translation;

  [BindProperty]
  public string txtCountryCode { get; set; } = string.Empty;

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

  public List<SelectListItem> StatusOptions { get; set; } = new();

  public CreateModel(CountryDbHelper countryDbHelper, TranslationService translation)
  {
    _countryDbHelper = countryDbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    if (string.IsNullOrWhiteSpace(txtCountryCode))
    {
      AlertMessageType = MessageType.Error;
      AlertMessageTitle = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var code = txtCountryCode.Trim().ToUpper();

    if (code.Length > 3)
    {
      AlertMessageType = MessageType.Error;
      AlertMessageTitle = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var existing = await _countryDbHelper.GetCountryByCodeAsync(code);
    if (existing != null)
    {
      AlertMessageType = MessageType.Error;
      AlertMessageTitle = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveError);
      return Page();
    }

    var country = new Country
    {
      CountryCode  = code,
      CurrencyCode = txtCurrencyCode?.Trim().ToUpper() ?? string.Empty,
      Status       = ddlStatus
    };

    var translations = new List<CountryTranslation>();
    if (!string.IsNullOrWhiteSpace(txtNameEn))
      translations.Add(new CountryTranslation { CountryCode = code, LanguageCode = "en",      CountryName = txtNameEn.Trim() });
    if (!string.IsNullOrWhiteSpace(txtNameZhHans))
      translations.Add(new CountryTranslation { CountryCode = code, LanguageCode = "zh-Hans", CountryName = txtNameZhHans.Trim() });
    if (!string.IsNullOrWhiteSpace(txtNameZhHant))
      translations.Add(new CountryTranslation { CountryCode = code, LanguageCode = "zh-Hant", CountryName = txtNameZhHant.Trim() });
    if (!string.IsNullOrWhiteSpace(txtNameTh))
      translations.Add(new CountryTranslation { CountryCode = code, LanguageCode = "th",       CountryName = txtNameTh.Trim() });

    await _countryDbHelper.AddCountryAsync(country, translations, CurrentUsername);

    AlertMessageType = MessageType.Success;
    AlertMessageTitle = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminCountry);
  }
}
