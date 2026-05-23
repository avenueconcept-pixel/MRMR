using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Countries;

public class IndexModel : AdminPageModel
{
  private readonly CountryDbHelper _countryDbHelper;
  private readonly TranslationService _translation;

  public List<Country> Countries { get; set; } = new();

  public IndexModel(CountryDbHelper countryDbHelper, TranslationService translation)
  {
    _countryDbHelper = countryDbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    Countries = await _countryDbHelper.GetAllCountriesAsync("en");
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] string countryCode)
  {
    var country = await _countryDbHelper.GetCountryByCodeAsync(countryCode);
    if (country == null)
      return new JsonResult(new { success = false });

    var newStatus = country.Status == UserStatusConstants.Active
        ? UserStatusConstants.Inactive
        : UserStatusConstants.Active;

    await _countryDbHelper.UpdateCountryStatusAsync(countryCode, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
