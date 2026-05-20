using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class LanguageDbHelper : DbHelper
{
  public LanguageDbHelper(AppDbContext db) : base(db) { }

  public async Task<List<Language>> GetAllActiveAsync()
      => await _db.Languages
          .Where(l => l.Status == UserStatusConstants.Active)
          .OrderBy(l => l.SortOrder)
          .ToListAsync();

  public async Task<Language?> GetByCodeAsync(string languageCode)
      => await _db.Languages
          .FirstOrDefaultAsync(l => l.LanguageCode == languageCode
                                 && l.Status == UserStatusConstants.Active);

  public async Task<Language?> GetByIdAsync(int id)
      => await _db.Languages.FindAsync(id);

  public async Task CreateAsync(Language language)
  {
    language.CreatedAt = DateTime.Now;
    _db.Languages.Add(language);
    await _db.SaveChangesAsync();
  }

  public async Task UpdateAsync(Language language)
  {
    _db.Languages.Update(language);
    await _db.SaveChangesAsync();
  }

  public async Task DeleteAsync(int id)
  {
    var language = await _db.Languages.FindAsync(id);
    if (language != null)
    {
      language.Status = UserStatusConstants.Deleted;
      await _db.SaveChangesAsync();
    }
  }
}
