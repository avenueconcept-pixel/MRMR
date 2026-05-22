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
          .Where(l => l.Status == UserStatusConstants.Active)
          .OrderBy(l => l.SortOrder)
          .ToListAsync());

  public async Task<Language?> GetByCodeAsync(string languageCode)
      => await ExecuteAsync(() => _db.Languages
          .FirstOrDefaultAsync(l => l.LanguageCode == languageCode
                                 && l.Status == UserStatusConstants.Active));

  public async Task<Language?> GetByIdAsync(int id)
      => await ExecuteAsync(async () => await _db.Languages.FindAsync(id));

  public async Task CreateAsync(Language language)
      => await ExecuteAsync(async () =>
      {
        language.CreatedAt = DateTime.Now;
        _db.Languages.Add(language);
        await _db.SaveChangesAsync();
      });

  public async Task UpdateAsync(Language language)
      => await ExecuteAsync(async () =>
      {
        _db.Languages.Update(language);
        await _db.SaveChangesAsync();
      });

  public async Task DeleteAsync(int id)
      => await ExecuteAsync(async () =>
      {
        var language = await _db.Languages.FindAsync(id);
        if (language != null)
        {
          language.Status = UserStatusConstants.Deleted;
          await _db.SaveChangesAsync();
        }
      });
}
