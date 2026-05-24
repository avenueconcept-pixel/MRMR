using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Logs;

public class IndexModel : AdminPageModel
{
  private readonly LogDbHelper        _logDb;
  private readonly TranslationService _translation;

  public List<AppLog>         Logs            { get; set; } = new();
  public List<SelectListItem> LogLevelOptions { get; set; } = new();

  [BindProperty(SupportsGet = true)]
  public string? FilterLogLevel { get; set; }

  [BindProperty(SupportsGet = true)]
  public string? FilterDateFrom { get; set; }

  [BindProperty(SupportsGet = true)]
  public string? FilterDateTo { get; set; }

  public IndexModel(LogDbHelper logDb, TranslationService translation)
  {
    _logDb       = logDb;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    FilterDateFrom ??= DateTime.UtcNow.AddHours(-24)
                        .ToUserLocalTime(UserTimezone, AppConstants.DateInputFormat);
    FilterDateTo   ??= DateTime.UtcNow
                        .ToUserLocalTime(UserTimezone, AppConstants.DateInputFormat);

    await LoadLogsAsync();
    await LoadLogLevelOptionsAsync();
  }

  public async Task<IActionResult> OnPostFilterAsync()
  {
    await LoadLogsAsync();
    await LoadLogLevelOptionsAsync();
    return Page();
  }

  private async Task LoadLogsAsync()
  {
    var dateFrom = DateTime.TryParse(FilterDateFrom, out var df)
        ? df.ToUtcFromUserTimezone(UserTimezone)
        : DateTime.UtcNow.AddHours(-24);

    var dateTo = DateTime.TryParse(FilterDateTo, out var dt)
        ? dt.AddDays(1).AddSeconds(-1).ToUtcFromUserTimezone(UserTimezone)
        : DateTime.UtcNow;

    Logs = await _logDb.GetByFilterAsync(dateFrom, dateTo, FilterLogLevel);
  }

  private async Task LoadLogLevelOptionsAsync()
  {
    var levels = await _logDb.GetDistinctLogLevelsAsync();
    LogLevelOptions = levels.Select(l => new SelectListItem { Value = l, Text = l }).ToList();
  }
}
