using Microsoft.EntityFrameworkCore;
using MyApp.Constants.MRMR;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models.MRMR;
using MyApp.Services.MRMR;

namespace MyApp.Helper.DB.MRMR;

public class RegistrationDbHelper : DbHelper
{
    private readonly ApplicationIdGeneratorService _idGen;

    public RegistrationDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory,
        ApplicationIdGeneratorService idGen) : base(db, audit, loggerFactory)
    {
        _idGen = idGen;
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
}
