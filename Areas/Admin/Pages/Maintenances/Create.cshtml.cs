using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Maintenances;

public class CreateModel : AdminPageModel
{
  private readonly MaintenanceDbHelper _dbHelper;
  private readonly MaintenanceService  _maintenanceService;
  private readonly SystemDbHelper      _systemDbHelper;
  private readonly LanguageDbHelper    _languageDbHelper;
  private readonly TranslationService  _translation;

  [BindProperty] public string txtTitle   { get; set; } = string.Empty;
  [BindProperty] public string txtStartAt { get; set; } = string.Empty;
  [BindProperty] public string txtEndAt   { get; set; } = string.Empty;

  public List<AppSystem>  Systems   { get; set; } = new();
  public List<Language>   Languages { get; set; } = new();

  public CreateModel(
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

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    await LoadFormDataAsync();
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    await LoadFormDataAsync();

    var selectedSystems = Request.Form["chkSystems"].ToList();

    if (string.IsNullOrWhiteSpace(txtTitle))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    if (selectedSystems.Count == 0)
    {
      SetError(await _translation.GetAsync("Maintenance.AffectedSystems.Required"));
      return Page();
    }

    if (!DateTime.TryParseExact(txtStartAt, AppConstants.DateTimeInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var startLocal) ||
        !DateTime.TryParseExact(txtEndAt, AppConstants.DateTimeInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var endLocal))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    if (startLocal >= endLocal)
    {
      SetError(await _translation.GetAsync("Maintenance.DateRangeError"));
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
      Title    = txtTitle.Trim(),
      StartAt  = startUtc,
      EndAt    = endUtc,
      IsActive = true,
      Status   = StatusConstants.Active
    };

    await _dbHelper.AddAsync(schedule, selectedSystems!, messages, CurrentUsername);
    _maintenanceService.InvalidateCache();

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveSuccess);
    return RedirectToPage(Routes.AdminMaintenances);
  }

  private async Task LoadFormDataAsync()
  {
    Systems   = await _systemDbHelper.GetAllActiveAsync();
    Languages = await _languageDbHelper.GetAllActiveAsync();
  }

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
