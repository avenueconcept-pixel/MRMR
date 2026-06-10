using Microsoft.EntityFrameworkCore;
using MyApp.Constants.MRMR;
using MyApp.Data;
using MyApp.Models.MRMR;

namespace MyApp.Helper.DB.MRMR;

public class SubmissionDbHelper : DbHelper
{
    public SubmissionDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
        : base(db, audit, loggerFactory) { }

    public async Task<ApplicationSubmission> GetOrCreateSubmissionAsync(int applicationDbId)
        => await ExecuteAsync(async () =>
        {
            var existing = await _db.ApplicationSubmissions
                .FirstOrDefaultAsync(s => s.ApplicationId == applicationDbId);
            if (existing != null) return existing;

            var submission = new ApplicationSubmission
            {
                ApplicationId = applicationDbId,
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow
            };
            _db.ApplicationSubmissions.Add(submission);
            await _db.SaveChangesAsync();
            return submission;
        });

    public async Task<ApplicationSubmission?> GetSubmissionAsync(int applicationDbId)
        => await ExecuteAsync(async () =>
            await _db.ApplicationSubmissions
                .Include(s => s.Application)
                    .ThenInclude(a => a.Registrant)
                .Include(s => s.Application)
                    .ThenInclude(a => a.AwardCategory)
                .FirstOrDefaultAsync(s => s.ApplicationId == applicationDbId));

