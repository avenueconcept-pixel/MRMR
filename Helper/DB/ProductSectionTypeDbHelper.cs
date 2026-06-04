using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class ProductSectionTypeDbHelper : DbHelper
{
  public ProductSectionTypeDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<ProductSectionType>> GetAllAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.ProductSectionTypes
            .Where(p => p.Status != StatusConstants.Deleted)
            .Include(p => p.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return items
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Translations.FirstOrDefault()?.SectionName ?? p.SectionCode)
            .ToList();
      });

  public async Task<List<ProductSectionType>> GetAllActiveAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.ProductSectionTypes
            .Where(p => p.Status == StatusConstants.Active)
            .Include(p => p.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return items
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Translations.FirstOrDefault()?.SectionName ?? p.SectionCode)
            .ToList();
      });

  public async Task<ProductSectionType?> GetByCodeAsync(string sectionCode)
      => await ExecuteAsync(async () =>
          await _db.ProductSectionTypes
              .Include(p => p.Translations)
              .FirstOrDefaultAsync(p => p.SectionCode == sectionCode));

  public async Task<ProductSectionTypeAddResult> AddAsync(ProductSectionType section, List<ProductSectionTypeTranslation> translations, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.ProductSectionTypes
            .Include(p => p.Translations)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.SectionCode == section.SectionCode);

        if (existing != null)
        {
          if (existing.Status != StatusConstants.Deleted)
            return ProductSectionTypeAddResult.DuplicateActive;

          existing.SortOrder = section.SortOrder;
          existing.Status    = StatusConstants.Active;
          existing.UpdatedBy = createdBy;
          existing.UpdatedAt = DateTime.UtcNow;

          _db.ProductSectionTypeTranslations.RemoveRange(existing.Translations);
          foreach (var t in translations)
          {
            t.SectionCode = existing.SectionCode;
            _db.ProductSectionTypeTranslations.Add(t);
          }
          await _db.SaveChangesAsync();
          return ProductSectionTypeAddResult.Restored;
        }

        section.CreatedBy = createdBy;
        section.CreatedAt = DateTime.UtcNow;
        section.UpdatedBy = createdBy;
        section.UpdatedAt = DateTime.UtcNow;
        section.Status    = StatusConstants.Active;
        _db.ProductSectionTypes.Add(section);
        await _db.SaveChangesAsync();

        foreach (var t in translations)
        {
          t.SectionCode = section.SectionCode;
          _db.ProductSectionTypeTranslations.Add(t);
        }
        await _db.SaveChangesAsync();
        return ProductSectionTypeAddResult.Created;
      });

  public async Task UpdateAsync(ProductSectionType section, List<ProductSectionTypeTranslation> translations, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.ProductSectionTypes
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.SectionCode == section.SectionCode);
        if (existing == null) return;

        existing.SortOrder = section.SortOrder;
        existing.Status    = section.Status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;

        _db.ProductSectionTypeTranslations.RemoveRange(existing.Translations);
        foreach (var t in translations)
        {
          t.SectionCode = section.SectionCode;
          _db.ProductSectionTypeTranslations.Add(t);
        }
        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(string sectionCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.ProductSectionTypes
            .FirstOrDefaultAsync(p => p.SectionCode == sectionCode);
        if (existing == null) return;
        existing.Status    = status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });
}
