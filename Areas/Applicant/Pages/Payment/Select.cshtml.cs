using Microsoft.AspNetCore.Mvc;
using MyApp.Constants.MRMR;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;

namespace MyApp.Areas.Applicant.Pages.Payment;

public class SelectModel : BasePageModel
{
    private readonly RegistrationDbHelper _dbHelper;
    private readonly IConfiguration       _config;

    public SelectModel(RegistrationDbHelper dbHelper, IConfiguration config)
    {
        _dbHelper = dbHelper;
        _config   = config;
    }

    [BindProperty(SupportsGet = true)] public int? PaymentId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var appDbId = TempData["RegisteredApplicationId"] as int?;
        var method  = TempData["RegisteredPaymentMethod"]  as string;

        if (PaymentId.HasValue && (appDbId == null || string.IsNullOrEmpty(method)))
        {
            var dbPayment = await _dbHelper.GetPaymentByIdAsync(PaymentId.Value);
            if (dbPayment == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

            var application2 = await _dbHelper.GetApplicationByDbIdAsync(dbPayment.ApplicationId);
            if (application2 == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });

            appDbId = application2.Id;
            method  = dbPayment.Method;
        }

        if (appDbId == null || string.IsNullOrEmpty(method))
            return RedirectToPage("/Register", new { area = "Applicant" });

        var application = await _dbHelper.GetApplicationByDbIdAsync(appDbId.Value);
        if (application == null)
            return RedirectToPage("/Register", new { area = "Applicant" });

        var payment = await _dbHelper.GetPaymentAsync(appDbId.Value, nameof(PaymentType.NominationFee));
        if (payment == null)
            return RedirectToPage("/Register", new { area = "Applicant" });

        if (method == nameof(PaymentMethod.Axaipay))
        {
            var gatewayUrl  = _config["Axaipay:GatewayUrl"]!;
            var merchantId  = _config["Axaipay:MerchantId"]!;
            var secretKey   = _config["Axaipay:SecretKey"]!;
            var callbackUrl = _config["Axaipay:CallbackUrl"]!;
            var returnUrl   = _config["Axaipay:ReturnUrl"]!;

            var queryParams = new Dictionary<string, string?>
            {
                ["merchant_id"]    = merchantId,
                ["order_id"]       = application.ApplicationId,
                ["amount"]         = payment.Amount.ToString("F2"),
                ["currency"]       = PaymentConstants.Currency,
                ["customer_name"]  = application.Registrant.FullName,
                ["customer_email"] = application.Registrant.Email,
                ["callback_url"]   = callbackUrl,
                ["return_url"]     = returnUrl,
                ["description"]    = $"MRMR2026 Nomination Fee - {application.ApplicationId}"
            };

            var signatureInput = $"{application.ApplicationId}{payment.Amount:F2}{secretKey}";
            queryParams["signature"] = ComputeHmacSha256(signatureInput, secretKey);

            var redirectUrl = gatewayUrl + "?" + string.Join("&",
                queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value ?? "")}"));

            TempData["AxaipayPaymentId"] = payment.Id;
            return Redirect(redirectUrl);
        }
        else
        {
            TempData["ManualPaymentId"]     = payment.Id;
            TempData["ManualApplicationId"] = application.ApplicationId;
            TempData["ManualAmount"]        = payment.Amount;
            return RedirectToPage("/Payment/Manual", new { area = "Applicant" });
        }
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLower();
    }
}
