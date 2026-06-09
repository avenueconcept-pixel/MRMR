using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Services;
using MyApp.Services.MRMR;

namespace MyApp.Areas.Applicant.Pages.Payment;

public class ManualModel : BasePageModel
{
    private readonly RegistrationDbHelper _dbHelper;
    private readonly FileUploadService    _fileUpload;
    private readonly TranslationService   _translation;
    private readonly IConfiguration       _config;

    public ManualModel(RegistrationDbHelper dbHelper, FileUploadService fileUpload,
        TranslationService translation, IConfiguration config)
    {
        _dbHelper    = dbHelper;
        _fileUpload  = fileUpload;
        _translation = translation;
        _config      = config;
    }

    [BindProperty] public IFormFile? fileSlip { get; set; }

    public string  ApplicationId   { get; set; } = string.Empty;
    public decimal Amount          { get; set; }
    public int     PaymentId       { get; set; }
    public string  BankName        { get; set; } = string.Empty;
    public string  AccountName     { get; set; } = string.Empty;
    public string  AccountNo       { get; set; } = string.Empty;
    public string  ReferenceFormat { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var paymentId     = TempData.Peek("ManualPaymentId")     as int?;
        var applicationId = TempData.Peek("ManualApplicationId") as string;
        var amount        = TempData.Peek("ManualAmount")        as decimal?;

        if (paymentId == null || string.IsNullOrEmpty(applicationId))
            return RedirectToPage("/Register", new { area = "Applicant" });

        TempData.Keep("ManualPaymentId");
        TempData.Keep("ManualApplicationId");
        TempData.Keep("ManualAmount");

        PaymentId     = paymentId.Value;
        ApplicationId = applicationId;
        Amount        = amount ?? MyApp.Constants.MRMR.PaymentConstants.NominationFeeAmount;
        LoadBankDetails();
        return Page();
    }

    public async Task<IActionResult> OnPostUploadSlipAsync()
    {
        var paymentId     = TempData["ManualPaymentId"]     as int?;
        var applicationId = TempData["ManualApplicationId"] as string;
        var amount        = TempData["ManualAmount"]        as decimal?;

        if (paymentId == null)
            return RedirectToPage("/Register", new { area = "Applicant" });

        PaymentId     = paymentId.Value;
        ApplicationId = applicationId ?? string.Empty;
        Amount        = amount ?? MyApp.Constants.MRMR.PaymentConstants.NominationFeeAmount;
        LoadBankDetails();

        if (fileSlip == null || fileSlip.Length == 0)
        {
            AlertMessageContent = await _translation.GetAsync("Payment.SlipRequired");
            AlertMessageType    = MyApp.Constants.MessageType.Error;
            return Page();
        }

        try
        {
            var filePath = await _fileUpload.SaveApplicationDocumentAsync(
                fileSlip, applicationId ?? "unknown", "payment_slip");

            await _dbHelper.SaveSlipUploadAsync(paymentId.Value, filePath);
            return RedirectToPage("/Payment/Pending", new { area = "Applicant" });
        }
        catch (InvalidOperationException ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = MyApp.Constants.MessageType.Error;
            return Page();
        }
    }

    private void LoadBankDetails()
    {
        BankName        = _config["BankTransfer:BankName"]        ?? string.Empty;
        AccountName     = _config["BankTransfer:AccountName"]     ?? string.Empty;
        AccountNo       = _config["BankTransfer:AccountNo"]       ?? string.Empty;
        ReferenceFormat = _config["BankTransfer:ReferenceFormat"] ?? string.Empty;
    }
}
