using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;
using PageAccessHistoryModel = MyApp.Models.PageAccessHistory;

namespace MyApp.Areas.Admin.Pages.PageAccessHistory;

public class IndexModel : AdminPageModel
{
  private readonly PageAccessDbHelper  _dbHelper;
  private readonly TranslationService  _translation;

  [BindProperty(SupportsGet = true)] public string? FilterUsername   { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterSystemType { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterPageUrl    { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterStartDate  { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterEndDate    { get; set; }
  [BindProperty(SupportsGet = true)] public int     CurrentPage      { get; set; } = 1;

  public List<PageAccessHistoryModel> Items      { get; set; } = new();
  public int                          TotalCount { get; set; }
  public int                          PageSize   => 50;
  public int                          TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

  public SelectList ddlSystemType { get; set; } = default!;

  public IndexModel(PageAccessDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = string.Empty;

    DateTime? startDate = null;
    DateTime? endDate   = null;

    if (!string.IsNullOrEmpty(FilterStartDate) &&
        DateTime.TryParseExact(FilterStartDate, AppConstants.DateInputFormat,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var sd))
    {
      startDate = sd.ToUtcFromUserTimezone(UserTimezone);
    }

    if (!string.IsNullOrEmpty(FilterEndDate) &&
        DateTime.TryParseExact(FilterEndDate, AppConstants.DateInputFormat,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var ed))
    {
      endDate = ed.AddDays(1).AddSeconds(-1).ToUtcFromUserTimezone(UserTimezone);
    }

    if (CurrentPage < 1) CurrentPage = 1;

    (Items, TotalCount) = await _dbHelper.GetPagedAsync(
        FilterUsername,
        FilterSystemType,
        FilterPageUrl,
        startDate,
        endDate,
        CurrentPage,
        PageSize);

    var allSystems = await _translation.GetAsync("PageAccessLog.AllSystems");
    ddlSystemType = new SelectList(new[]
    {
      new { Value = string.Empty,                    Text = allSystems },
      new { Value = AppConstants.SystemTypeAdmin,    Text = "Admin" },
      new { Value = AppConstants.SystemTypeCustomer, Text = "Customer" }
    }, "Value", "Text", FilterSystemType);
  }
}
