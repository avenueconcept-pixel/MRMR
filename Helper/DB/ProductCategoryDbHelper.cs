using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class ProductCategoryDbHelper : DbHelper
{
  public ProductCategoryDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<ProductCategory>> GetAllAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.ProductCategories
            .Where(c => c.Status != StatusConstants.Deleted)
            .Include(c => c.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return items
            .OrderBy(c => c.Translations.FirstOrDefault()?.CategoryName ?? c.CategoryCode)
            .ToList();
      });

  public async Task<ProductCategory?> GetByCodeAsync(string categoryCode)
      => await ExecuteAsync(() => _db.ProductCategories
          .Include(c => c.Translations)
          .FirstOrDefaultAsync(c => c.CategoryCode == categoryCode && c.Status != StatusConstants.Deleted));

  public async Task<List<ProductCategory>> GetAllActiveAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.ProductCategories
            .Where(c => c.Status == StatusConstants.Active)
            .Include(c => c.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return items
            .OrderBy(c => c.Translations.FirstOrDefault()?.CategoryName ?? c.CategoryCode)
            .ToList();
      });

  public async Task<ProductCategoryAddResult> AddAsync(ProductCategory category, List<ProductCategoryTranslation> translations, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.ProductCategories
            .FirstOrDefaultAsync(c => c.CategoryCode == category.CategoryCode);

        if (existing != null && existing.Status == StatusConstants.Deleted)
        {
          existing.Status    = StatusConstants.Active;
          existing.UpdatedAt = DateTime.UtcNow;
          existing.UpdatedBy = createdBy;

          var existingTranslations = await _db.ProductCategoryTranslations
              .Where(t => t.CategoryCode == category.CategoryCode)
              .ToListAsync();

          foreach (var translation in translations)
          {
            var existingT = existingTranslations.FirstOrDefault(x => x.LanguageCode == translation.LanguageCode);
            if (existingT != null)
              existingT.CategoryName = translation.CategoryName;
            else
              _db.ProductCategoryTranslations.Add(translation);
          }

          await _db.SaveChangesAsync();
          await _audit.LogActionAsync("product_categories", existing.CategoryCode, AuditConstants.Actions.Restore, createdBy, remarks: "Restored from deleted");
          return ProductCategoryAddResult.Restored;
        }

        if (existing != null)
          return ProductCategoryAddResult.DuplicateActive;

        category.CreatedAt = DateTime.UtcNow;
        category.CreatedBy = createdBy;
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = createdBy;
        _db.ProductCategories.Add(category);
        _db.ProductCategoryTranslations.AddRange(translations);
        await _db.SaveChangesAsync();
        await _audit.LogInsertAsync("product_categories", category.CategoryCode, category, createdBy);
        return ProductCategoryAddResult.Created;
      });

  public async Task UpdateAsync(ProductCategory category, List<ProductCategoryTranslation> translations, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.ProductCategories.FindAsync(category.CategoryCode);
        if (existing == null) return;

        var old = new ProductCategory
        {
          CategoryCode = existing.CategoryCode,
          Status       = existing.Status
        };

        existing.Status    = category.Status;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = updatedBy;

        var existingTranslations = await _db.ProductCategoryTranslations
            .Where(t => t.CategoryCode == category.CategoryCode)
            .ToListAsync();

        foreach (var translation in translations)
        {
          var existingT = existingTranslations.FirstOrDefault(x => x.LanguageCode == translation.LanguageCode);
          if (existingT != null)
            existingT.CategoryName = translation.CategoryName;
          else
            _db.ProductCategoryTranslations.Add(translation);
        }

        await _db.SaveChangesAsync();
        await _audit.LogUpdateAsync("product_categories", existing.CategoryCode, old, existing, updatedBy);
      });

  public async Task UpdateStatusAsync(string categoryCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var category = await _db.ProductCategories.FindAsync(categoryCode);
        if (category != null)
        {
          var oldStatus    = category.Status;
          category.Status    = status;
          category.UpdatedAt = DateTime.UtcNow;
          category.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
          var action = status == StatusConstants.Deleted
              ? AuditConstants.Actions.Delete
              : AuditConstants.Actions.Update;
          await _audit.LogActionAsync("product_categories", categoryCode, action, updatedBy,
              remarks: $"Status changed from {oldStatus} to {status}");
        }
      });
}
