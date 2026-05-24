using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class LanguageDbHelper : DbHelper
{
  public LanguageDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<Language>> GetAllAsync()
      => await ExecuteAsync(() => _db.Languages
          .Where(l => l.Status != StatusConstants.Deleted)
          .OrderBy(l => l.SortOrder)
          .ToListAsync());

  public async Task<List<Language>> GetAllActiveAsync()
      => await ExecuteAsync(() => _db.Languages
          .Where(l => l.Status == StatusConstants.Active)
          .OrderBy(l => l.SortOrder)
          .ToListAsync());

  public async Task<Language?> GetByCodeAsync(string languageCode)
      => await ExecuteAsync(() => _db.Languages
          .FirstOrDefaultAsync(l => l.LanguageCode == languageCode
                                 && l.Status == StatusConstants.Active));

  public async Task<Language?> GetByIdAsync(int id)
      => await ExecuteAsync(async () => await _db.Languages.FindAsync(id));

  public async Task CreateAsync(Language language, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var entity = new Language
        {
          LanguageCode = language.LanguageCode,
          LanguageName = language.LanguageName,
          NativeName   = language.NativeName,
          SortOrder    = language.SortOrder,
          Status       = language.Status,
          CreatedAt    = DateTime.UtcNow,
          CreatedBy    = createdBy,
          UpdatedAt    = DateTime.UtcNow,
          UpdatedBy    = createdBy
        };
        _db.Languages.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.LogInsertAsync("languages", entity.Id.ToString(), entity, createdBy);
      });

  public async Task UpdateAsync(Language language, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Languages.FindAsync(language.Id);
        if (existing == null) return;

        var old = new Language
        {
          Id           = existing.Id,
          LanguageCode = existing.LanguageCode,
          LanguageName = existing.LanguageName,
          NativeName   = existing.NativeName,
          SortOrder    = existing.SortOrder,
          Status       = existing.Status
        };

        existing.LanguageCode = language.LanguageCode;
        existing.LanguageName = language.LanguageName;
        existing.NativeName   = language.NativeName;
        existing.SortOrder    = language.SortOrder;
        existing.Status       = language.Status;
        existing.UpdatedAt    = DateTime.UtcNow;
        existing.UpdatedBy    = updatedBy;
        await _db.SaveChangesAsync();
        await _audit.LogUpdateAsync("languages", existing.Id.ToString(), old, existing, updatedBy);
      });

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var language = await _db.Languages.FindAsync(id);
        if (language != null)
        {
          var oldStatus  = language.Status;
          language.Status    = status;
          language.UpdatedAt = DateTime.UtcNow;
          language.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
          var action = status == StatusConstants.Deleted
              ? AuditConstants.Actions.Delete
              : AuditConstants.Actions.Update;
          await _audit.LogActionAsync("languages", id.ToString(), action, updatedBy,
              remarks: $"Status changed from {oldStatus} to {status}");
        }
      });
}
