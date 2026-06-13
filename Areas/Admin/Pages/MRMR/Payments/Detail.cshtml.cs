using Microsoft.AspNetCore.Mvc;
using MyApp.Constants.MRMR;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.MRMR.Payments;

public class DetailModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;
    private readonly EmailService      _emailService;

    public DetailModel(AdminMrmrDbHelper mrmrDb, EmailService emailService)
    {
        _mrmrDb       = mrmrDb;
        _emailService = emailService;
    }

    [BindProperty(SupportsGet = true)] public int    PaymentId     { get; set; }
    [BindProperty]                     public string RejectRemarks { get; set; } = string.Empty;

    public Payment? Payment { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Payment = await _mrmrDb.GetPaymentForAdminAsync(PaymentId);
        if (Payment == null) return RedirectToPage("Index");
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync()
    {
        Payment = await _mrmrDb.GetPaymentForAdminAsync(PaymentId);
        if (Payment == null) return RedirectToPage("Index");

        if (Payment.Status != nameof(PaymentStatus.PendingVerification))
        {
            AlertMessageContent = "This payment is no longer pending verification.";
            AlertMessageType    = "error";
            return Page();
        }

        try
        {
            await _mrmrDb.ApprovePaymentAsync(PaymentId, CurrentUserId);

            var reg  = Payment.Application?.Registrant;
            var lang = reg?.PreferredLang ?? "en";

            if (reg != null)
            {
                if (Payment.PaymentType == nameof(PaymentType.NominationFee))
                {
                    await _emailService.SendApplicantCredentialsAsync(
                        reg.Email,
                        reg.FullName,
                        reg.Username ?? reg.Email,
                        reg.TempPassword ?? string.Empty,
                        lang);
                }
                else
                {
                    await _emailService.SendApplicantSubmissionUnlockedAsync(
                        reg.Email,
                        reg.FullName,
                        Payment.Application!.ApplicationId,
                        lang);
                }
            }

            TempData["SuccessMessage"] = $"Payment {Payment.InvoiceNo} approved successfully.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = "error";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRejectAsync()
    {
        Payment = await _mrmrDb.GetPaymentForAdminAsync(PaymentId);
        if (Payment == null) return RedirectToPage("Index");

        if (string.IsNullOrWhiteSpace(RejectRemarks))
        {
            AlertMessageContent = "Rejection remarks are required.";
            AlertMessageType    = "error";
            return Page();
        }

        if (Payment.Status != nameof(PaymentStatus.PendingVerification))
        {
            AlertMessageContent = "This payment is no longer pending verification.";
            AlertMessageType    = "error";
            return Page();
        }

        try
        {
            await _mrmrDb.RejectPaymentAsync(PaymentId, CurrentUserId, RejectRemarks);

            var reg  = Payment.Application?.Registrant;
            var lang = reg?.PreferredLang ?? "en";

            if (reg != null)
            {
                await _emailService.SendApplicantPaymentRejectedAsync(
                    reg.Email,
                    reg.FullName,
                    Payment.PaymentType,
                    RejectRemarks,
                    lang);
            }

            TempData["SuccessMessage"] = $"Payment {Payment.InvoiceNo} rejected.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = "error";
            return Page();
        }
    }
}