    public async Task<Application?> GetApplicationByStringIdAsync(string applicationId)
        => await ExecuteAsync(async () =>
            await _db.Applications
                .Include(a => a.Registrant)
                .Include(a => a.AwardCategory)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId));

    // ── Section A ──

    public async Task<SubmissionSectionA?> GetSectionAAsync(int applicationDbId)
        => await ExecuteAsync(async () =>
            await _db.SubmissionSectionsA
                .FirstOrDefaultAsync(s => s.ApplicationId == applicationDbId));

    public async Task SaveSectionAAsync(SubmissionSectionA data, bool markComplete)
        => await ExecuteAsync(async () =>
        {
            var existing = await _db.SubmissionSectionsA
                .FirstOrDefaultAsync(s => s.ApplicationId == data.ApplicationId);

            if (existing == null)
            {
                data.CreatedAt = DateTime.UtcNow;
                data.UpdatedAt = DateTime.UtcNow;
                _db.SubmissionSectionsA.Add(data);
            }
            else
            {
                existing.Title        = data.Title;
                existing.FullName     = data.FullName;
                existing.NricPassport = data.NricPassport;
                existing.ContactNo    = data.ContactNo;
                existing.Email        = data.Email;
                existing.AddressLine1 = data.AddressLine1;
                existing.AddressLine2 = data.AddressLine2;
                existing.City         = data.City;
                existing.State        = data.State;
                existing.Postcode     = data.Postcode;
                existing.Country      = data.Country;
                existing.MembershipNo = data.MembershipNo;
                existing.UpdatedAt    = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();

            var submission = await _db.ApplicationSubmissions
                .FirstOrDefaultAsync(s => s.ApplicationId == data.ApplicationId);
            if (submission != null)
            {
                submission.SectionAComplete = markComplete;
                submission.LastSavedAt      = DateTime.UtcNow;
                submission.UpdatedAt        = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            if (markComplete)
            {
                var app = await _db.Applications.FindAsync(data.ApplicationId);
                if (app?.Status == nameof(ApplicationStatus.AwardFeeVerified))
                {
                    app.Status    = nameof(ApplicationStatus.SubmissionInProgress);
                    app.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }
        });

    // ── Section B ──

    public async Task<SubmissionSectionB?> GetSectionBAsync(int applicationDbId)
        => await ExecuteAsync(async () =>
            await _db.SubmissionSectionsB
                .FirstOrDefaultAsync(s => s.ApplicationId == applicationDbId));

    public async Task SaveSectionBAsync(SubmissionSectionB data, bool markComplete)
        => await ExecuteAsync(async () =>
        {
            var existing = await _db.SubmissionSectionsB
                .FirstOrDefaultAsync(s => s.ApplicationId == data.ApplicationId);

            if (existing == null)
            {
                data.CreatedAt = DateTime.UtcNow;
                data.UpdatedAt = DateTime.UtcNow;
                _db.SubmissionSectionsB.Add(data);
            }
            else
            {
                existing.CompanyName       = data.CompanyName;
                existing.SsmRegNo          = data.SsmRegNo;
                existing.IncorporationDate = data.IncorporationDate;
                existing.ContactNo         = data.ContactNo;
                existing.AddressLine1      = data.AddressLine1;
                existing.AddressLine2      = data.AddressLine2;
                existing.City              = data.City;
                existing.State             = data.State;
                existing.Postcode          = data.Postcode;
                existing.Country           = data.Country;
                existing.Website           = data.Website;
                existing.Industry          = data.Industry;
                existing.BusinessNature    = data.BusinessNature;
                existing.UpdatedAt         = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();

            var submission = await _db.ApplicationSubmissions
                .FirstOrDefaultAsync(s => s.ApplicationId == data.ApplicationId);
            if (submission != null)
            {
                submission.SectionBComplete = markComplete;
                submission.LastSavedAt      = DateTime.UtcNow;
                submission.UpdatedAt        = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        });

    public async Task<SubmissionSectionJsonb?> GetJsonbSectionAsync(int applicationDbId, string sectionCode)
        => await ExecuteAsync(async () =>
            await _db.SubmissionSectionsJsonb
                .FirstOrDefaultAsync(s =>
                    s.ApplicationId == applicationDbId
                    && s.SectionCode == sectionCode));

    public async Task SaveJsonbSectionAsync(int applicationDbId, string sectionCode,
        string jsonData, bool markComplete)
        => await ExecuteAsync(async () =>
        {
            var existing = await _db.SubmissionSectionsJsonb
                .FirstOrDefaultAsync(s =>
                    s.ApplicationId == applicationDbId
                    && s.SectionCode == sectionCode);

            if (existing == null)
            {
                _db.SubmissionSectionsJsonb.Add(new SubmissionSectionJsonb
                {
                    ApplicationId = applicationDbId,
                    SectionCode   = sectionCode,
                    SectionData   = jsonData,
                    IsComplete    = markComplete,
                    CreatedAt     = DateTime.UtcNow,
                    UpdatedAt     = DateTime.UtcNow
                });
            }
            else
            {
                existing.SectionData = jsonData;
                existing.IsComplete  = markComplete;
                existing.UpdatedAt   = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();

            var submission = await _db.ApplicationSubmissions
                .FirstOrDefaultAsync(s => s.ApplicationId == applicationDbId);
            if (submission == null) return;

            switch (sectionCode)
            {
                case "C": submission.SectionCComplete = markComplete; break;
                case "D": submission.SectionDComplete = markComplete; break;
                case "E": submission.SectionEComplete = markComplete; break;
                case "F": submission.SectionFComplete = markComplete; break;
                case "G": submission.SectionGComplete = markComplete; break;
                case "H": submission.SectionHComplete = markComplete; break;
                case "I": submission.SectionIComplete = markComplete; break;
                case "J": submission.SectionJComplete = markComplete; break;
                case "K": submission.SectionKComplete = markComplete; break;
                case "L": submission.SectionLComplete = markComplete; break;
            }
            submission.LastSavedAt = DateTime.UtcNow;
            submission.UpdatedAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        });

    public async Task<AwardCategory?> GetCategoryWithCriteriaAsync(int categoryId)
        => await ExecuteAsync(async () =>
            await _db.AwardCategories
                .Include(c => c.Criteria.Where(cr => cr.IsActive).OrderBy(cr => cr.DisplayOrder))
                .FirstOrDefaultAsync(c => c.Id == categoryId));
}
