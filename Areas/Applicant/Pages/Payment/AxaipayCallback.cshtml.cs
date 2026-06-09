using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Constants.MRMR;
using MyApp.Services;

namespace MyApp.Areas.Applicant.Pages.Payment;

[IgnoreAntiforgeryToken]
public class AxaipayCallbackModel : BasePageModel
{
    private readonly RegistrationDbHelper          _dbHelper;
    private readonly IConfiguration                _config;
    private readonly EmailService                  _emailService;
    private readonly ILogger<AxaipayCallbackModel> _logger;

    public AxaipayCallbackModel(RegistrationDbHelper dbHelper, IConfiguration config,
        EmailService emailService, ILogger<AxaipayCallbackModel> logger)
    {
        _dbHelper     = dbHelper;
        _config       = config;
        _emailService = emailService;
        _logger       = logger;
    }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var orderId   = Request.Form["order_id"].ToString();
            var refNo     = Request.Form["transaction_id"].ToString();
            var status    = Request.Form["status"].ToString();
            var signature = Request.Form["signature"].ToString();
            var payload   = System.Text.Json.JsonSerializer.Serialize(
                Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString()));

            var secretKey = _config["Axaipay:SecretKey"]!;
            var expected  = ComputeHmacSha256($"{orderId}{refNo}{secretKey}", secretKey);
            if (!string.Equals(signature, expected, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Axaipay callback signature mismatch for order {OrderId}", orderId);
                return new BadRequestObjectResult("Invalid signature");
            }

            if (status?.ToLower() != "success")
            {
                _logger.LogInformation("Axaipay callback non-success for order {OrderId}: {Status}", orderId, status);
                return new OkResult();
            }

            var app = await _dbHelper.GetApplicationByApplicationIdAsync(orderId);
            if (app == null)
            {
                _logger.LogWarning("Axaipay callback: application not found for order {OrderId}", orderId);
                return new OkResult();
            }

            var payment = await _dbHelper.GetPaymentAsync(app.Id, nameof(PaymentType.NominationFee));
            if (payment == null) return new OkResult();

            await _dbHelper.UpdatePaymentAxaipayAsync(payment.Id, refNo, payload);

            if (payment.PaymentType == nameof(PaymentType.NominationFee))
            {
                await _dbHelper.CreateAwardFeePaymentAsync(payment.ApplicationId);

                var appWithRegistrant = await _dbHelper.GetApplicationByDbIdAsync(payment.ApplicationId);
                if (appWithRegistrant?.Registrant != null)
                {
                    var reg  = appWithRegistrant.Registrant;
                    var lang = reg.PreferredLang ?? "en";
                    await _emailService.SendApplicantCredentialsAsync(
                        reg.Email,
                        reg.FullName,
                        reg.Username ?? reg.Email,
                        reg.TempPassword ?? string.Empty,
                        lang);
                }
            }

            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Axaipay callback");
            return new StatusCodeResult(500);
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
