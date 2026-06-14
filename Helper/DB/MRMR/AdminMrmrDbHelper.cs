using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Constants.MRMR;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Models.MRMR;
using AdminUser = MyApp.Models.AdminUser;

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

    public async Task ReorderCategoriesAsync(List<(int Id, int Order)> items)
        => await ExecuteAsync(async () =>
        {
            foreach (var (id, order) in items)
            {
                var cat = await _db.AwardCategories.FindAsync(id);
                if (cat != null)
                {
                    cat.DisplayOrder = (short)order;
                    cat.UpdatedAt    = DateTime.UtcNow;
                }
            }
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

    // ── Judge Management ──

    public async Task<List<AdminUser>> GetJudgeListAsync()
        => await ExecuteAsync(async () =>
        {
            var judgeRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleCode == "JUDGE");
            if (judgeRole == null) return [];

            return await _db.AdminUsers
                .Where(u => u.RoleId == judgeRole.Id)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        });

    public async Task<List<JudgeCategoryAssignment>> GetJudgeAssignmentsAsync(int judgeId)
        => await ExecuteAsync(async () =>
            await _db.JudgeCategoryAssignments
                .Include(a => a.AwardCategory)
                .Where(a => a.JudgeId == judgeId && a.IsActive)
                .ToListAsync());

    public async Task<List<AwardCategory>> GetActiveCategoriesAsync()
        => await ExecuteAsync(async () =>
            await _db.AwardCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync());

    public async Task<AdminUser> CreateJudgeAsync(
        string fullName, string email, string username,
        string tempPassword, int createdByAdminId)
        => await ExecuteAsync(async () =>
        {
            if (await _db.AdminUsers.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("An account with this email already exists.");

            if (await _db.AdminUsers.AnyAsync(u => u.Username == username))
                throw new InvalidOperationException("This username is already taken.");

            var judgeRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleCode == "JUDGE")
                ?? throw new InvalidOperationException(
                    "Judge role not found. Please ensure a role with RoleCode='JUDGE' exists in the roles table.");

            var judge = new AdminUser
            {
                FullName              = fullName,
                Email                 = email,
                Username              = username,
                PasswordHash          = PasswordCryptoHelper.Encrypt(tempPassword),
                RoleId                = judgeRole.Id,
                CountryCode           = "MY",
                Status                = StatusConstants.Active,
                IsForceChangePassword = true,
                CreatedAt             = DateTime.UtcNow,
                CreatedBy             = createdByAdminId.ToString(),
                UpdatedAt             = DateTime.UtcNow,
                UpdatedBy             = createdByAdminId.ToString()
            };

            _db.AdminUsers.Add(judge);
            await _db.SaveChangesAsync();
            return judge;
        });

    public async Task AssignJudgeToCategoriesAsync(
        int judgeId, List<int> categoryIds, int assignedByAdminId)
        => await ExecuteAsync(async () =>
        {
            foreach (var catId in categoryIds)
            {
                var exists = await _db.JudgeCategoryAssignments
                    .AnyAsync(a => a.JudgeId == judgeId && a.AwardCategoryId == catId);

                if (!exists)
                {
                    _db.JudgeCategoryAssignments.Add(new JudgeCategoryAssignment
                    {
                        JudgeId         = judgeId,
                        AwardCategoryId = catId,
                        AssignedBy      = assignedByAdminId,
                        AssignedAt      = DateTime.UtcNow,
                        IsActive        = true
                    });
                }
            }
            await _db.SaveChangesAsync();
        });

    public async Task DeactivateJudgeAssignmentAsync(int assignmentId)
        => await ExecuteAsync(async () =>
        {
            var assignment = await _db.JudgeCategoryAssignments.FindAsync(assignmentId)
                ?? throw new InvalidOperationException("Assignment not found.");

            assignment.IsActive = false;
            await _db.SaveChangesAsync();
        });

    public async Task DeactivateJudgeAsync(int judgeId)
        => await ExecuteAsync(async () =>
        {
            var judge = await _db.AdminUsers.FindAsync(judgeId)
                ?? throw new InvalidOperationException("Judge not found.");

            judge.Status    = StatusConstants.Inactive;
            judge.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var assignments = await _db.JudgeCategoryAssignments
                .Where(a => a.JudgeId == judgeId && a.IsActive)
                .ToListAsync();

            foreach (var a in assignments)
                a.IsActive = false;

            await _db.SaveChangesAsync();
        });

    public async Task<int> GetCategoryApplicationCountAsync(int categoryId)
        => await ExecuteAsync(async () =>
            await _db.Applications.CountAsync(a =>
                a.AwardCategoryId == categoryId &&
                a.IsFinalSubmitted));

    // ── Judge Evaluation ──

    public async Task<List<Application>> GetCategoryApplicationsForJudgeAsync(int categoryId)
        => await ExecuteAsync(async () =>
            await _db.Applications
                .Include(a => a.Registrant)
                .Include(a => a.AwardCategory)
                .Where(a => a.AwardCategoryId == categoryId && a.IsFinalSubmitted)
                .OrderBy(a => a.ApplicationId)
                .ToListAsync());

    public async Task<JudgeEvaluation?> GetJudgeEvaluationAsync(int applicationId, int judgeId)
        => await ExecuteAsync(async () =>
            await _db.JudgeEvaluations
                .Include(e => e.Scores)
                    .ThenInclude(s => s.AwardCriterion)
                .FirstOrDefaultAsync(e =>
                    e.ApplicationId == applicationId && e.JudgeId == judgeId));

    public async Task<bool> IsJudgeAssignedToCategoryAsync(int judgeId, int categoryId)
        => await ExecuteAsync(async () =>
            await _db.JudgeCategoryAssignments.AnyAsync(a =>
                a.JudgeId == judgeId &&
                a.AwardCategoryId == categoryId &&
                a.IsActive));

    public async Task<Application?> GetApplicationForJudgeAsync(int applicationId)
        => await ExecuteAsync(async () =>
            await _db.Applications
                .Include(a => a.Registrant)
                .Include(a => a.AwardCategory)
                    .ThenInclude(c => c.Criteria.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder))
                .Include(a => a.Submission)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.IsFinalSubmitted));

    public async Task SaveEvaluationDraftAsync(
        int applicationId, int judgeId,
        List<(int CriterionId, decimal Score, string? Comment)> scores,
        string? overallComment, string? recommendation)
        => await ExecuteAsync(async () =>
        {
            var eval = await _db.JudgeEvaluations
                .Include(e => e.Scores)
                .FirstOrDefaultAsync(e => e.ApplicationId == applicationId && e.JudgeId == judgeId);

            if (eval == null)
            {
                eval = new JudgeEvaluation
                {
                    ApplicationId  = applicationId,
                    JudgeId        = judgeId,
                    Status         = nameof(EvaluationStatus.Draft),
                    OverallComment = overallComment,
                    Recommendation = recommendation,
                    CreatedAt      = DateTime.UtcNow,
                    UpdatedAt      = DateTime.UtcNow
                };
                _db.JudgeEvaluations.Add(eval);
                await _db.SaveChangesAsync();
            }
            else
            {
                if (eval.Status == nameof(EvaluationStatus.Submitted))
                    throw new InvalidOperationException("Evaluation already submitted and cannot be modified.");

                eval.OverallComment = overallComment;
                eval.Recommendation = recommendation;
                eval.Status         = nameof(EvaluationStatus.Draft);
                eval.UpdatedAt      = DateTime.UtcNow;
            }

            foreach (var (criterionId, score, comment) in scores)
            {
                var existing  = eval.Scores.FirstOrDefault(s => s.AwardCriterionId == criterionId);
                var criterion = await _db.AwardCriteria.FindAsync(criterionId);
                var weighted  = criterion != null ? Math.Round(score * (criterion.Weight / 100m), 4) : (decimal?)null;

                if (existing != null)
                {
                    existing.Score         = score;
                    existing.WeightedScore = weighted;
                    existing.Comment       = comment;
                    existing.UpdatedAt     = DateTime.UtcNow;
                }
                else
                {
                    _db.JudgeScores.Add(new JudgeScore
                    {
                        ApplicationId    = applicationId,
                        JudgeId          = judgeId,
                        AwardCriterionId = criterionId,
                        Score            = score,
                        WeightedScore    = weighted,
                        Comment          = comment,
                        CreatedAt        = DateTime.UtcNow,
                        UpdatedAt        = DateTime.UtcNow
                    });
                }
            }
            await _db.SaveChangesAsync();
        });

    public async Task SubmitEvaluationAsync(
        int applicationId, int judgeId,
        List<(int CriterionId, decimal Score, string? Comment)> scores,
        string? overallComment, string? recommendation)
        => await ExecuteAsync(async () =>
        {
            await SaveEvaluationDraftAsync(applicationId, judgeId, scores, overallComment, recommendation);

            var eval = await _db.JudgeEvaluations
                .FirstOrDefaultAsync(e => e.ApplicationId == applicationId && e.JudgeId == judgeId)
                ?? throw new InvalidOperationException("Evaluation not found after save.");

            eval.Status      = nameof(EvaluationStatus.Submitted);
            eval.SubmittedAt = DateTime.UtcNow;
            eval.UpdatedAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        });

    // ── Reports ──

    public async Task<List<Application>> GetAllApplicationsForReportAsync()
        => await ExecuteAsync(async () =>
            await _db.Applications
                .Include(a => a.Registrant)
                .Include(a => a.AwardCategory)
                .Include(a => a.Payments)
                .Include(a => a.Ranking)
                .OrderBy(a => a.ApplicationId)
                .ToListAsync());

    public async Task<List<Payment>> GetAllPaymentsForReportAsync()
        => await ExecuteAsync(async () =>
            await _db.Payments
                .Include(p => p.Application)
                    .ThenInclude(a => a.Registrant)
                .Include(p => p.Application)
                    .ThenInclude(a => a.AwardCategory)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync());

    public async Task<List<ApplicationRanking>> GetRankingsForReportAsync(int? categoryId)
        => await ExecuteAsync(async () =>
        {
            var query = _db.ApplicationRankings
                .Include(r => r.Application)
                    .ThenInclude(a => a.Registrant)
                .Include(r => r.AwardCategory)
                .AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
                query = query.Where(r => r.AwardCategoryId == categoryId);

            return await query
                .OrderBy(r => r.AwardCategoryId)
                .ThenBy(r => r.RankPosition)
                .ToListAsync();
        });

    // ── Evaluation Overview ──

    public async Task<List<ApplicationScoreSummaryDto>> GetEvaluationSummaryAsync(int categoryId)
        => await ExecuteAsync(async () =>
        {
            var apps = await _db.Applications
                .Include(a => a.Registrant)
                .Include(a => a.AwardCategory)
                .Include(a => a.Ranking)
                .Where(a => a.AwardCategoryId == categoryId && a.IsFinalSubmitted)
                .ToListAsync();

            var result = new List<ApplicationScoreSummaryDto>();

            foreach (var app in apps)
            {
                var evals = await _db.JudgeEvaluations
                    .Include(e => e.Scores)
                        .ThenInclude(s => s.AwardCriterion)
                    .Where(e => e.ApplicationId == app.Id &&
                                e.Status == nameof(EvaluationStatus.Submitted))
                    .ToListAsync();

                var judgeRows = new List<JudgeScoreRowDto>();

                foreach (var ev in evals)
                {
                    var judge = await _db.AdminUsers.FindAsync(ev.JudgeId);
                    var total = ev.Scores.Sum(s => s.WeightedScore ?? 0);
                    judgeRows.Add(new JudgeScoreRowDto
                    {
                        JudgeId        = ev.JudgeId,
                        JudgeName      = judge?.FullName ?? $"Judge #{ev.JudgeId}",
                        TotalWeighted  = Math.Round(total, 2),
                        Recommendation = ev.Recommendation
                    });
                }

                var avgScore = judgeRows.Any()
                    ? Math.Round(judgeRows.Average(r => r.TotalWeighted), 2)
                    : 0;

                result.Add(new ApplicationScoreSummaryDto
                {
                    Application   = app,
                    TotalWeighted = avgScore,
                    JudgeCount    = judgeRows.Count,
                    JudgeRows     = judgeRows,
                    Ranking       = app.Ranking
                });
            }

            return result.OrderByDescending(r => r.TotalWeighted).ToList();
        });

    public async Task FinalizeRankingsAsync(
        int categoryId,
        List<(int ApplicationId, bool IsApprovedWinner, string? CommitteeRemarks)> decisions,
        int adminId)
        => await ExecuteAsync(async () =>
        {
            var existing = await _db.ApplicationRankings
                .Where(r => r.AwardCategoryId == categoryId)
                .ToListAsync();
            _db.ApplicationRankings.RemoveRange(existing);
            await _db.SaveChangesAsync();

            short rank = 1;
            foreach (var (appId, isWinner, remarks) in decisions)
            {
                _db.ApplicationRankings.Add(new ApplicationRanking
                {
                    ApplicationId    = appId,
                    AwardCategoryId  = categoryId,
                    FinalScore       = 0,
                    RankPosition     = rank++,
                    IsRecommended    = true,
                    IsApprovedWinner = isWinner,
                    CommitteeRemarks = remarks,
                    ApprovedBy       = isWinner ? adminId : null,
                    ApprovedAt       = isWinner ? DateTime.UtcNow : null,
                    RankedAt         = DateTime.UtcNow,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                });

                var app = await _db.Applications.FindAsync(appId);
                if (app != null)
                {
                    app.Status    = isWinner
                        ? nameof(ApplicationStatus.Approved)
                        : nameof(ApplicationStatus.UnderEvaluation);
                    app.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();

            var summaries = await GetEvaluationSummaryAsync(categoryId);
            foreach (var s in summaries)
            {
                var ranking = await _db.ApplicationRankings
                    .FirstOrDefaultAsync(r => r.ApplicationId == s.Application.Id);
                if (ranking != null)
                {
                    ranking.FinalScore = s.TotalWeighted;
                    ranking.UpdatedAt  = DateTime.UtcNow;
                }
            }
            await _db.SaveChangesAsync();
        });
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
