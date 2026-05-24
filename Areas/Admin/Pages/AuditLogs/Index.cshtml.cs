using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.AuditLogs;

public class IndexModel : AdminPageModel
{
  private readonly AuditLogDbHelper   _auditLogDb;
  private readonly TranslationService _translation;

  public List<AuditLog>       AuditLogs       { get; set; } = new();
  public List<SelectListItem> TableNameOptions { get; set; } = new();
  public List<SelectListItem> ActionOptions    { get; set; } = new();

  [BindProperty(SupportsGet = true)] public string? FilterTableName { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterAction    { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterChangedBy { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterDateFrom  { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterDateTo    { get; set; }

  public IndexModel(AuditLogDbHelper auditLogDb, TranslationService translation)
  {
    _auditLogDb  = auditLogDb;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    FilterDateFrom ??= DateTime.UtcNow.AddDays(-7)
                        .ToUserLocalTime(UserTimezone, AppConstants.DateInputFormat);
    FilterDateTo   ??= DateTime.UtcNow
                        .ToUserLocalTime(UserTimezone, AppConstants.DateInputFormat);

    await LoadDataAsync();
  }

  public async Task<IActionResult> OnPostFilterAsync()
  {
    await LoadDataAsync();
    return Page();
  }

  private async Task LoadDataAsync()
  {
    var dateFrom = DateTime.TryParse(FilterDateFrom, out var df)
        ? df.ToUtcFromUserTimezone(UserTimezone)
        : DateTime.UtcNow.AddDays(-7);

    var dateTo = DateTime.TryParse(FilterDateTo, out var dt)
        ? dt.AddDays(1).AddSeconds(-1).ToUtcFromUserTimezone(UserTimezone)
        : DateTime.UtcNow;

    AuditLogs = await _auditLogDb.GetByFilterAsync(
        dateFrom, dateTo,
        FilterTableName,
        FilterAction,
        FilterChangedBy);

    await LoadFilterOptionsAsync();
  }

  private async Task LoadFilterOptionsAsync()
  {
    var tables = await _auditLogDb.GetDistinctTableNamesAsync();
    TableNameOptions = tables.Select(t => new SelectListItem
    {
      Value = t,
      Text  = t
    }).ToList();

    ActionOptions = new List<SelectListItem>
    {
      new() { Value = AuditConstants.Actions.Insert,  Text = AuditConstants.Actions.Insert  },
      new() { Value = AuditConstants.Actions.Update,  Text = AuditConstants.Actions.Update  },
      new() { Value = AuditConstants.Actions.Delete,  Text = AuditConstants.Actions.Delete  },
      new() { Value = AuditConstants.Actions.Restore, Text = AuditConstants.Actions.Restore },
      new() { Value = AuditConstants.Actions.Login,   Text = AuditConstants.Actions.Login   },
      new() { Value = AuditConstants.Actions.Logout,  Text = AuditConstants.Actions.Logout  }
    };
  }
}
