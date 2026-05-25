using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Locations;

public class IndexModel : AdminPageModel
{
  private readonly LocationDbHelper _locationDbHelper;
  private readonly TranslationService _translation;

  public List<Location> Locations { get; set; } = new();

  public IndexModel(LocationDbHelper locationDbHelper, TranslationService translation)
  {
    _locationDbHelper = locationDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    Locations = await _locationDbHelper.GetAllAsync();
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] int id)
  {
    var location = await _locationDbHelper.GetByIdAsync(id);
    if (location == null)
      return new JsonResult(new { success = false });

    var newStatus = location.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _locationDbHelper.UpdateStatusAsync(id, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
