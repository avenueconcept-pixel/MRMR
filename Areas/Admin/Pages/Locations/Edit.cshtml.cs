using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Locations;

public class EditModel : AdminPageModel
{
  private readonly LocationDbHelper _locationDbHelper;
  private readonly CountryDbHelper  _countryDbHelper;
  private readonly StateDbHelper    _stateDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtLocationName { get; set; } = string.Empty;
  [BindProperty] public string ddlLocationType { get; set; } = string.Empty;
  [BindProperty] public string ddlCountryCode  { get; set; } = string.Empty;
  [BindProperty] public string ddlStateId      { get; set; } = string.Empty;
  [BindProperty] public string txtCity         { get; set; } = string.Empty;
  [BindProperty] public string txtPostcode     { get; set; } = string.Empty;
  [BindProperty] public string txtAddress      { get; set; } = string.Empty;
  [BindProperty] public string ddlStatus       { get; set; } = StatusConstants.Active;

  public int      Id           { get; set; }
  public string   LocationCode { get; set; } = string.Empty;
  public string   CreatedBy    { get; set; } = string.Empty;
  public DateTime CreatedAt    { get; set; }
  public string   UpdatedBy    { get; set; } = string.Empty;
  public DateTime UpdatedAt    { get; set; }

  public List<SelectListItem> StatusOptions       { get; set; } = new();
  public List<SelectListItem> LocationTypeOptions { get; set; } = new();
  public List<SelectListItem> CountryOptions      { get; set; } = new();
  public List<SelectListItem> StateOptions        { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(
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

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";

    var location = await _locationDbHelper.GetByIdAsync(id);
    if (location == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminLocation);
    }

    Id              = location.Id;
    LocationCode    = location.LocationCode;
    txtLocationName = location.LocationName;
    ddlLocationType = location.LocationType;
    ddlCountryCode  = location.CountryCode;
    ddlStateId      = location.StateId?.ToString() ?? string.Empty;
    txtCity         = location.City     ?? string.Empty;
    txtPostcode     = location.Postcode ?? string.Empty;
    txtAddress      = location.Address  ?? string.Empty;
    ddlStatus       = location.Status;
    CreatedBy       = location.CreatedBy;
    CreatedAt       = location.CreatedAt;
    UpdatedBy       = location.UpdatedBy;
    UpdatedAt       = location.UpdatedAt;

    StatusOptions       = await SelectListHelper.GetStatusOptions(_translation);
    LocationTypeOptions = await SelectListHelper.GetLocationTypeOptions(_translation);
    CountryOptions      = await SelectListHelper.GetCountryOptions(_countryDbHelper, "en");

    var states = await _stateDbHelper.GetActiveByCountryAsync(location.CountryCode, "en");
    StateOptions = states.Select(s => new SelectListItem
    {
      Value = s.Id.ToString(),
      Text  = s.Translations.FirstOrDefault()?.StateName ?? s.StateCode
    }).ToList();

    var entityName        = location.LocationName;
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
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

  public async Task<IActionResult> OnPostUpdateAsync(int id)
  {
    var existing = await _locationDbHelper.GetByIdAsync(id);
    if (existing == null)
      return RedirectToPage(Routes.AdminLocation);

    Id           = id;
    LocationCode = existing.LocationCode;
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

    if (string.IsNullOrWhiteSpace(txtLocationName)
        || string.IsNullOrWhiteSpace(ddlLocationType)
        || string.IsNullOrWhiteSpace(ddlCountryCode))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    int.TryParse(ddlStateId, out var stateId);
    var location = new Location
    {
      Id           = id,
      LocationCode = existing.LocationCode,
      LocationName = txtLocationName.Trim(),
      LocationType = ddlLocationType,
      CountryCode  = ddlCountryCode,
      StateId      = stateId > 0 ? stateId : (int?)null,
      City         = string.IsNullOrWhiteSpace(txtCity)     ? null : txtCity.Trim(),
      Postcode     = string.IsNullOrWhiteSpace(txtPostcode) ? null : txtPostcode.Trim(),
      Address      = string.IsNullOrWhiteSpace(txtAddress)  ? null : txtAddress.Trim(),
      Status       = ddlStatus
    };

    await _locationDbHelper.UpdateAsync(location, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminLocation);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _locationDbHelper.UpdateStatusAsync(id, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }
}
