using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.States;

public class IndexModel : AdminPageModel
{
  private readonly StateDbHelper   _stateDbHelper;
  private readonly CountryDbHelper _countryDbHelper;
  private readonly TranslationService _translation;

  public List<SelectListItem> CountryOptions { get; set; } = new();

  public IndexModel(StateDbHelper stateDbHelper, CountryDbHelper countryDbHelper, TranslationService translation)
  {
    _stateDbHelper   = stateDbHelper;
    _countryDbHelper = countryDbHelper;
    _translation     = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    var langCode     = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    var countries    = await _countryDbHelper.GetAllAsync(langCode);
    CountryOptions   = countries
        .Select(c => new SelectListItem
        {
          Value = c.CountryCode,
          Text  = c.Translations.FirstOrDefault()?.CountryName ?? c.CountryCode
        })
        .ToList();
  }

  public async Task<IActionResult> OnPostLoadStatesAsync([FromForm] string countryCode)
  {
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    var states   = await _stateDbHelper.GetAllAsync(countryCode, langCode);
    var result   = states.Select(s => new
    {
      id        = s.Id,
      stateCode = s.StateCode,
      stateName = s.Translations.FirstOrDefault()?.StateName ?? s.StateCode,
      sortOrder = s.SortOrder,
      status    = s.Status,
      updatedAt = s.UpdatedAt.ToUserLocalTime(UserTimezone, AppConstants.DateTimeFormat)
    });
    return new JsonResult(new { success = true, states = result });
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] int id)
  {
    var state = await _stateDbHelper.GetByIdAsync(id);
    if (state == null)
      return new JsonResult(new { success = false });

    var newStatus = state.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _stateDbHelper.UpdateStatusAsync(id, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
