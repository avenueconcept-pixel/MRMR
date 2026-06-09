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
        Models.MRMR.Payment?     payment     = null;
        Models.MRMR.Application? application = null;

        if (PaymentId.HasValue)
        {
            // Path A: Dashboard — paymentId in query string
            payment = await _dbHelper.GetPaymentFullAsync(PaymentId.Value);
            if (payment == null) return RedirectToPage("/Dashboard", new { area = "Applicant" });
            application = payment.Application;
        }
        else
        {
            // Path B: Post-registration — from TempData
            var appDbId = TempData["RegisteredApplicationId"] as int?;
            var method  = TempData["RegisteredPaymentMethod"]  as string;
            if (appDbId == null || string.IsNullOrEmpty(method))
                return RedirectToPage("/Register", new { area = "Applicant" });

            application = await _dbHelper.GetApplicationByDbIdAsync(appDbId.Value);
            if (application == null) return RedirectToPage("/Register", new { area = "Applicant" });

            payment = await _dbHelper.GetPaymentAsync(appDbId.Value, nameof(PaymentType.NominationFee));
            if (payment == null) return RedirectToPage("/Register", new { area = "Applicant" });
        }

        var paymentMethod = payment.Method;
        var amount        = payment.Amount;

        if (paymentMethod == nameof(PaymentMethod.Axaipay))
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
                ["amount"]         = amount.ToString("F2"),
                ["currency"]       = PaymentConstants.Currency,
                ["customer_name"]  = application.Registrant?.FullName ?? string.Empty,
                ["customer_email"] = application.Registrant?.Email    ?? string.Empty,
                ["callback_url"]   = callbackUrl,
                ["return_url"]     = returnUrl,
                ["description"]    = $"MRMR2026 {payment.PaymentType} - {application.ApplicationId}"
            };

            var signatureInput = $"{application.ApplicationId}{amount:F2}{secretKey}";
            queryParams["signature"] = ComputeHmacSha256(signatureInput, secretKey);

            var redirectUrl = gatewayUrl + "?" + string.Join("&",
                queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value ?? "")}"));

            return Redirect(redirectUrl);
        }
        else
        {
            TempData["ManualPaymentId"]     = payment.Id;
            TempData["ManualApplicationId"] = application.ApplicationId;
            TempData["ManualAmount"]        = amount;
            return RedirectToPage("/Payment/Manual", new { area = "Applicant" });
        }
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(
            hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data))).ToLower();
    }
}
