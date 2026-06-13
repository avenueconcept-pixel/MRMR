using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Applications;

public class DetailModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public DetailModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty(SupportsGet = true)] public int    Id             { get; set; }
    [BindProperty]                     public string OverrideStatus { get; set; } = string.Empty;
    [BindProperty]                     public string OverrideReason { get; set; } = string.Empty;

    public Application? Application { get; set; }

    public IEnumerable<string> AllStatuses => Enum.GetNames<MyApp.Constants.MRMR.ApplicationStatus>();

    public async Task<IActionResult> OnGetAsync()
    {
        Application = await _mrmrDb.GetApplicationDetailAsync(Id);
        if (Application == null) return RedirectToPage("Index");
        return Page();
    }

    public async Task<IActionResult> OnPostOverrideStatusAsync()
    {
        Application = await _mrmrDb.GetApplicationDetailAsync(Id);
        if (Application == null) return RedirectToPage("Index");

        if (string.IsNullOrWhiteSpace(OverrideStatus))
        {
            AlertMessageContent = "Please select a status.";
            AlertMessageType    = "error";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(OverrideReason))
        {
            AlertMessageContent = "Reason for override is required.";
            AlertMessageType    = "error";
            return Page();
        }

        if (OverrideStatus == Application.Status)
        {
            AlertMessageContent = "The selected status is the same as the current status.";
            AlertMessageType    = "error";
            return Page();
        }

        try
        {
            await _mrmrDb.OverrideApplicationStatusAsync(Id, OverrideStatus, CurrentUserId, OverrideReason);
            TempData["SuccessMessage"] = $"Status updated to '{OverrideStatus}' successfully.";
            return RedirectToPage("Detail", new { id = Id });
        }
        catch (InvalidOperationException ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = "error";
            return Page();
        }
    }
}
