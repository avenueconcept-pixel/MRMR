using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Applications;

public class IndexModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public IndexModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public string? FilterType   { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search       { get; set; }

    public List<Application> Applications { get; set; } = [];

    public IEnumerable<string> AllStatuses => Enum.GetNames<MyApp.Constants.MRMR.ApplicationStatus>();

    public async Task<IActionResult> OnGetAsync()
    {
        AlertMessageType = string.Empty;
        Applications = await _mrmrDb.GetApplicationListAsync(FilterStatus, FilterType, Search);
        return Page();
    }
}
