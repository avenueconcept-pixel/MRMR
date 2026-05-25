using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Regions;

public class EditModel : AdminPageModel
{
  private readonly RegionDbHelper  _regionDbHelper;
  private readonly CountryDbHelper _countryDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string       txtRegionName     { get; set; } = string.Empty;
  [BindProperty] public string       ddlStatus         { get; set; } = StatusConstants.Active;
  [BindProperty] public List<string> SelectedCountries { get; set; } = new();

  public int      Id         { get; set; }
  public string   RegionCode { get; set; } = string.Empty;
  public string   CreatedBy  { get; set; } = string.Empty;
  public DateTime CreatedAt  { get; set; }
  public string   UpdatedBy  { get; set; } = string.Empty;
  public DateTime UpdatedAt  { get; set; }

  public List<Country>        AllCountries  { get; set; } = new();
  public List<SelectListItem> StatusOptions { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(RegionDbHelper regionDbHelper, CountryDbHelper countryDbHelper, TranslationService translation)
  {
    _regionDbHelper  = regionDbHelper;
    _countryDbHelper = countryDbHelper;
    _translation     = translation;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";

    var region = await _regionDbHelper.GetByIdAsync(id);
    if (region == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminRegion);
    }

    Id                = region.Id;
    RegionCode        = region.RegionCode;
    txtRegionName     = region.RegionName;
    ddlStatus         = region.Status;
    SelectedCountries = region.RegionCountries.Select(rc => rc.CountryCode).ToList();
    CreatedBy         = region.CreatedBy;
    CreatedAt         = region.CreatedAt;
    UpdatedBy         = region.UpdatedBy;
    UpdatedAt         = region.UpdatedAt;

    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);
    AllCountries  = await _countryDbHelper.GetAllActiveAsync("en");

    var entityName        = region.RegionName;
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(int id)
  {
    var existing = await _regionDbHelper.GetByIdAsync(id);
    if (existing == null)
      return RedirectToPage(Routes.AdminRegion);

    Id         = id;
    RegionCode = existing.RegionCode;
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);
    AllCountries  = await _countryDbHelper.GetAllActiveAsync("en");

    if (string.IsNullOrWhiteSpace(txtRegionName))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var region = new Region
    {
      Id         = id,
      RegionCode = existing.RegionCode,
      RegionName = txtRegionName.Trim(),
      Status     = ddlStatus
    };

    await _regionDbHelper.UpdateAsync(region, SelectedCountries, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminRegion);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _regionDbHelper.UpdateStatusAsync(id, StatusConstants.Deleted, CurrentUsername);
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
