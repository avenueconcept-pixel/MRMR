using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class AnnouncementDbHelper : DbHelper
{
  public AnnouncementDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<List<Announcement>> GetAllAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Announcements
            .Where(a => a.Status != StatusConstants.Deleted)
            .Include(a => a.Translations.Where(t => t.LanguageCode == languageCode))
            .OrderBy(a => a.SortOrder)
            .ToListAsync();

        return items
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Translations.FirstOrDefault()?.Title ?? a.AnnouncementCode)
            .ToList();
      });

  public async Task<Announcement?> GetByCodeAsync(string announcementCode)
      => await ExecuteAsync(async () =>
          await _db.Announcements
              .Where(a => a.AnnouncementCode == announcementCode && a.Status != StatusConstants.Deleted)
              .Include(a => a.Translations)
              .Include(a => a.Attachments.OrderBy(at => at.SortOrder))
              .FirstOrDefaultAsync());

  public async Task<List<Announcement>> GetActiveForAudienceAsync(string audience, string languageCode)
      => await ExecuteAsync(async () =>
      {
        var now = DateTime.UtcNow;
        return await _db.Announcements
            .Where(a => a.Status == StatusConstants.Active
                     && a.StartAt <= now
                     && a.EndAt   >= now
                     && (a.Audience == audience || a.Audience == AnnouncementConstants.AudienceAll))
            .Include(a => a.Translations.Where(t => t.LanguageCode == languageCode))
            .OrderBy(a => a.SortOrder)
            .ToListAsync();
      });

  public async Task<AnnouncementAddResult> AddAsync(
      Announcement ann,
      List<AnnouncementTranslation> translations,
      string createdBy)
      => await ExecuteAsync(async () =>
      {
        bool exists = await _db.Announcements.AnyAsync(a =>
            a.AnnouncementCode == ann.AnnouncementCode &&
            a.Status           != StatusConstants.Deleted);

        if (exists) return AnnouncementAddResult.DuplicateActive;

        ann.CreatedBy = createdBy;
        ann.CreatedAt = DateTime.UtcNow;
        ann.UpdatedBy = createdBy;
        ann.UpdatedAt = DateTime.UtcNow;

        foreach (var t in translations) t.AnnouncementCode = ann.AnnouncementCode;
        ann.Translations = translations;

        _db.Announcements.Add(ann);
        await _db.SaveChangesAsync();
        return AnnouncementAddResult.Created;
      });

  public async Task UpdateAsync(
      Announcement ann,
      List<AnnouncementTranslation> translations,
      string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Announcements
            .Include(a => a.Translations)
            .FirstOrDefaultAsync(a => a.AnnouncementCode == ann.AnnouncementCode);
        if (existing == null) return;

        existing.Audience  = ann.Audience;
        existing.StartAt   = ann.StartAt;
        existing.EndAt     = ann.EndAt;
        existing.SortOrder = ann.SortOrder;
        existing.Status    = ann.Status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;

        foreach (var input in translations)
        {
          var row = existing.Translations.FirstOrDefault(t => t.LanguageCode == input.LanguageCode);
          if (row != null)
          {
            row.Title = input.Title;
            row.Body  = input.Body;
          }
          else
          {
            existing.Translations.Add(new AnnouncementTranslation
            {
              AnnouncementCode = existing.AnnouncementCode,
              LanguageCode     = input.LanguageCode,
              Title            = input.Title,
              Body             = input.Body
            });
          }
        }

        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(string announcementCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Announcements
            .FirstOrDefaultAsync(a => a.AnnouncementCode == announcementCode);
        if (existing == null) return;
        existing.Status    = status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task AddAttachmentAsync(AnnouncementAttachment attachment)
      => await ExecuteAsync(async () =>
      {
        _db.AnnouncementAttachments.Add(attachment);
        await _db.SaveChangesAsync();
      });

  public async Task<AnnouncementAttachment?> DeleteAttachmentAsync(int attachmentId)
      => await ExecuteAsync(async () =>
      {
        var att = await _db.AnnouncementAttachments.FindAsync(attachmentId);
        if (att == null) return null;
        _db.AnnouncementAttachments.Remove(att);
        await _db.SaveChangesAsync();
        return att;
      });

  public async Task<List<AnnouncementAttachment>> GetAttachmentsAsync(string announcementCode)
      => await ExecuteAsync(async () =>
          await _db.AnnouncementAttachments
              .Where(a => a.AnnouncementCode == announcementCode)
              .OrderBy(a => a.SortOrder)
              .ToListAsync());
}
