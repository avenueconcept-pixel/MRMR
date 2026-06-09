using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Constants.MRMR;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models.MRMR;
using MyApp.Services.MRMR;

namespace MyApp.Helper.DB.MRMR;

public class RegistrationDbHelper : DbHelper
{
    private readonly ApplicationIdGeneratorService _idGen;
    private readonly InvoiceService _invoiceService;

    public RegistrationDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory,
        ApplicationIdGeneratorService idGen, InvoiceService invoiceService) : base(db, audit, loggerFactory)
    {
        _idGen          = idGen;
        _invoiceService = invoiceService;
    }

    public async Task<List<CategorySummaryDto>> GetActiveCategoriesAsync(string? categoryType = null)
        => await ExecuteAsync(async () =>
        {
            var query = _db.AwardCategories.Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(categoryType))
                query = query.Where(c => c.CategoryType == categoryType);

            return await query
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategorySummaryDto
                {
                    Id           = c.Id,
                    Name         = c.Name,
                    CategoryType = c.CategoryType,
                    Price        = c.Price
                })
                .ToListAsync();
        });

    public async Task<CategorySummaryDto?> GetCategoryByIdAsync(int id)
        => await ExecuteAsync(async () =>
            await _db.AwardCategories
                .Where(c => c.Id == id && c.IsActive)
                .Select(c => new CategorySummaryDto
                {
                    Id           = c.Id,
                    Name         = c.Name,
                    CategoryType = c.CategoryType,
                    Price        = c.Price
                })
                .FirstOrDefaultAsync());

    public async Task<bool> EmailExistsAsync(string email)
        => await ExecuteAsync(async () =>
            await _db.Registrants.AnyAsync(r => r.Email.ToLower() == email.ToLower()));

    public async Task<bool> NricExistsAsync(string nric)
        => await ExecuteAsync(async () =>
            await _db.Registrants.AnyAsync(r => r.NricPassport.ToLower() == nric.ToLower()));

    public async Task<bool> ApplicationTypeExistsAsync(int registrantId, string applicationType)
        => await ExecuteAsync(async () =>
            await _db.Applications.AnyAsync(a =>
                a.RegistrantId == registrantId &&
                a.ApplicationType == applicationType));

    public async Task<(RegistrationResult Result, int? ApplicationDbId)> RegisterAsync(
        Registrant registrant, string applicationType, int awardCategoryId, string paymentMethod)
        => await ExecuteAsync(async () =>
        {
            if (await _db.Registrants.AnyAsync(r => r.Email.ToLower() == registrant.Email.ToLower()))
                return (RegistrationResult.DuplicateEmail, (int?)null);

            if (await _db.Registrants.AnyAsync(r => r.NricPassport.ToLower() == registrant.NricPassport.ToLower()))
                return (RegistrationResult.DuplicateNric, (int?)null);

            var category = await _db.AwardCategories.FindAsync(awardCategoryId);
            if (category == null || !category.IsActive)
                return (RegistrationResult.CategoryNotFound, (int?)null);

            registrant.CreatedAt = DateTime.UtcNow;
            registrant.UpdatedAt = DateTime.UtcNow;
            _db.Registrants.Add(registrant);
            await _db.SaveChangesAsync();

            var applicationId = await _idGen.GenerateAsync(applicationType);

            var application = new Application
            {
                ApplicationId   = applicationId,
                RegistrantId    = registrant.Id,
                ApplicationType = applicationType,
                AwardCategoryId = awardCategoryId,
                Status          = nameof(ApplicationStatus.Registered),
                PaymentMethod   = paymentMethod,
                CreatedAt       = DateTime.UtcNow,
                UpdatedAt       = DateTime.UtcNow
            };
            _db.Applications.Add(application);
            await _db.SaveChangesAsync();

            var payment = new Payment
            {
                ApplicationId = application.Id,
                PaymentType   = nameof(PaymentType.NominationFee),
                Amount        = PaymentConstants.NominationFeeAmount,
                Method        = paymentMethod,
                Status        = nameof(PaymentStatus.Pending),
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            return (RegistrationResult.Success, (int?)application.Id);
        });

    public async Task<Payment?> GetPaymentAsync(int applicationDbId, string paymentType)
        => await ExecuteAsync(async () =>
            await _db.Payments
                .FirstOrDefaultAsync(p => p.ApplicationId == applicationDbId && p.PaymentType == paymentType));

    public async Task<Application?> GetApplicationByDbIdAsync(int id)
        => await ExecuteAsync(async () =>
            await _db.Applications.Include(a => a.Registrant).FirstOrDefaultAsync(a => a.Id == id));

    public async Task<Application?> GetApplicationByApplicationIdAsync(string applicationId)
        => await ExecuteAsync(async () =>
            await _db.Applications.Include(a => a.Registrant)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId));

    public async Task UpdatePaymentAxaipayAsync(int paymentId, string refNo, string payload)
        => await ExecuteAsync(async () =>
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment == null) return;
            payment.AxaipayRefNo   = refNo;
            payment.AxaipayPayload = payload;
            payment.Status         = nameof(PaymentStatus.Verified);
            payment.VerifiedAt     = DateTime.UtcNow;
            payment.UpdatedAt      = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _db.PaymentAuditLogs.Add(new PaymentAuditLog
            {
                PaymentId   = paymentId,
                Action      = "AxaipayCallback",
                PerformedAt = DateTime.UtcNow,
                Remarks     = $"Axaipay ref: {refNo}",
                Snapshot    = payload
            });
            await _db.SaveChangesAsync();

            var app = await _db.Applications.FindAsync(payment.ApplicationId);
            if (app != null)
            {
                app.Status    = nameof(ApplicationStatus.NominationFeeVerified);
                app.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        });

    public async Task CreateAwardFeePaymentAsync(int applicationDbId)
        => await ExecuteAsync(async () =>
        {
            var exists = await _db.Payments.AnyAsync(p =>
                p.ApplicationId == applicationDbId
                && p.PaymentType == nameof(PaymentType.AwardFee));
            if (exists) return;

            var app = await _db.Applications
                .Include(a => a.AwardCategory)
                .FirstOrDefaultAsync(a => a.Id == applicationDbId);
            if (app == null) return;

            var amount    = app.AwardCategory?.Price ?? 0;
            var invoiceNo = await _invoiceService.GenerateInvoiceNoAsync();

            var payment = new Payment
            {
                ApplicationId = applicationDbId,
                PaymentType   = nameof(PaymentType.AwardFee),
                Method        = nameof(PaymentMethod.ManualBankTransfer),
                Amount        = amount,
                Status        = nameof(PaymentStatus.Pending),
                InvoiceNo     = invoiceNo,
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            _db.PaymentAuditLogs.Add(new PaymentAuditLog
            {
                PaymentId   = payment.Id,
                Action      = "AwardFeeCreated",
                PerformedAt = DateTime.UtcNow,
                Remarks     = "Auto-created on NominationFeeVerified"
            });
            await _db.SaveChangesAsync();
        });

    public async Task<Payment?> GetPaymentFullAsync(int paymentId)
        => await ExecuteAsync(async () =>
            await _db.Payments
                .Include(p => p.Application)
                    .ThenInclude(a => a.Registrant)
                .Include(p => p.Application)
                    .ThenInclude(a => a.AwardCategory)
                .FirstOrDefaultAsync(p => p.Id == paymentId));

    public async Task ReUploadSlipAsync(int paymentId, string filePath)
        => await ExecuteAsync(async () =>
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment == null) return;
            payment.SlipFilePath   = filePath;
            payment.SlipUploadedAt = DateTime.UtcNow;
            payment.Status         = nameof(PaymentStatus.PendingVerification);
            payment.UpdatedAt      = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var app = await _db.Applications.FindAsync(payment.ApplicationId);
            if (app != null)
            {
                app.Status = payment.PaymentType == nameof(PaymentType.NominationFee)
                    ? nameof(ApplicationStatus.NominationFeePending)
                    : nameof(ApplicationStatus.AwardFeePending);
                app.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            _db.PaymentAuditLogs.Add(new PaymentAuditLog
            {
                PaymentId   = paymentId,
                Action      = "SlipReUploaded",
                PerformedAt = DateTime.UtcNow,
                Remarks     = "Applicant re-uploaded slip after rejection"
            });
            await _db.SaveChangesAsync();
        });

    public async Task<List<Application>> GetApplicationsAsync(int registrantId)
        => await ExecuteAsync(async () =>
            await _db.Applications
                .Include(a => a.AwardCategory)
                .Where(a => a.RegistrantId == registrantId && a.Status != nameof(ApplicationStatus.Withdrawn))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync()) ?? new();

    public async Task<List<Payment>> GetPaymentsAsync(int applicationDbId)
        => await ExecuteAsync(async () =>
            await _db.Payments
                .Where(p => p.ApplicationId == applicationDbId)
                .ToListAsync()) ?? new();

    public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        => await ExecuteAsync(async () =>
            await _db.Payments.FindAsync(paymentId));

    public async Task<Registrant?> GetRegistrantByDbIdAsync(int id)
        => await ExecuteAsync(async () =>
            await _db.Registrants.FindAsync(id));

    public async Task ClearFirstLoginAsync(int registrantId)
        => await ExecuteAsync(async () =>
        {
            var r = await _db.Registrants.FindAsync(registrantId);
            if (r == null) return;
            r.IsFirstLogin = false;
            r.UpdatedAt    = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        });

    public async Task UpdatePasswordAsync(int registrantId, string newPasswordHash)
        => await ExecuteAsync(async () =>
        {
            var r = await _db.Registrants.FindAsync(registrantId);
            if (r == null) return;
            r.PasswordHash = newPasswordHash;
            r.UpdatedAt    = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        });

    public async Task<Registrant?> GetRegistrantByEmailAsync(string email)
        => await ExecuteAsync(async () =>
            await _db.Registrants
                .FirstOrDefaultAsync(r => r.Email == email && r.Status == StatusConstants.Active));

    public async Task SetResetTokenAsync(int registrantId, string token, DateTime expiresAt)
        => await ExecuteAsync(async () =>
        {
            var r = await _db.Registrants.FindAsync(registrantId);
            if (r == null) return;
            r.ResetToken          = token;
            r.ResetTokenExpiresAt = expiresAt;
            r.UpdatedAt           = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        });

    public async Task<Registrant?> GetRegistrantByResetTokenAsync(string token)
        => await ExecuteAsync(async () =>
            await _db.Registrants
                .FirstOrDefaultAsync(r =>
                    r.ResetToken == token
                    && r.ResetTokenExpiresAt > DateTime.UtcNow
                    && r.Status == StatusConstants.Active));

    public async Task ResetPasswordAsync(int registrantId, string newPasswordHash)
        => await ExecuteAsync(async () =>
        {
            var r = await _db.Registrants.FindAsync(registrantId);
            if (r == null) return;
            r.PasswordHash        = newPasswordHash;
            r.ResetToken          = null;
            r.ResetTokenExpiresAt = null;
            r.UpdatedAt           = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        });

    public async Task<Registrant?> GetRegistrantByUsernameAsync(string usernameOrEmail)
        => await ExecuteAsync(async () =>
            await _db.Registrants
                .FirstOrDefaultAsync(r =>
                    (r.Username == usernameOrEmail || r.Email == usernameOrEmail)
                    && r.Status == StatusConstants.Active));

    public async Task<Application?> GetActiveApplicationAsync(int registrantId)
        => await ExecuteAsync(async () =>
            await _db.Applications
                .Include(a => a.AwardCategory)
                .FirstOrDefaultAsync(a =>
                    a.RegistrantId == registrantId
                    && a.Status != nameof(ApplicationStatus.Withdrawn)));

    public async Task SaveSlipUploadAsync(int paymentId, string filePath)
        => await ExecuteAsync(async () =>
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment == null) return;
            payment.SlipFilePath   = filePath;
            payment.SlipUploadedAt = DateTime.UtcNow;
            payment.Status         = nameof(PaymentStatus.PendingVerification);
            payment.UpdatedAt      = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var app = await _db.Applications.FindAsync(payment.ApplicationId);
            if (app != null)
            {
                app.Status    = nameof(ApplicationStatus.NominationFeePending);
                app.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            _db.PaymentAuditLogs.Add(new PaymentAuditLog
            {
                PaymentId   = paymentId,
                Action      = "SlipUploaded",
                PerformedAt = DateTime.UtcNow,
                Remarks     = "Manual slip uploaded at registration"
            });
            await _db.SaveChangesAsync();
        });
}
