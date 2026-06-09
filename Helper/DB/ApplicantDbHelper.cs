using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Models;
using Microsoft.Extensions.Logging;

namespace MyApp.Helper.DB;

public class ApplicantDbHelper : DbHelper
{
  public ApplicantDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<Applicant?> GetByEmailAsync(string email)
      => await ExecuteAsync(() => _db.Applicants
          .FirstOrDefaultAsync(c => c.Email == email && c.IsActive));

  public async Task<Applicant?> GetByIdAsync(int id)
      => await ExecuteAsync(async () => await _db.Applicants.FindAsync(id));

  public async Task<bool> EmailExistsAsync(string email)
      => await ExecuteAsync(() => _db.Applicants.AnyAsync(c => c.Email == email));

  public async Task UpdateLastLoginAsync(int applicantId)
      => await ExecuteAsync(async () =>
      {
        var applicant = await _db.Applicants.FindAsync(applicantId);
        if (applicant != null)
        {
          applicant.LastLogin = DateTime.UtcNow;
          await _db.SaveChangesAsync();
        }
      });

  public async Task UpdateLanguageAsync(int applicantId, string languageCode)
      => await ExecuteAsync(async () =>
      {
        var applicant = await _db.Applicants.FindAsync(applicantId);
        if (applicant != null)
        {
          applicant.LanguageCode = languageCode;
          await _db.SaveChangesAsync();
        }
      });
}
