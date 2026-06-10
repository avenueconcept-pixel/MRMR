using Microsoft.AspNetCore.Mvc;
using MyApp.Constants.MRMR;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;
using MyApp.Services.MRMR;
using System.Text.Json;

namespace MyApp.Areas.Applicant.Pages.Submission;

public class DocumentSlot
{
    public string  DocType    { get; init; } = string.Empty;
    public bool    IsRequired { get; init; }
    public string  DisplayKey { get; init; } = string.Empty;
    public ApplicationDocument? Uploaded { get; set; }
}

public class SectionKModel : ApplicantPageModel
{
    private readonly SubmissionDbHelper _dbHelper;
    private readonly TranslationService _translation;
    private readonly FileUploadService  _fileUpload;

    public SectionKModel(SubmissionDbHelper dbHelper, TranslationService translation, FileUploadService fileUpload)
    {
        _dbHelper    = dbHelper;
        _translation = translation;
        _fileUpload  = fileUpload;
    }

    [BindProperty(SupportsGet = true)] public string ApplicationId { get; set; } = string.Empty;

    [BindProperty] public IFormFile? UploadFile         { get; set; }
    [BindProperty] public string     UploadDocumentType { get; set; } = string.Empty;
    [BindProperty] public int        DeleteDocumentId   { get; set; }

