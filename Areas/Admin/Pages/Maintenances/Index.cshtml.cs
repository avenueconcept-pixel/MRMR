using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Maintenances;

public class IndexModel : AdminPageModel
{
  private readonly MaintenanceDbHelper _dbHelper;
  private readonly MaintenanceService  _maintenanceService;
  private readonly TranslationService  _translation;

  public List<MaintenanceSchedule> Items { get; set; } = new();

  public string MsgToggleSuccess { get; set; } = string.Empty;
  public string MsgToggleError   { get; set; } = string.Empty;
  public string MsgCancelBtn     { get; set; } = string.Empty;

  public IndexModel(
      MaintenanceDbHelper dbHelper,
      MaintenanceService  maintenanceService,
      TranslationService  translation)
  {
    _dbHelper           = dbHelper;
    _maintenanceService = maintenanceService;
    _translation        = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType  = "";
    Items             = await _dbHelper.GetAllAsync();
    MsgToggleSuccess  = await _translation.GetAsync(MessageConstants.SaveSuccess);
    MsgToggleError    = await _translation.GetAsync(MessageConstants.SaveError);
    MsgCancelBtn      = await _translation.GetAsync("Btn.Cancel");
  }

  public async Task<IActionResult> OnPostToggleActiveAsync(int id)
  {
    var schedule = await _dbHelper.GetByIdAsync(id);
    if (schedule == null)
      return new JsonResult(new { success = false });

    await _dbHelper.UpdateIsActiveAsync(id, !schedule.IsActive, CurrentUsername);
    _maintenanceService.InvalidateCache();
    return new JsonResult(new { success = true, isActive = !schedule.IsActive });
  }
}
