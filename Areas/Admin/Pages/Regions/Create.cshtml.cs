using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Regions;

public class CreateModel : AdminPageModel
{
  private readonly RegionDbHelper  _regionDbHelper;
  private readonly CountryDbHelper _countryDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string       txtRegionCode     { get; set; } = string.Empty;
  [BindProperty] public string       txtRegionName     { get; set; } = string.Empty;
  [BindProperty] public string       ddlStatus         { get; set; } = StatusConstants.Active;
  [BindProperty] public List<string> SelectedCountries { get; set; } = new();

  public List<Country>        AllCountries  { get; set; } = new();
  public List<SelectListItem> StatusOptions { get; set; } = new();

  public CreateModel(RegionDbHelper regionDbHelper, CountryDbHelper countryDbHelper, TranslationService translation)
  {
    _regionDbHelper  = regionDbHelper;
    _countryDbHelper = countryDbHelper;
    _translation     = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    StatusOptions    = await SelectListHelper.GetStatusOptions(_translation);
    AllCountries     = await _countryDbHelper.GetAllActiveAsync("en");
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);
    AllCountries  = await _countryDbHelper.GetAllActiveAsync("en");

    if (string.IsNullOrWhiteSpace(txtRegionCode) || string.IsNullOrWhiteSpace(txtRegionName))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var region = new Region
    {
      RegionCode = txtRegionCode.Trim().ToUpperInvariant(),
      RegionName = txtRegionName.Trim(),
      Status     = ddlStatus
    };

    var result = await _regionDbHelper.AddAsync(region, SelectedCountries, CurrentUsername);

    if (result == RegionAddResult.DuplicateActive)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.DuplicateError);
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = result == RegionAddResult.Restored
        ? await _translation.GetAsync(MessageConstants.RestoreSuccess)
        : await _translation.GetAsync(MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminRegion);
  }
}
