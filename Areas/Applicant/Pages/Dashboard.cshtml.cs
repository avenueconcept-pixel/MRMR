using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Constants;
using MyApp.Constants.MRMR;
using MyApp.Dtos;

namespace MyApp.Areas.Applicant.Pages;

public class DashboardModel : ApplicantPageModel
{
    private readonly RegistrationDbHelper _dbHelper;

    public DashboardModel(RegistrationDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public List<ApplicationDashboardDto> Applications { get; set; } = new();
    public string FullName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var registrantIdStr = User.FindFirst(CookieConstants.SessionKeys.UserId)?.Value;
        if (!int.TryParse(registrantIdStr, out int registrantId))
            return RedirectToPage("/Login", new { area = "Applicant" });

        FullName = User.FindFirst(CookieConstants.SessionKeys.FullName)?.Value ?? string.Empty;

        var apps = await _dbHelper.GetApplicationsAsync(registrantId);

        foreach (var app in apps)
        {
            var payments = await _dbHelper.GetPaymentsAsync(app.Id);
            Applications.Add(new ApplicationDashboardDto
            {
                Application       = app,
                NominationPayment = payments.FirstOrDefault(p => p.PaymentType == nameof(PaymentType.NominationFee)),
                AwardPayment      = payments.FirstOrDefault(p => p.PaymentType == nameof(PaymentType.AwardFee)),
            });
        }

        return Page();
    }
}
