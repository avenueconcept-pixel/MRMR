using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class PaymentMethodDbHelper : DbHelper
{
  public PaymentMethodDbHelper(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

  public async Task<List<PaymentMethod>> GetAllAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var methods = await _db.PaymentMethods
            .Where(p => p.Status != StatusConstants.Deleted)
            .Include(p => p.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return methods
            .OrderBy(p => p.Translations.FirstOrDefault()?.PaymentName ?? p.PaymentCode)
            .ToList();
      });

  public async Task<PaymentMethod?> GetByCodeAsync(string paymentCode)
      => await ExecuteAsync(() => _db.PaymentMethods
          .Include(p => p.Translations)
          .FirstOrDefaultAsync(p => p.PaymentCode == paymentCode && p.Status != StatusConstants.Deleted));

  public async Task<List<PaymentMethod>> GetAllActiveAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var methods = await _db.PaymentMethods
            .Where(p => p.Status == StatusConstants.Active)
            .Include(p => p.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return methods
            .OrderBy(p => p.Translations.FirstOrDefault()?.PaymentName ?? p.PaymentCode)
            .ToList();
      });

  public async Task<PaymentMethodAddResult> AddAsync(PaymentMethod pm, List<PaymentMethodTranslation> translations, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.PaymentMethods
            .FirstOrDefaultAsync(p => p.PaymentCode == pm.PaymentCode);

        if (existing != null && existing.Status == StatusConstants.Deleted)
        {
          existing.Status    = StatusConstants.Active;
          existing.UpdatedAt = DateTime.UtcNow;
          existing.UpdatedBy = createdBy;

          var existingTranslations = await _db.PaymentMethodTranslations
              .Where(t => t.PaymentCode == pm.PaymentCode)
              .ToListAsync();

          foreach (var translation in translations)
          {
            var existingT = existingTranslations.FirstOrDefault(x => x.LanguageCode == translation.LanguageCode);
            if (existingT != null)
              existingT.PaymentName = translation.PaymentName;
            else
              _db.PaymentMethodTranslations.Add(translation);
          }

          await _db.SaveChangesAsync();
          return PaymentMethodAddResult.Restored;
        }

        if (existing != null)
          return PaymentMethodAddResult.DuplicateActive;

        pm.CreatedAt = DateTime.UtcNow;
        pm.CreatedBy = createdBy;
        pm.UpdatedAt = DateTime.UtcNow;
        pm.UpdatedBy = createdBy;
        _db.PaymentMethods.Add(pm);
        _db.PaymentMethodTranslations.AddRange(translations);
        await _db.SaveChangesAsync();
        return PaymentMethodAddResult.Created;
      });

  public async Task UpdateAsync(PaymentMethod pm, List<PaymentMethodTranslation> translations, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.PaymentMethods.FindAsync(pm.PaymentCode);
        if (existing == null) return;

        existing.Status    = pm.Status;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = updatedBy;

        var existingTranslations = await _db.PaymentMethodTranslations
            .Where(t => t.PaymentCode == pm.PaymentCode)
            .ToListAsync();

        foreach (var translation in translations)
        {
          var existingT = existingTranslations.FirstOrDefault(x => x.LanguageCode == translation.LanguageCode);
          if (existingT != null)
            existingT.PaymentName = translation.PaymentName;
          else
            _db.PaymentMethodTranslations.Add(translation);
        }

        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(string paymentCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var pm = await _db.PaymentMethods.FindAsync(paymentCode);
        if (pm != null)
        {
          pm.Status    = status;
          pm.UpdatedAt = DateTime.UtcNow;
          pm.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
        }
      });
}
