using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Maintenances;

public class EditModel : AdminPageModel
{
  private readonly MaintenanceDbHelper _dbHelper;
  private readonly MaintenanceService  _maintenanceService;
  private readonly SystemDbHelper      _systemDbHelper;
  private readonly LanguageDbHelper    _languageDbHelper;
  private readonly TranslationService  _translation;

  [BindProperty] public string txtTitle   { get; set; } = string.Empty;
  [BindProperty] public string txtStartAt { get; set; } = string.Empty;
  [BindProperty] public string txtEndAt   { get; set; } = string.Empty;

  public int              ScheduleId       { get; set; }
  public List<AppSystem>  Systems          { get; set; } = new();
  public List<Language>   Languages        { get; set; } = new();
  public List<string>     SelectedSystems  { get; set; } = new();
  public Dictionary<string, string> ExistingMessages { get; set; } = new();
  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(
      MaintenanceDbHelper dbHelper,
      MaintenanceService  maintenanceService,
      SystemDbHelper      systemDbHelper,
      LanguageDbHelper    languageDbHelper,
      TranslationService  translation)
  {
    _dbHelper           = dbHelper;
    _maintenanceService = maintenanceService;
    _systemDbHelper     = systemDbHelper;
    _languageDbHelper   = languageDbHelper;
    _translation        = translation;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";

    var schedule = await _dbHelper.GetByIdAsync(id);
    if (schedule == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminMaintenances);
    }

    ScheduleId      = schedule.Id;
    txtTitle        = schedule.Title;
    txtStartAt      = schedule.StartAt.ToUserLocalTime(UserTimezone, AppConstants.DateTimeInputFormat);
    txtEndAt        = schedule.EndAt.ToUserLocalTime(UserTimezone, AppConstants.DateTimeInputFormat);
    SelectedSystems = schedule.Systems.Select(s => s.SystemCode).ToList();
    ExistingMessages = schedule.Messages.ToDictionary(m => m.LanguageCode, m => m.Message);
    CreatedBy       = schedule.CreatedBy;
    CreatedAt       = schedule.CreatedAt;
    UpdatedBy       = schedule.UpdatedBy;
    UpdatedAt       = schedule.UpdatedAt;

    await LoadFormDataAsync();
    await LoadDeleteMessagesAsync(schedule.Title);
    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(int id)
  {
    await LoadFormDataAsync();

    var selectedSystems = Request.Form["chkSystems"].ToList();
    ScheduleId = id;

    if (string.IsNullOrWhiteSpace(txtTitle))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      await LoadDeleteMessagesAsync(txtTitle);
      return Page();
    }

    if (selectedSystems.Count == 0)
    {
      SetError(await _translation.GetAsync("Maintenance.AffectedSystems.Required"));
      await LoadDeleteMessagesAsync(txtTitle);
      return Page();
    }

    if (!DateTime.TryParseExact(txtStartAt, AppConstants.DateTimeInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var startLocal) ||
        !DateTime.TryParseExact(txtEndAt, AppConstants.DateTimeInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var endLocal))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      await LoadDeleteMessagesAsync(txtTitle);
      return Page();
    }

    if (startLocal >= endLocal)
    {
      SetError(await _translation.GetAsync("Maintenance.DateRangeError"));
      await LoadDeleteMessagesAsync(txtTitle);
      return Page();
    }

    var startUtc = startLocal.ToUtcFromUserTimezone(UserTimezone);
    var endUtc   = endLocal.ToUtcFromUserTimezone(UserTimezone);

    var messages = Languages
        .Select(l => new MaintenanceScheduleMessage
        {
          LanguageCode = l.LanguageCode,
          Message      = Request.Form[$"txtMessage_{l.LanguageCode}"].ToString()
        })
        .ToList();

    var schedule = new MaintenanceSchedule
    {
      Id      = id,
      Title   = txtTitle.Trim(),
      StartAt = startUtc,
      EndAt   = endUtc
    };

    await _dbHelper.UpdateAsync(schedule, selectedSystems!, messages, CurrentUsername);
    _maintenanceService.InvalidateCache();

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    return RedirectToPage(Routes.AdminMaintenances);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _dbHelper.UpdateStatusAsync(id, StatusConstants.Deleted, CurrentUsername);
      _maintenanceService.InvalidateCache();
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task LoadFormDataAsync()
  {
    Systems   = await _systemDbHelper.GetAllActiveAsync();
    Languages = await _languageDbHelper.GetAllActiveAsync();
  }

  private async Task LoadDeleteMessagesAsync(string entityName)
  {
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");
  }

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
