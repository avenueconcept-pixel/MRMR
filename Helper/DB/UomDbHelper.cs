using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class UomDbHelper : DbHelper
{
  public UomDbHelper(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

  public async Task<List<UnitOfMeasure>> GetAllAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.UnitsOfMeasure
            .Where(u => u.Status != StatusConstants.Deleted)
            .Include(u => u.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return items
            .OrderBy(u => u.Translations.FirstOrDefault()?.UomName ?? u.UomCode)
            .ToList();
      });

  public async Task<List<UnitOfMeasure>> GetAllActiveAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.UnitsOfMeasure
            .Where(u => u.Status == StatusConstants.Active)
            .Include(u => u.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return items
            .OrderBy(u => u.Translations.FirstOrDefault()?.UomName ?? u.UomCode)
            .ToList();
      });

  public async Task<UnitOfMeasure?> GetByCodeAsync(string uomCode)
      => await ExecuteAsync(async () =>
          await _db.UnitsOfMeasure
              .Include(u => u.Translations)
              .FirstOrDefaultAsync(u => u.UomCode == uomCode));

  public async Task<UomAddResult> AddAsync(UnitOfMeasure uom, List<UomTranslation> translations, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.UnitsOfMeasure
            .Include(u => u.Translations)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.UomCode == uom.UomCode);

        if (existing != null)
        {
          if (existing.Status != StatusConstants.Deleted)
            return UomAddResult.DuplicateActive;

          existing.UomName   = uom.UomName;
          existing.Status    = StatusConstants.Active;
          existing.UpdatedBy = createdBy;
          existing.UpdatedAt = DateTime.UtcNow;

          _db.UomTranslations.RemoveRange(existing.Translations);
          foreach (var t in translations)
          {
            t.UomCode = existing.UomCode;
            _db.UomTranslations.Add(t);
          }
          await _db.SaveChangesAsync();
          return UomAddResult.Restored;
        }

        uom.CreatedBy = createdBy;
        uom.CreatedAt = DateTime.UtcNow;
        uom.UpdatedBy = createdBy;
        uom.UpdatedAt = DateTime.UtcNow;
        uom.Status    = StatusConstants.Active;
        _db.UnitsOfMeasure.Add(uom);
        await _db.SaveChangesAsync();

        foreach (var t in translations)
        {
          t.UomCode = uom.UomCode;
          _db.UomTranslations.Add(t);
        }
        await _db.SaveChangesAsync();
        return UomAddResult.Created;
      });

  public async Task UpdateAsync(UnitOfMeasure uom, List<UomTranslation> translations, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.UnitsOfMeasure
            .Include(u => u.Translations)
            .FirstOrDefaultAsync(u => u.UomCode == uom.UomCode);
        if (existing == null) return;

        existing.UomName   = uom.UomName;
        existing.Status    = uom.Status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;

        _db.UomTranslations.RemoveRange(existing.Translations);
        foreach (var t in translations)
        {
          t.UomCode = uom.UomCode;
          _db.UomTranslations.Add(t);
        }
        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(string uomCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.UnitsOfMeasure.FirstOrDefaultAsync(u => u.UomCode == uomCode);
        if (existing == null) return;
        existing.Status    = status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });
}
