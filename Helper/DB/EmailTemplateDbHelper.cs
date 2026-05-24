using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Models;
using Microsoft.Extensions.Logging;

namespace MyApp.Helper.DB;

public class EmailTemplateDbHelper : DbHelper
{
  public EmailTemplateDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<EmailTemplate?> GetByKeyAsync(string templateKey, string languageCode)
      => await ExecuteAsync(() => _db.EmailTemplates
          .FirstOrDefaultAsync(e => e.TemplateKey == templateKey
                                 && e.LanguageCode == languageCode
                                 && e.Status == StatusConstants.Active));

  public async Task<List<EmailTemplate>> GetAllAsync()
      => await ExecuteAsync(() => _db.EmailTemplates
          .Where(e => e.Status != StatusConstants.Deleted)
          .OrderBy(e => e.TemplateKey)
          .ToListAsync());

  public async Task CreateAsync(EmailTemplate template)
      => await ExecuteAsync(async () =>
      {
        template.CreatedAt = DateTime.Now;
        _db.EmailTemplates.Add(template);
        await _db.SaveChangesAsync();
      });

  public async Task UpdateAsync(EmailTemplate template)
      => await ExecuteAsync(async () =>
      {
        _db.EmailTemplates.Update(template);
        await _db.SaveChangesAsync();
      });

  public async Task DeleteAsync(int id)
      => await ExecuteAsync(async () =>
      {
        var template = await _db.EmailTemplates.FindAsync(id);
        if (template != null)
        {
          template.Status = StatusConstants.Deleted;
          await _db.SaveChangesAsync();
        }
      });
}
