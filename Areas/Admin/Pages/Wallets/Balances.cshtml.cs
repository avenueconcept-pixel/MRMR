using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Wallets;

public class BalancesModel : AdminPageModel
{
  private readonly WalletDbHelper    _walletDbHelper;
  private readonly CountryDbHelper   _countryDbHelper;
  private readonly TranslationService _translation;

  [BindProperty(SupportsGet = true)] public string? FilterUsername    { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterCountryCode { get; set; }

  public List<MemberWalletBalanceRowDto> Items      { get; set; } = new();
  public List<SelectListItem>            ddlCountry { get; set; } = new();

  public BalancesModel(WalletDbHelper walletDbHelper, CountryDbHelper countryDbHelper, TranslationService translation)
  {
    _walletDbHelper  = walletDbHelper;
    _countryDbHelper = countryDbHelper;
    _translation     = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    Items = await _walletDbHelper.GetAllBalancesAsync(FilterUsername, FilterCountryCode);

    var langCode  = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    var countries = await _countryDbHelper.GetAllActiveAsync(langCode);

    ddlCountry = new List<SelectListItem>
    {
      new() { Value = string.Empty, Text = await _translation.GetAsync("Filter.All") }
    };
    ddlCountry.AddRange(countries.Select(c => new SelectListItem
    {
      Value = c.CountryCode,
      Text  = c.Translations.FirstOrDefault()?.CountryName ?? c.CountryCode
    }));
  }
}