    public ApplicationSubmission? Submission        { get; set; }
    public Application?           CurrentApplication { get; set; }
    public List<DocumentSlot>     Slots             { get; set; } = [];
    public bool                   RequiredComplete  { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);
        await BuildSlotsAsync(CurrentApplication);
        SetViewData(CurrentApplication, Submission);
        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null)
            return new JsonResult(new { success = false, message = "Application not found." });

        if (CurrentApplication.IsFinalSubmitted)
            return new JsonResult(new { success = false, message = "Submission is locked." });

        if (UploadFile == null || UploadFile.Length == 0)
            return new JsonResult(new { success = false, message = "No file selected." });

        if (!GetAllDocTypes().Contains(UploadDocumentType))
            return new JsonResult(new { success = false, message = "Invalid document type." });

        try
        {
            var existing = await _dbHelper.GetDocumentByTypeAsync(CurrentApplication.Id, UploadDocumentType);
            if (existing != null)
            {
                _fileUpload.DeleteFile(existing.FilePath);
                await _dbHelper.DeleteApplicationDocumentRecordAsync(existing.Id);
            }

            var filePath = await _fileUpload.SaveApplicationDocumentAsync(
                UploadFile, ApplicationId, UploadDocumentType);

            var doc = new ApplicationDocument
            {
                ApplicationId      = CurrentApplication.Id,
                DocumentType       = UploadDocumentType,
                OriginalFilename   = UploadFile.FileName,
                FilePath           = filePath,
                FileSizeBytes      = (int)UploadFile.Length,
                MimeType           = UploadFile.ContentType,
                IsRequired         = IsRequiredForAppType(UploadDocumentType, CurrentApplication.ApplicationType),
                VerificationStatus = "Pending"
            };

            await _dbHelper.InsertApplicationDocumentAsync(doc);

            return new JsonResult(new
            {
                success            = true,
                filename           = UploadFile.FileName,
                fileSizeKb         = (int)(UploadFile.Length / 1024),
                uploadedAt         = DateTime.UtcNow.ToString("dd MMM yyyy HH:mm"),
                verificationStatus = "Pending"
            });
        }
        catch (InvalidOperationException ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
        catch (Exception)
        {
            return new JsonResult(new { success = false, message = "Upload failed. Please try again." });
        }
    }

    public async Task<IActionResult> OnPostDeleteDocumentAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null)
            return new JsonResult(new { success = false, message = "Application not found." });

        if (CurrentApplication.IsFinalSubmitted)
            return new JsonResult(new { success = false, message = "Submission is locked." });

        var doc = await _dbHelper.GetApplicationDocumentAsync(DeleteDocumentId);
        if (doc == null || doc.ApplicationId != CurrentApplication.Id)
            return new JsonResult(new { success = false, message = "Document not found." });

        if (doc.VerificationStatus == "Verified")
            return new JsonResult(new { success = false, message = "Verified documents cannot be deleted." });

        _fileUpload.DeleteFile(doc.FilePath);
        await _dbHelper.DeleteApplicationDocumentRecordAsync(doc.Id);

        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostSaveNextAsync()
    {
        CurrentApplication = await _dbHelper.GetApplicationByStringIdAsync(ApplicationId);
        if (CurrentApplication == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        var complete = await _dbHelper.AreRequiredDocumentsCompleteAsync(
            CurrentApplication.Id, CurrentApplication.ApplicationType);

        if (!complete)
        {
            Submission = await _dbHelper.GetOrCreateSubmissionAsync(CurrentApplication.Id);
            await BuildSlotsAsync(CurrentApplication);
            SetViewData(CurrentApplication, Submission);
            AlertMessageContent = await _translation.GetAsync("Submission.SectionK.RequiredError");
            AlertMessageType    = "error";
            return Page();
        }

        await _dbHelper.SaveJsonbSectionAsync(
            CurrentApplication.Id, SubmissionConstants.SectionK, "{\"complete\":true}", true);

        return RedirectToPage("/Submission/SectionL",
            new { area = "Applicant", applicationId = ApplicationId });
    }

    public IActionResult OnPostBackAsync()
        => RedirectToPage("/Submission/SectionJ",
            new { area = "Applicant", applicationId = ApplicationId });

    private async Task BuildSlotsAsync(Application app)
    {
        var uploaded  = await _dbHelper.GetApplicationDocumentsAsync(app.Id);
        bool isCorporate = app.ApplicationType == "Corporate";

        var slotDefs = new List<(string DocType, bool Required, string DisplayKey)>
        {
            (DocumentTypeConstants.IcPassport,      true, "Submission.SectionK.Doc.IcPassport"),
            (DocumentTypeConstants.BusinessProfile,  true, "Submission.SectionK.Doc.BusinessProfile"),
            (DocumentTypeConstants.TaxCompliance,    true, "Submission.SectionK.Doc.TaxCompliance"),
        };

        if (isCorporate)
        {
            slotDefs.Insert(1, (DocumentTypeConstants.CompanyRegistration, true, "Submission.SectionK.Doc.CompanyRegistration"));
            slotDefs.Insert(2, (DocumentTypeConstants.AuditorReport,       true, "Submission.SectionK.Doc.AuditorReport"));
        }

        slotDefs.Add((DocumentTypeConstants.SupportingDocument1, false, "Submission.SectionK.Doc.Supporting1"));
        slotDefs.Add((DocumentTypeConstants.SupportingDocument2, false, "Submission.SectionK.Doc.Supporting2"));
        slotDefs.Add((DocumentTypeConstants.SupportingDocument3, false, "Submission.SectionK.Doc.Supporting3"));
        slotDefs.Add((DocumentTypeConstants.SupportingDocument4, false, "Submission.SectionK.Doc.Supporting4"));
        slotDefs.Add((DocumentTypeConstants.SupportingDocument5, false, "Submission.SectionK.Doc.Supporting5"));

        Slots = slotDefs.Select(def => new DocumentSlot
        {
            DocType    = def.DocType,
            IsRequired = def.Required,
            DisplayKey = def.DisplayKey,
            Uploaded   = uploaded.FirstOrDefault(u => u.DocumentType == def.DocType)
        }).ToList();

        RequiredComplete = Slots
            .Where(s => s.IsRequired)
            .All(s => s.Uploaded != null);
    }

    private static bool IsRequiredForAppType(string docType, string appType)
    {
        var required = appType == "Corporate"
            ? DocumentTypeConstants.RequiredCorporate
            : DocumentTypeConstants.RequiredIndividual;
        return required.Contains(docType);
    }

    private static IEnumerable<string> GetAllDocTypes()
        =>
        [
            DocumentTypeConstants.IcPassport,
            DocumentTypeConstants.CompanyRegistration,
            DocumentTypeConstants.AuditorReport,
            DocumentTypeConstants.BusinessProfile,
            DocumentTypeConstants.TaxCompliance,
            DocumentTypeConstants.SupportingDocument1,
            DocumentTypeConstants.SupportingDocument2,
            DocumentTypeConstants.SupportingDocument3,
            DocumentTypeConstants.SupportingDocument4,
            DocumentTypeConstants.SupportingDocument5
        ];

    private void SetViewData(Application app, ApplicationSubmission? sub)
    {
        ViewData["ApplicationId"]   = ApplicationId;
        ViewData["CurrentSection"]  = "SectionK";
        ViewData["Submission"]      = sub;
        ViewData["ApplicationType"] = app.ApplicationType;
    }
}
