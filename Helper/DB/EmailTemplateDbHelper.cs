using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class EmailTemplateDbHelper : DbHelper
{
  public EmailTemplateDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<EmailTemplate?> GetByKeyAsync(string templateKey, string languageCode)
      => await ExecuteAsync(() => _db.EmailTemplates
          .FirstOrDefaultAsync(e => e.TemplateKey  == templateKey
                                 && e.LanguageCode == languageCode
                                 && e.Status       == StatusConstants.Active));

  public async Task<EmailTemplate?> GetByIdAsync(int id)
      => await ExecuteAsync(() => _db.EmailTemplates
          .FirstOrDefaultAsync(e => e.Id == id && e.Status != StatusConstants.Deleted));

  public async Task<List<EmailTemplate>> GetAllAsync()
      => await ExecuteAsync(() => _db.EmailTemplates
          .Where(e => e.Status != StatusConstants.Deleted)
          .OrderBy(e => e.TemplateKey)
          .ThenBy(e => e.LanguageCode)
          .ToListAsync());

  public async Task<List<string>> GetAllKeysAsync()
      => await ExecuteAsync(() => _db.EmailTemplates
          .Where(e => e.Status != StatusConstants.Deleted)
          .Select(e => e.TemplateKey)
          .Distinct()
          .OrderBy(k => k)
          .ToListAsync());

  public async Task<EmailTemplateAddResult> AddAsync(EmailTemplate template, string createdBy)
      => await ExecuteAsync(async () =>
      {
        bool exists = await _db.EmailTemplates.AnyAsync(e =>
            e.TemplateKey  == template.TemplateKey &&
            e.LanguageCode == template.LanguageCode &&
            e.Status       != StatusConstants.Deleted);

        if (exists) return EmailTemplateAddResult.Duplicate;

        template.CreatedBy = createdBy;
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedBy = createdBy;
        template.UpdatedAt = DateTime.UtcNow;
        _db.EmailTemplates.Add(template);
        await _db.SaveChangesAsync();
        return EmailTemplateAddResult.Created;
      });

  public async Task UpdateAsync(EmailTemplate template, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.EmailTemplates.FindAsync(template.Id);
        if (existing == null) return;
        existing.Subject   = template.Subject;
        existing.BodyHtml  = template.BodyHtml;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.EmailTemplates.FindAsync(id);
        if (existing == null) return;
        existing.Status    = status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });
}
