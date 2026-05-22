using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;

namespace MyApp.Areas.Admin.Pages.Logs;

public class IndexModel : AdminPageModel
{
  private readonly LogDbHelper _logDb;

  public IndexModel(LogDbHelper logDb)
  {
    _logDb = logDb;
  }

  public List<AppLog> Logs { get; set; } = new();

  public async Task OnGetAsync()
  {
    Logs = await _logDb.GetRecentAsync(500);
  }
}
