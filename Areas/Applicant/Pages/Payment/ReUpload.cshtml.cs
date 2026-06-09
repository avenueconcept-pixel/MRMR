using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Services;
using MyApp.Services.MRMR;

namespace MyApp.Areas.Applicant.Pages.Payment;

public class ReUploadModel : ApplicantPageModel
{
    private readonly RegistrationDbHelper _dbHelper;
    private readonly FileUploadService    _fileUpload;
    private readonly TranslationService   _translation;

    public ReUploadModel(RegistrationDbHelper dbHelper, FileUploadService fileUpload,
        TranslationService translation)
    {
        _dbHelper    = dbHelper;
        _fileUpload  = fileUpload;
        _translation = translation;
    }

    [BindProperty(SupportsGet = true)] public int PaymentId { get; set; }
    [BindProperty] public IFormFile? fileSlip { get; set; }

    public MyApp.Models.MRMR.Payment? Payment { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Payment = await _dbHelper.GetPaymentFullAsync(PaymentId);
        if (Payment == null || Payment.Status != nameof(MyApp.Constants.MRMR.PaymentStatus.Rejected))
            return RedirectToPage("/Dashboard", new { area = "Applicant" });
        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        Payment = await _dbHelper.GetPaymentFullAsync(PaymentId);
        if (Payment == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

        if (fileSlip == null || fileSlip.Length == 0)
        {
            AlertMessageContent = await _translation.GetAsync("Payment.SlipRequired");
            AlertMessageType    = "error";
            return Page();
        }

        try
        {
            var applicationId = Payment.Application?.ApplicationId ?? "unknown";
            var filePath = await _fileUpload.SaveApplicationDocumentAsync(
                fileSlip, applicationId, "payment_slip_reupload");
            await _dbHelper.ReUploadSlipAsync(PaymentId, filePath);
            return RedirectToPage("/Dashboard", new { area = "Applicant" });
        }
        catch (InvalidOperationException ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = "error";
            return Page();
        }
    }
}
