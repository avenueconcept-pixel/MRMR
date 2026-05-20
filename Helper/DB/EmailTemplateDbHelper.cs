using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class EmailTemplateDbHelper : DbHelper
{
  public EmailTemplateDbHelper(AppDbContext db) : base(db) { }

  public async Task<EmailTemplate?> GetByKeyAsync(string templateKey, string languageCode)
      => await _db.EmailTemplates
          .FirstOrDefaultAsync(e => e.TemplateKey == templateKey
                                 && e.LanguageCode == languageCode
                                 && e.Status == UserStatusConstants.Active);

  public async Task<List<EmailTemplate>> GetAllAsync()
      => await _db.EmailTemplates
          .Where(e => e.Status != UserStatusConstants.Deleted)
          .OrderBy(e => e.TemplateKey)
          .ToListAsync();

  public async Task CreateAsync(EmailTemplate template)
  {
    template.CreatedAt = DateTime.Now;
    _db.EmailTemplates.Add(template);
    await _db.SaveChangesAsync();
  }

  public async Task UpdateAsync(EmailTemplate template)
  {
    _db.EmailTemplates.Update(template);
    await _db.SaveChangesAsync();
  }

  public async Task DeleteAsync(int id)
  {
    var template = await _db.EmailTemplates.FindAsync(id);
    if (template != null)
    {
      template.Status = UserStatusConstants.Deleted;
      await _db.SaveChangesAsync();
    }
  }
}
