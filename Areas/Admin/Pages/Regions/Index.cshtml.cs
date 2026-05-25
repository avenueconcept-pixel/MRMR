using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Regions;

public class IndexModel : AdminPageModel
{
  private readonly RegionDbHelper  _regionDbHelper;
  private readonly TranslationService _translation;

  public List<Region> Regions { get; set; } = new();

  public IndexModel(RegionDbHelper regionDbHelper, TranslationService translation)
  {
    _regionDbHelper = regionDbHelper;
    _translation    = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    Regions = await _regionDbHelper.GetAllAsync();
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] int id)
  {
    var region = await _regionDbHelper.GetByIdAsync(id);
    if (region == null)
      return new JsonResult(new { success = false });

    var newStatus = region.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _regionDbHelper.UpdateStatusAsync(id, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
