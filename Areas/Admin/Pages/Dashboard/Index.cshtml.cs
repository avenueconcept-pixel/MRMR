using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.Dashboard;

public class IndexModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public IndexModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    public MrmrDashboardStats    Stats               { get; set; } = new();
    public List<Application>     RecentRegistrations { get; set; } = [];
    public List<Payment>         PendingPayments     { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        AlertMessageType    = string.Empty;
        Stats               = await _mrmrDb.GetDashboardStatsAsync();
        RecentRegistrations = await _mrmrDb.GetRecentRegistrationsAsync(8);
        PendingPayments     = await _mrmrDb.GetRecentPaymentsAsync(8);
        return Page();
    }
}
