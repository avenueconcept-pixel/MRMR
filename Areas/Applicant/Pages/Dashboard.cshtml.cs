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
    private readonly SubmissionDbHelper   _submissionDbHelper;

    public DashboardModel(RegistrationDbHelper dbHelper, SubmissionDbHelper submissionDbHelper)
    {
        _dbHelper           = dbHelper;
        _submissionDbHelper = submissionDbHelper;
    }

    public List<ApplicationDashboardDto> Applications { get; set; } = new();
    public string FullName { get; set; } = string.Empty;

    private static readonly HashSet<string> PostSubmitStatuses =
    [
        nameof(ApplicationStatus.SubmissionCompleted),
        nameof(ApplicationStatus.UnderEvaluation),
        nameof(ApplicationStatus.Shortlisted),
        nameof(ApplicationStatus.Approved),
        nameof(ApplicationStatus.Rejected),
        nameof(ApplicationStatus.Withdrawn)
    ];

    public async Task<IActionResult> OnGetAsync()
    {
        var registrantIdStr = User.FindFirst(CookieConstants.SessionKeys.UserId)?.Value;
        if (!int.TryParse(registrantIdStr, out int registrantId))
            return RedirectToPage("/Login", new { area = "Applicant" });

        FullName = User.FindFirst(CookieConstants.SessionKeys.FullName)?.Value ?? string.Empty;

        var apps = await _dbHelper.GetApplicationsAsync(registrantId);

        foreach (var app in apps)
        {
            var payments  = await _dbHelper.GetPaymentsAsync(app.Id);
            var documents = PostSubmitStatuses.Contains(app.Status)
                ? await _submissionDbHelper.GetApplicationDocumentsAsync(app.Id)
                : [];

            Applications.Add(new ApplicationDashboardDto
            {
                Application       = app,
                NominationPayment = payments.FirstOrDefault(p => p.PaymentType == nameof(PaymentType.NominationFee)),
                AwardPayment      = payments.FirstOrDefault(p => p.PaymentType == nameof(PaymentType.AwardFee)),
                Documents         = documents
            });
        }

        return Page();
    }
}
