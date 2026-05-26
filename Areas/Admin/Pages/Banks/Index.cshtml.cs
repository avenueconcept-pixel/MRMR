using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Banks;

public class IndexModel : AdminPageModel
{
  private readonly BankDbHelper    _bankDbHelper;
  private readonly CountryDbHelper _countryDbHelper;
  private readonly TranslationService _translation;

  public List<Bank>           Banks           { get; set; } = new();
  public List<SelectListItem> CountryOptions  { get; set; } = new();
  public string               SelectedCountry { get; set; } = string.Empty;

  public IndexModel(BankDbHelper bankDbHelper, CountryDbHelper countryDbHelper, TranslationService translation)
  {
    _bankDbHelper    = bankDbHelper;
    _countryDbHelper = countryDbHelper;
    _translation     = translation;
  }

  public async Task OnGetAsync(string countryCode = "")
  {
    AlertMessageType = "";
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;

    var allCountry  = await _translation.GetAsync("Bank.AllCountries");
    CountryOptions  = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
    CountryOptions.Insert(0, new SelectListItem { Value = string.Empty, Text = allCountry });

    var allBanks = await _bankDbHelper.GetAllAsync(langCode);
    Banks = string.IsNullOrEmpty(countryCode)
        ? allBanks
        : allBanks.Where(b => b.CountryCode == countryCode).ToList();

    SelectedCountry = countryCode;
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] string bankCode)
  {
    var bank = await _bankDbHelper.GetByCodeAsync(bankCode);
    if (bank == null)
      return new JsonResult(new { success = false });

    var newStatus = bank.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _bankDbHelper.UpdateStatusAsync(bankCode, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
