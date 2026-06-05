using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Locations;

public class CreateModel : AdminPageModel
{
  private readonly LocationDbHelper _locationDbHelper;
  private readonly CountryDbHelper  _countryDbHelper;
  private readonly StateDbHelper    _stateDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtLocationCode { get; set; } = string.Empty;
  [BindProperty] public string txtLocationName { get; set; } = string.Empty;
  [BindProperty] public string ddlLocationType { get; set; } = string.Empty;
  [BindProperty] public string ddlCountryCode  { get; set; } = string.Empty;
  [BindProperty] public string ddlStateId      { get; set; } = string.Empty;
  [BindProperty] public string txtCity         { get; set; } = string.Empty;
  [BindProperty] public string txtPostcode     { get; set; } = string.Empty;
  [BindProperty] public string txtAddress      { get; set; } = string.Empty;
  [BindProperty] public string ddlStatus       { get; set; } = StatusConstants.Active;

  public List<SelectListItem> StatusOptions       { get; set; } = new();
  public List<SelectListItem> LocationTypeOptions { get; set; } = new();
  public List<SelectListItem> CountryOptions      { get; set; } = new();
  public List<SelectListItem> StateOptions        { get; set; } = new();

  public CreateModel(
    LocationDbHelper locationDbHelper,
    CountryDbHelper  countryDbHelper,
    StateDbHelper    stateDbHelper,
    TranslationService translation)
  {
    _locationDbHelper = locationDbHelper;
    _countryDbHelper  = countryDbHelper;
    _stateDbHelper    = stateDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType    = "";
    StatusOptions       = await SelectListHelper.GetStatusOptions(_translation);
    LocationTypeOptions = await SelectListHelper.GetLocationTypeOptions(_translation);
    CountryOptions      = await SelectListHelper.GetCountryOptions(_countryDbHelper, "en");
  }

  public async Task<IActionResult> OnGetStatesByCountryAsync(string countryCode)
  {
    var states = await _stateDbHelper.GetActiveByCountryAsync(countryCode, "en");
    var result = states.Select(s => new
    {
      id   = s.Id,
      name = s.Translations.FirstOrDefault()?.StateName ?? s.StateCode
    });
    return new JsonResult(result);
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    StatusOptions       = await SelectListHelper.GetStatusOptions(_translation);
    LocationTypeOptions = await SelectListHelper.GetLocationTypeOptions(_translation);
    CountryOptions      = await SelectListHelper.GetCountryOptions(_countryDbHelper, "en");

    if (!string.IsNullOrEmpty(ddlCountryCode))
    {
      var states = await _stateDbHelper.GetActiveByCountryAsync(ddlCountryCode, "en");
      StateOptions = states.Select(s => new SelectListItem
      {
        Value = s.Id.ToString(),
        Text  = s.Translations.FirstOrDefault()?.StateName ?? s.StateCode
      }).ToList();
    }

    if (string.IsNullOrWhiteSpace(txtLocationCode) || string.IsNullOrWhiteSpace(txtLocationName)
        || string.IsNullOrWhiteSpace(ddlLocationType) || string.IsNullOrWhiteSpace(ddlCountryCode))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    int.TryParse(ddlStateId, out var stateId);
    var location = new Location
    {
      LocationCode = txtLocationCode.Trim().ToUpperInvariant(),
      LocationName = txtLocationName.Trim(),
      LocationType = ddlLocationType,
      CountryCode  = ddlCountryCode,
      StateId      = stateId > 0 ? stateId : (int?)null,
      City         = string.IsNullOrWhiteSpace(txtCity)     ? null : txtCity.Trim(),
      Postcode     = string.IsNullOrWhiteSpace(txtPostcode) ? null : txtPostcode.Trim(),
      Address      = string.IsNullOrWhiteSpace(txtAddress)  ? null : txtAddress.Trim(),
      Status       = ddlStatus
    };

    var result = await _locationDbHelper.AddAsync(location, CurrentUsername);

    if (result == LocationAddResult.DuplicateActive)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.DuplicateError);
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = result == LocationAddResult.Restored
        ? await _translation.GetAsync(MessageConstants.RestoreSuccess)
        : await _translation.GetAsync(MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminLocation);
  }
}
