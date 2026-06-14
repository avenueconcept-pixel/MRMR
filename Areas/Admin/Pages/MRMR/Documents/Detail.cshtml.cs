using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Documents;

public class DetailModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public DetailModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty(SupportsGet = true)] public int    ApplicationId { get; set; }
    [BindProperty]                     public int    DocumentId    { get; set; }
    [BindProperty]                     public string RejectRemarks { get; set; } = string.Empty;

    public Application?              Application { get; set; }
    public List<ApplicationDocument> Documents   { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        AlertMessageType = string.Empty;
        Application = await _mrmrDb.GetApplicationDetailAsync(ApplicationId);
        if (Application == null) return RedirectToPage("/MRMR/Applications/Index");

        Documents = await _mrmrDb.GetApplicationDocumentsAsync(ApplicationId);
        return Page();
    }

    public async Task<IActionResult> OnPostVerifyAsync()
    {
        try
        {
            await _mrmrDb.VerifyDocumentAsync(DocumentId, CurrentUserId);
            TempData["SuccessMessage"] = "Document verified successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToPage(new { ApplicationId });
    }

    public async Task<IActionResult> OnPostRejectAsync()
    {
        if (string.IsNullOrWhiteSpace(RejectRemarks))
        {
            TempData["ErrorMessage"] = "Remarks are required when rejecting a document.";
            return RedirectToPage(new { ApplicationId });
        }

        try
        {
            await _mrmrDb.RejectDocumentAsync(DocumentId, CurrentUserId, RejectRemarks);
            TempData["SuccessMessage"] = "Document rejected.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToPage(new { ApplicationId });
    }
}
