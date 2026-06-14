using Microsoft.AspNetCore.Mvc;
using MyApp.Constants.MRMR;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Documents;

public class IndexModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public IndexModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search       { get; set; }

    public List<ApplicationDocument> Documents   { get; set; } = [];

    public List<string> AllStatuses { get; set; } =
    [
        nameof(DocumentVerificationStatus.Pending),
        nameof(DocumentVerificationStatus.Verified),
        nameof(DocumentVerificationStatus.Rejected)
    ];

    public async Task<IActionResult> OnGetAsync()
    {
        AlertMessageType = string.Empty;
        Documents = await _mrmrDb.GetAllDocumentsAsync(FilterStatus, Search);
        return Page();
    }
}
