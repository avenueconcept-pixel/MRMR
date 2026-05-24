using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;
using Microsoft.Extensions.Logging;

namespace MyApp.Helper.DB;

public class LanguageDbHelper : DbHelper
{
  public LanguageDbHelper(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

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
      });

  public async Task UpdateAsync(Language language, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Languages.FindAsync(language.Id);
        if (existing == null) return;

        existing.LanguageCode = language.LanguageCode;
        existing.LanguageName = language.LanguageName;
        existing.NativeName   = language.NativeName;
        existing.SortOrder    = language.SortOrder;
        existing.Status       = language.Status;
        existing.UpdatedAt    = DateTime.UtcNow;
        existing.UpdatedBy    = updatedBy;
        await _db.SaveChangesAsync();
      });

  public async Task DeleteAsync(int id)
      => await ExecuteAsync(async () =>
      {
        var language = await _db.Languages.FindAsync(id);
        if (language != null)
        {
          language.Status = StatusConstants.Deleted;
          await _db.SaveChangesAsync();
        }
      });
}
