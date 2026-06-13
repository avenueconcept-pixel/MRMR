using Microsoft.EntityFrameworkCore;
using MyApp.Constants.MRMR;
using MyApp.Data;
using MyApp.Models.MRMR;

namespace MyApp.Helper.DB.MRMR;

public class AdminMrmrDbHelper : DbHelper
{
    public AdminMrmrDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
        : base(db, audit, loggerFactory) { }

    public async Task<MrmrDashboardStats> GetDashboardStatsAsync()
        => await ExecuteAsync(async () =>
        {
            var now   = DateTime.UtcNow;
            var month = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var totalRegistrations = await _db.Applications.CountAsync();
            var thisMonthReg       = await _db.Applications.CountAsync(a => a.CreatedAt >= month);

            var pendingPayments    = await _db.Payments.CountAsync(p =>
                p.Status == nameof(PaymentStatus.PendingVerification));

            var submittedApps      = await _db.Applications.CountAsync(a =>
                a.Status == nameof(ApplicationStatus.SubmissionCompleted));

            var underEvaluation    = await _db.Applications.CountAsync(a =>
                a.Status == nameof(ApplicationStatus.UnderEvaluation));

            var pendingDocs        = await _db.ApplicationDocuments.CountAsync(d =>
                d.VerificationStatus == "Pending" &&
                _db.Applications.Any(a => a.Id == d.ApplicationId && a.IsFinalSubmitted));

            return new MrmrDashboardStats
            {
                TotalRegistrations = totalRegistrations,
                ThisMonthReg       = thisMonthReg,
                PendingPayments    = pendingPayments,
                SubmittedApps      = submittedApps,
                UnderEvaluation    = underEvaluation,
                PendingDocuments   = pendingDocs
            };
        });

    public async Task<List<Application>> GetRecentRegistrationsAsync(int take = 8)
        => await ExecuteAsync(async () =>
            await _db.Applications
                .Include(a => a.Registrant)
                .Include(a => a.AwardCategory)
                .OrderByDescending(a => a.CreatedAt)
                .Take(take)
                .ToListAsync());

    public async Task<List<Payment>> GetRecentPaymentsAsync(int take = 8)
        => await ExecuteAsync(async () =>
            await _db.Payments
                .Include(p => p.Application)
                    .ThenInclude(a => a.Registrant)
                .Where(p => p.Status == nameof(PaymentStatus.PendingVerification))
                .OrderByDescending(p => p.CreatedAt)
                .Take(take)
                .ToListAsync());
}

public class MrmrDashboardStats
{
    public int TotalRegistrations { get; set; }
    public int ThisMonthReg       { get; set; }
    public int PendingPayments    { get; set; }
    public int SubmittedApps      { get; set; }
    public int UnderEvaluation    { get; set; }
    public int PendingDocuments   { get; set; }
}
