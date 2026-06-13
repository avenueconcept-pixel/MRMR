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

    // ── Payment Verification ──

    public async Task<List<Payment>> GetPaymentListAsync(
        string? paymentType, string? status, string? search)
        => await ExecuteAsync(async () =>
        {
            var query = _db.Payments
                .Include(p => p.Application)
                    .ThenInclude(a => a.Registrant)
                .AsQueryable();

            query = query.Where(p => p.Method == nameof(PaymentMethod.ManualBankTransfer));

            if (!string.IsNullOrWhiteSpace(paymentType))
                query = query.Where(p => p.PaymentType == paymentType);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status == status);
            else
                query = query.Where(p => p.Status == nameof(PaymentStatus.PendingVerification));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    (p.InvoiceNo != null && p.InvoiceNo.ToLower().Contains(s)) ||
                    p.Application.Registrant.FullName.ToLower().Contains(s) ||
                    p.Application.ApplicationId.ToLower().Contains(s));
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Take(100)
                .ToListAsync();
        });

    public async Task<Payment?> GetPaymentForAdminAsync(int paymentId)
        => await ExecuteAsync(async () =>
            await _db.Payments
                .Include(p => p.Application)
                    .ThenInclude(a => a.Registrant)
                .Include(p => p.Application)
                    .ThenInclude(a => a.AwardCategory)
                .Include(p => p.AuditLogs.OrderByDescending(l => l.PerformedAt))
                .FirstOrDefaultAsync(p => p.Id == paymentId));

    public async Task ApprovePaymentAsync(int paymentId, int adminId)
        => await ExecuteAsync(async () =>
        {
            var payment = await _db.Payments
                .Include(p => p.Application)
                    .ThenInclude(a => a.AwardCategory)
                .FirstOrDefaultAsync(p => p.Id == paymentId)
                ?? throw new InvalidOperationException("Payment not found.");

            if (payment.Status != nameof(PaymentStatus.PendingVerification))
                throw new InvalidOperationException("Payment is not pending verification.");

            payment.Status     = nameof(PaymentStatus.Verified);
            payment.VerifiedBy = adminId;
            payment.VerifiedAt = DateTime.UtcNow;
            payment.UpdatedAt  = DateTime.UtcNow;

            var app = payment.Application;
            if (app != null)
            {
                app.Status    = payment.PaymentType == nameof(PaymentType.NominationFee)
                    ? nameof(ApplicationStatus.NominationFeeVerified)
                    : nameof(ApplicationStatus.AwardFeeVerified);
                app.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            _db.PaymentAuditLogs.Add(new PaymentAuditLog
            {
                PaymentId   = paymentId,
                Action      = "AdminApproved",
                PerformedBy = adminId,
                PerformedAt = DateTime.UtcNow,
                Remarks     = "Approved by admin"
            });
            await _db.SaveChangesAsync();

            if (payment.PaymentType == nameof(PaymentType.NominationFee))
            {
                var exists = await _db.Payments.AnyAsync(p =>
                    p.ApplicationId == payment.ApplicationId &&
                    p.PaymentType   == nameof(PaymentType.AwardFee));

                if (!exists && app != null)
                {
                    var invoiceNo = $"INV-{DateTime.UtcNow:yyyyMMdd}-{payment.ApplicationId:D5}-AWD";
                    var awardFee  = new Payment
                    {
                        ApplicationId = payment.ApplicationId,
                        PaymentType   = nameof(PaymentType.AwardFee),
                        Method        = nameof(PaymentMethod.ManualBankTransfer),
                        Amount        = app.AwardCategory?.Price ?? 0,
                        Status        = nameof(PaymentStatus.Pending),
                        InvoiceNo     = invoiceNo,
                        CreatedAt     = DateTime.UtcNow,
                        UpdatedAt     = DateTime.UtcNow
                    };
                    _db.Payments.Add(awardFee);
                    await _db.SaveChangesAsync();

                    _db.PaymentAuditLogs.Add(new PaymentAuditLog
                    {
                        PaymentId   = awardFee.Id,
                        Action      = "AwardFeeCreated",
                        PerformedAt = DateTime.UtcNow,
                        Remarks     = "Auto-created on NominationFee admin approval"
                    });
                    await _db.SaveChangesAsync();
                }
            }
        });

    public async Task RejectPaymentAsync(int paymentId, int adminId, string remarks)
        => await ExecuteAsync(async () =>
        {
            var payment = await _db.Payments
                .Include(p => p.Application)
                .FirstOrDefaultAsync(p => p.Id == paymentId)
                ?? throw new InvalidOperationException("Payment not found.");

            if (payment.Status != nameof(PaymentStatus.PendingVerification))
                throw new InvalidOperationException("Payment is not pending verification.");

            payment.Status       = nameof(PaymentStatus.Rejected);
            payment.AdminRemarks = remarks;
            payment.VerifiedBy   = adminId;
            payment.VerifiedAt   = DateTime.UtcNow;
            payment.UpdatedAt    = DateTime.UtcNow;

            var app = payment.Application;
            if (app != null)
            {
                app.Status    = payment.PaymentType == nameof(PaymentType.NominationFee)
                    ? nameof(ApplicationStatus.NominationFeePending)
                    : nameof(ApplicationStatus.AwardFeePending);
                app.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            _db.PaymentAuditLogs.Add(new PaymentAuditLog
            {
                PaymentId   = paymentId,
                Action      = "AdminRejected",
                PerformedBy = adminId,
                PerformedAt = DateTime.UtcNow,
                Remarks     = remarks
            });
            await _db.SaveChangesAsync();
        });

    // ── Application Management ──

    public async Task<List<Application>> GetApplicationListAsync(
        string? statusFilter, string? typeFilter, string? search)
        => await ExecuteAsync(async () =>
        {
            var query = _db.Applications
                .Include(a => a.Registrant)
                .Include(a => a.AwardCategory)
                .Include(a => a.Payments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(a => a.Status == statusFilter);

            if (!string.IsNullOrWhiteSpace(typeFilter))
                query = query.Where(a => a.ApplicationType == typeFilter);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(a =>
                    a.ApplicationId.ToLower().Contains(s) ||
                    a.Registrant.FullName.ToLower().Contains(s) ||
                    a.Registrant.Email.ToLower().Contains(s) ||
                    (a.Registrant.CompanyName != null && a.Registrant.CompanyName.ToLower().Contains(s)));
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(200)
                .ToListAsync();
        });

    public async Task<Application?> GetApplicationDetailAsync(int applicationId)
        => await ExecuteAsync(async () =>
            await _db.Applications
                .Include(a => a.Registrant)
                .Include(a => a.AwardCategory)
                .Include(a => a.Payments.OrderByDescending(p => p.CreatedAt))
                    .ThenInclude(p => p.AuditLogs.OrderByDescending(l => l.PerformedAt))
                .Include(a => a.Documents.OrderBy(d => d.DocumentType))
                .Include(a => a.Submission)
                .FirstOrDefaultAsync(a => a.Id == applicationId));

    public async Task OverrideApplicationStatusAsync(
        int applicationId, string newStatus, int adminId, string reason)
        => await ExecuteAsync(async () =>
        {
            var app = await _db.Applications.FindAsync(applicationId)
                ?? throw new InvalidOperationException("Application not found.");

            var oldStatus = app.Status;
            app.Status    = newStatus;
            app.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var payment = await _db.Payments
                .Where(p => p.ApplicationId == applicationId &&
                            p.PaymentType   == nameof(PaymentType.NominationFee))
                .FirstOrDefaultAsync();

            if (payment != null)
            {
                _db.PaymentAuditLogs.Add(new PaymentAuditLog
                {
                    PaymentId   = payment.Id,
                    Action      = "AdminStatusOverride",
                    PerformedBy = adminId,
                    PerformedAt = DateTime.UtcNow,
                    Remarks     = $"Status changed: {oldStatus} → {newStatus}. Reason: {reason}"
                });
                await _db.SaveChangesAsync();
            }
        });

    // ── Document Verification ──

    public async Task<List<ApplicationDocument>> GetApplicationDocumentsAsync(int applicationId)
        => await ExecuteAsync(async () =>
            await _db.ApplicationDocuments
                .Where(d => d.ApplicationId == applicationId)
                .OrderBy(d => d.DocumentType)
                .ToListAsync());

    public async Task VerifyDocumentAsync(int documentId, int adminId)
        => await ExecuteAsync(async () =>
        {
            var doc = await _db.ApplicationDocuments.FindAsync(documentId)
                ?? throw new InvalidOperationException("Document not found.");

            doc.VerificationStatus = nameof(DocumentVerificationStatus.Verified);
            doc.AdminRemarks       = null;
            doc.VerifiedBy         = adminId;
            doc.VerifiedAt         = DateTime.UtcNow;
            doc.UpdatedAt          = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        });

    public async Task RejectDocumentAsync(int documentId, int adminId, string remarks)
        => await ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(remarks))
                throw new InvalidOperationException("Remarks are required when rejecting a document.");

            var doc = await _db.ApplicationDocuments.FindAsync(documentId)
                ?? throw new InvalidOperationException("Document not found.");

            doc.VerificationStatus = nameof(DocumentVerificationStatus.Rejected);
            doc.AdminRemarks       = remarks;
            doc.VerifiedBy         = adminId;
            doc.VerifiedAt         = DateTime.UtcNow;
            doc.UpdatedAt          = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        });

    // ── Category Management ──

    public async Task<List<AwardCategory>> GetCategoryListAsync()
        => await ExecuteAsync(async () =>
            await _db.AwardCategories
                .Include(c => c.Criteria.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder))
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync());

    public async Task<AwardCategory?> GetCategoryAsync(int id)
        => await ExecuteAsync(async () =>
            await _db.AwardCategories
                .Include(c => c.Criteria.OrderBy(x => x.DisplayOrder))
                .FirstOrDefaultAsync(c => c.Id == id));

    public async Task CreateCategoryAsync(AwardCategory category, List<AwardCriterion> criteria)
        => await ExecuteAsync(async () =>
        {
            ValidateCriteriaWeights(criteria);

            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            _db.AwardCategories.Add(category);
            await _db.SaveChangesAsync();

            short order = 1;
            foreach (var c in criteria.Where(x => !string.IsNullOrWhiteSpace(x.CriterionName)))
            {
                c.AwardCategoryId = category.Id;
                c.DisplayOrder    = order++;
                c.IsActive        = true;
                c.CreatedAt       = DateTime.UtcNow;
                c.UpdatedAt       = DateTime.UtcNow;
                _db.AwardCriteria.Add(c);
            }
            await _db.SaveChangesAsync();
        });

    public async Task UpdateCategoryAsync(AwardCategory updated, List<AwardCriterion> criteria)
        => await ExecuteAsync(async () =>
        {
            var category = await _db.AwardCategories
                .Include(c => c.Criteria)
                .FirstOrDefaultAsync(c => c.Id == updated.Id)
                ?? throw new InvalidOperationException("Category not found.");

            category.Name          = updated.Name;
            category.CategoryType  = updated.CategoryType;
            category.Price         = updated.Price;
            category.MaxRecipients = updated.MaxRecipients;
            category.IsActive      = updated.IsActive;
            category.DisplayOrder  = updated.DisplayOrder;
            category.Description   = updated.Description;
            category.UpdatedAt     = DateTime.UtcNow;

            if (!category.CriteriaLocked)
            {
                ValidateCriteriaWeights(criteria);

                foreach (var existing in category.Criteria)
                {
                    existing.IsActive  = false;
                    existing.UpdatedAt = DateTime.UtcNow;
                }

                short order = 1;
                foreach (var c in criteria.Where(x => !string.IsNullOrWhiteSpace(x.CriterionName)))
                {
                    c.AwardCategoryId = category.Id;
                    c.DisplayOrder    = order++;
                    c.IsActive        = true;
                    c.CreatedAt       = DateTime.UtcNow;
                    c.UpdatedAt       = DateTime.UtcNow;
                    _db.AwardCriteria.Add(c);
                }
            }

            await _db.SaveChangesAsync();
        });

    public async Task DeactivateCategoryAsync(int id)
        => await ExecuteAsync(async () =>
        {
            var category = await _db.AwardCategories.FindAsync(id)
                ?? throw new InvalidOperationException("Category not found.");

            var hasApps = await _db.Applications.AnyAsync(a => a.AwardCategoryId == id);
            if (hasApps)
                throw new InvalidOperationException(
                    "Cannot deactivate a category that has applications linked to it.");

            category.IsActive  = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        });

    private static void ValidateCriteriaWeights(List<AwardCriterion> criteria)
    {
        var active = criteria.Where(x => !string.IsNullOrWhiteSpace(x.CriterionName)).ToList();
        if (!active.Any()) return;

        var total = active.Sum(x => x.Weight);
        if (total != 100)
            throw new InvalidOperationException(
                $"Criteria weights must total exactly 100%. Current total: {total}%.");
    }
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
