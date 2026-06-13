using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;
using MyApp.Services.MRMR;
using MyApp.Constants.MRMR;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class DocReUploadModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;
    private readonly FileUploadService  _fileUpload;
    private readonly TranslationService _translation;

    public DocReUploadModel(SubmissionDbHelper dbHelper, FileUploadService fileUpload, TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _fileUpload  = fileUpload;
        _translation = translation;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;
    [BindProperty(SupportsGet = true)] public string DocumentType  { get; set; } = string.Empty;

    [BindProperty] public IFormFile? UploadFile { get; set; }

    public Application?         CurrentApplication { get; set; }
    public ApplicationDocument? ExistingDoc        { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await LoadAndValidateAsync()) return RedirectToPage("/Dashboard", new { area = "Applicant" });
        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (!await LoadAndValidateAsync()) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        if (UploadFile == null || UploadFile.Length == 0)
        {
            AlertMessageContent = await _translation.GetAsync("Submission.SectionK.UploadFailed");
            AlertMessageType    = "error";
            return Page();
        }

        try
        {
            if (ExistingDoc != null)
            {
                _fileUpload.DeleteFile(ExistingDoc.FilePath);
                await _dbHelper.DeleteApplicationDocumentRecordAsync(ExistingDoc.Id);
            }

            var filePath = await _fileUpload.SaveApplicationDocumentAsync(
                UploadFile, ApplicationId, DocumentType);

            var isRequired = IsRequiredForAppType(DocumentType, CurrentApplication!.ApplicationType);

            var doc = new ApplicationDocument
            {
                ApplicationId      = CurrentApplication!.Id,
                DocumentType       = DocumentType,
                OriginalFilename   = UploadFile.FileName,
                FilePath           = filePath,
                FileSizeBytes      = (int)UploadFile.Length,
                MimeType           = UploadFile.ContentType,
                IsRequired         = isRequired,
                VerificationStatus = "Pending"
            };

            await _dbHelper.InsertApplicationDocumentAsync(doc);

            TempData["SuccessMessage"] = await _translation.GetAsync("Dashboard.DocVerification.ReUploadSuccess");
            return RedirectToPage("/Dashboard", new { area = "Applicant" });
        }
        catch (InvalidOperationException ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = "error";
            return Page();
        }
        catch
        {
            AlertMessageContent = await _translation.GetAsync("Submission.SectionK.UploadFailed");
            AlertMessageType    = "error";
            return Page();
        }
    }

    private async Task<bool> LoadAndValidateAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);

        if (CurrentApplication == null || !CurrentApplication.IsFinalSubmitted)
            return false;

        ExistingDoc = await _dbHelper.GetDocumentByTypeAsync(CurrentApplication.Id, DocumentType);
        if (ExistingDoc == null || ExistingDoc.VerificationStatus != "Rejected")
            return false;

        return true;
    }

    private static bool IsRequiredForAppType(string docType, string appType)
    {
        var required = appType == "Corporate"
            ? DocumentTypeConstants.RequiredCorporate
            : DocumentTypeConstants.RequiredIndividual;
        return required.Contains(docType);
    }
}
