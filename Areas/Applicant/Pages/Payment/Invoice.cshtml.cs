using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;

namespace MyApp.Areas.Applicant.Pages.Payment;

public class InvoiceModel : ApplicantPageModel
{
    private readonly RegistrationDbHelper _dbHelper;

    public InvoiceModel(RegistrationDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    [BindProperty(SupportsGet = true)] public int PaymentId { get; set; }
    public MyApp.Models.MRMR.Payment? Payment { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Payment = await _dbHelper.GetPaymentFullAsync(PaymentId);
        if (Payment == null || Payment.Status != nameof(MyApp.Constants.MRMR.PaymentStatus.Verified))
            return RedirectToPage("/Dashboard", new { area = "Applicant" });
        return Page();
    }
}
