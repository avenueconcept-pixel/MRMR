using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class ProductDbHelper : DbHelper
{
  public ProductDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<List<Product>> GetAllAsync(
      string  languageCode,
      string? filterStatus       = null,
      string? filterType         = null,
      string? filterCategoryCode = null,
      string? filterCountryCode  = null)
      => await ExecuteAsync(async () =>
      {
        var query = _db.Products.AsQueryable();

        query = string.IsNullOrEmpty(filterStatus)
            ? query.Where(p => p.Status != StatusConstants.Deleted)
            : query.Where(p => p.Status == filterStatus);

        if (!string.IsNullOrEmpty(filterType))
            query = query.Where(p => p.ProductType == filterType);

        if (!string.IsNullOrEmpty(filterCategoryCode))
            query = query.Where(p => p.CategoryMaps.Any(m => m.CategoryCode == filterCategoryCode));

        if (!string.IsNullOrEmpty(filterCountryCode))
            query = query.Where(p => p.Countries.Any(c => c.CountryCode == filterCountryCode));

        var items = await query
            .Include(p => p.Translations.Where(t => t.LanguageCode == languageCode))
            .Include(p => p.CategoryMaps)
            .Include(p => p.Countries)
            .ToListAsync();

        return items
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.ProductCode)
            .ToList();
      });

  public async Task<List<Product>> GetAllActiveAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Products
            .Where(p => p.Status == StatusConstants.Active)
            .Include(p => p.Translations.Where(t => t.LanguageCode == languageCode))
            .Include(p => p.CategoryMaps)
            .Include(p => p.Countries)
            .ToListAsync();

        return items
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.ProductCode)
            .ToList();
      });

  public async Task<Product?> GetByCodeAsync(string productCode)
      => await ExecuteAsync(async () =>
          await _db.Products
              .Where(p => p.ProductCode == productCode && p.Status != StatusConstants.Deleted)
              .Include(p => p.Translations)
              .Include(p => p.CategoryMaps).ThenInclude(m => m.ProductCategory).ThenInclude(c => c.Translations)
              .Include(p => p.Countries).ThenInclude(c => c.Country)
              .Include(p => p.UnitOfMeasure)
              .FirstOrDefaultAsync());

  public async Task<ProductAddResult> AddAsync(
      Product product,
      List<ProductTranslation> translations,
      List<string> categoryCodes,
      List<ProductCountry> countries,
      List<ProductPriceTier> priceTiers,
      string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ProductCode == product.ProductCode);

        if (existing != null && existing.Status != StatusConstants.Deleted)
          return ProductAddResult.DuplicateActive;

        if (existing != null)
        {
          existing.ProductType   = product.ProductType;
          existing.ProductNature = product.ProductNature;
          existing.UomCode       = product.UomCode;
          existing.Pv            = product.Pv;
          existing.SortOrder     = product.SortOrder;
          existing.Status        = StatusConstants.Active;
          existing.UpdatedBy     = createdBy;
          existing.UpdatedAt     = DateTime.UtcNow;

          _db.ProductTranslations.RemoveRange(
              await _db.ProductTranslations.Where(t => t.ProductCode == existing.ProductCode).ToListAsync());
          _db.ProductCategoryMaps.RemoveRange(
              await _db.ProductCategoryMaps.Where(c => c.ProductCode == existing.ProductCode).ToListAsync());
          _db.ProductCountries.RemoveRange(
              await _db.ProductCountries.Where(c => c.ProductCode == existing.ProductCode).ToListAsync());
          _db.ProductPriceTiers.RemoveRange(
              await _db.ProductPriceTiers.Where(p => p.ProductCode == existing.ProductCode).ToListAsync());

          foreach (var t in translations) { t.ProductCode = existing.ProductCode; _db.ProductTranslations.Add(t); }
          foreach (var c in categoryCodes) _db.ProductCategoryMaps.Add(new ProductCategoryMap { ProductCode = existing.ProductCode, CategoryCode = c });
          foreach (var c in countries)     { c.ProductCode = existing.ProductCode; _db.ProductCountries.Add(c); }
          foreach (var p in priceTiers)    { p.ProductCode = existing.ProductCode; _db.ProductPriceTiers.Add(p); }

          await _db.SaveChangesAsync();
          return ProductAddResult.Restored;
        }

        product.CreatedBy  = createdBy;
        product.CreatedAt  = DateTime.UtcNow;
        product.UpdatedBy  = createdBy;
        product.UpdatedAt  = DateTime.UtcNow;

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        foreach (var t in translations) { t.ProductCode = product.ProductCode; _db.ProductTranslations.Add(t); }
        foreach (var c in categoryCodes) _db.ProductCategoryMaps.Add(new ProductCategoryMap { ProductCode = product.ProductCode, CategoryCode = c });
        foreach (var c in countries)     { c.ProductCode = product.ProductCode; _db.ProductCountries.Add(c); }
        foreach (var p in priceTiers)    { p.ProductCode = product.ProductCode; _db.ProductPriceTiers.Add(p); }

        await _db.SaveChangesAsync();
        return ProductAddResult.Created;
      });

  public async Task UpdateAsync(
      Product product,
      List<ProductTranslation> translations,
      List<string> categoryCodes,
      List<ProductCountry> countries,
      List<ProductPriceTier> priceTiers,
      string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Products
            .Include(p => p.Translations)
            .Include(p => p.CategoryMaps)
            .Include(p => p.Countries)
            .Include(p => p.PriceTiers)
            .FirstOrDefaultAsync(p => p.ProductCode == product.ProductCode);
        if (existing == null) return;

        existing.ProductType   = product.ProductType;
        existing.ProductNature = product.ProductNature;
        existing.UomCode       = product.UomCode;
        existing.Pv            = product.Pv;
        existing.SortOrder     = product.SortOrder;
        existing.Status        = product.Status;
        existing.UpdatedBy     = updatedBy;
        existing.UpdatedAt     = DateTime.UtcNow;

        _db.ProductTranslations.RemoveRange(existing.Translations);
        _db.ProductCategoryMaps.RemoveRange(existing.CategoryMaps);
        _db.ProductCountries.RemoveRange(existing.Countries);
        _db.ProductPriceTiers.RemoveRange(existing.PriceTiers);

        foreach (var t in translations) { t.ProductCode = existing.ProductCode; _db.ProductTranslations.Add(t); }
        foreach (var c in categoryCodes) _db.ProductCategoryMaps.Add(new ProductCategoryMap { ProductCode = existing.ProductCode, CategoryCode = c });
        foreach (var c in countries)     { c.ProductCode = existing.ProductCode; _db.ProductCountries.Add(c); }
        foreach (var p in priceTiers)    { p.ProductCode = existing.ProductCode; _db.ProductPriceTiers.Add(p); }

        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(string productCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.ProductCode == productCode);
        if (existing == null) return;
        existing.Status    = status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  // ── Package items ─────────────────────────────────────────────────────────

  public async Task<List<ProductPackageItem>> GetPackageItemsAsync(string packageCode)
      => await ExecuteAsync(async () =>
          await _db.ProductPackageItems
              .Where(p => p.PackageProductCode == packageCode)
              .Include(p => p.ItemProduct).ThenInclude(c => c!.Translations)
              .OrderBy(p => p.SortOrder)
              .ToListAsync());

  public async Task AddPackageItemAsync(ProductPackageItem item)
      => await ExecuteAsync(async () =>
      {
        var maxSort = await _db.ProductPackageItems
            .Where(p => p.PackageProductCode == item.PackageProductCode)
            .MaxAsync(p => (int?)p.SortOrder) ?? 0;
        item.SortOrder = maxSort + 1;
        _db.ProductPackageItems.Add(item);
        await _db.SaveChangesAsync();
      });

  public async Task DeletePackageItemAsync(int id)
      => await ExecuteAsync(async () =>
      {
        var item = await _db.ProductPackageItems.FindAsync(id);
        if (item != null)
        {
          _db.ProductPackageItems.Remove(item);
          await _db.SaveChangesAsync();
        }
      });

  public async Task SavePackageItemSortAsync(List<PackageItemSortItem> items)
      => await ExecuteAsync(async () =>
      {
        foreach (var s in items)
        {
          var item = await _db.ProductPackageItems.FindAsync(s.Id);
          if (item != null) item.SortOrder = s.SortOrder;
        }
        await _db.SaveChangesAsync();
      });

  public async Task<List<Product>> GetAllActiveNonPackageAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Products
            .Where(p => p.Status == StatusConstants.Active
                     && p.ProductType != ProductTypeConstants.Package)
            .Include(p => p.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return items
            .OrderBy(p => p.Translations.FirstOrDefault()?.ProductName ?? p.ProductCode)
            .ToList();
      });

  public async Task<List<ProductSection>> GetSectionsAsync(string productCode)
      => await ExecuteAsync(async () =>
          await _db.ProductSections
              .Where(s => s.ProductCode == productCode)
              .Include(s => s.ProductSectionType).ThenInclude(t => t.Translations)
              .Include(s => s.Translations)
              .OrderBy(s => s.SortOrder)
              .ToListAsync());

  public async Task SaveSectionSortAsync(List<SectionSortItem> items, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        foreach (var item in items)
        {
          var s = await _db.ProductSections.FindAsync(item.Id);
          if (s != null) s.SortOrder = item.SortOrder;
        }
        await _db.SaveChangesAsync();
      });

  public async Task AddSectionAsync(ProductSection section, List<ProductSectionTranslation> translations, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        _db.ProductSections.Add(section);
        await _db.SaveChangesAsync();

        foreach (var t in translations)
        {
          t.ProductSectionId = section.Id;
          _db.ProductSectionTranslations.Add(t);
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductCode == section.ProductCode);
        if (product != null) { product.UpdatedBy = updatedBy; product.UpdatedAt = DateTime.UtcNow; }

        await _db.SaveChangesAsync();
      });

  public async Task UpdateSectionAsync(ProductSection section, List<ProductSectionTranslation> translations, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.ProductSections
            .Include(s => s.Translations)
            .FirstOrDefaultAsync(s => s.Id == section.Id);
        if (existing == null) return;

        existing.SectionCode = section.SectionCode;
        existing.SortOrder   = section.SortOrder;

        _db.ProductSectionTranslations.RemoveRange(existing.Translations);
        foreach (var t in translations)
        {
          t.ProductSectionId = existing.Id;
          _db.ProductSectionTranslations.Add(t);
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductCode == existing.ProductCode);
        if (product != null) { product.UpdatedBy = updatedBy; product.UpdatedAt = DateTime.UtcNow; }

        await _db.SaveChangesAsync();
      });

  public async Task DeleteSectionAsync(int productSectionId, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var section = await _db.ProductSections.FindAsync(productSectionId);
        if (section == null) return;

        var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductCode == section.ProductCode);
        if (product != null) { product.UpdatedBy = updatedBy; product.UpdatedAt = DateTime.UtcNow; }

        _db.ProductSections.Remove(section);
        await _db.SaveChangesAsync();
      });

  public async Task AddImageAsync(ProductImage image, string createdBy)
      => await ExecuteAsync(async () =>
      {
        image.CreatedBy = createdBy;
        image.CreatedAt = DateTime.UtcNow;
        _db.ProductImages.Add(image);
        await _db.SaveChangesAsync();
      });

  public async Task UpdateImageSortAsync(List<int> imageIds, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        for (int i = 0; i < imageIds.Count; i++)
        {
          var img = await _db.ProductImages.FindAsync(imageIds[i]);
          if (img != null) img.SortOrder = i;
        }
        await _db.SaveChangesAsync();
      });

  public async Task DeleteImageAsync(int imageId)
      => await ExecuteAsync(async () =>
      {
        var img = await _db.ProductImages.FindAsync(imageId);
        if (img == null) return;
        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync();
      });

  public async Task SetPrimaryImageAsync(string productCode, int imageId)
      => await ExecuteAsync(async () =>
      {
        var images = await _db.ProductImages
            .Where(i => i.ProductCode == productCode)
            .ToListAsync();
        foreach (var img in images)
          img.IsPrimary = img.Id == imageId;
        await _db.SaveChangesAsync();
      });

  public async Task<List<ProductImage>> GetImagesAsync(string productCode)
      => await ExecuteAsync(async () =>
          await _db.ProductImages
              .Where(i => i.ProductCode == productCode)
              .Include(i => i.Country)
              .OrderBy(i => i.SortOrder)
              .ToListAsync());

  // ── Pricing ───────────────────────────────────────────────────────────────

  public async Task<List<ProductPriceTier>> GetPriceTiersAsync(string productCode)
      => await ExecuteAsync(async () =>
          await _db.ProductPriceTiers
              .Where(p => p.ProductCode == productCode)
              .Include(p => p.Country).ThenInclude(c => c.Translations)
              .Include(p => p.PriceTier)
              .OrderBy(p => p.CountryCode)
              .ThenBy(p => p.TierCode)
              .ToListAsync());

  public async Task AddPriceTierEntryAsync(ProductPriceTier tier, string createdBy)
      => await ExecuteAsync(async () =>
      {
        _db.ProductPriceTiers.Add(tier);
        await _db.SaveChangesAsync();

        _db.ProductPriceHistories.Add(new ProductPriceHistory
        {
          ProductCode = tier.ProductCode,
          CountryCode = tier.CountryCode,
          TierCode    = tier.TierCode,
          ChangeType  = PriceChangeTypeConstants.ManualUpdate,
          ChangedFrom = 0,
          ChangedTo   = tier.Price,
          ChangedBy   = createdBy,
          CreatedAt   = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
      });

  public async Task UpdatePriceTierAsync(int id, decimal newPrice, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var tier = await _db.ProductPriceTiers.FindAsync(id);
        if (tier == null) return;

        _db.ProductPriceHistories.Add(new ProductPriceHistory
        {
          ProductCode = tier.ProductCode,
          CountryCode = tier.CountryCode,
          TierCode    = tier.TierCode,
          ChangeType  = PriceChangeTypeConstants.ManualUpdate,
          ChangedFrom = tier.Price,
          ChangedTo   = newPrice,
          ChangedBy   = updatedBy,
          CreatedAt   = DateTime.UtcNow
        });

        tier.Price = newPrice;
        await _db.SaveChangesAsync();
      });

  public async Task<List<ProductPriceSchedule>> GetPriceSchedulesAsync(string productCode)
      => await ExecuteAsync(async () =>
          await _db.ProductPriceSchedules
              .Where(s => s.ProductCode == productCode)
              .Include(s => s.Country).ThenInclude(c => c.Translations)
              .Include(s => s.PriceTier)
              .OrderByDescending(s => s.ValidFrom)
              .ToListAsync());

  public async Task AddPriceScheduleAsync(ProductPriceSchedule schedule, string createdBy)
      => await ExecuteAsync(async () =>
      {
        schedule.Status    = ScheduleStatusConstants.Pending;
        schedule.CreatedBy = createdBy;
        schedule.CreatedAt = DateTime.UtcNow;
        schedule.UpdatedBy = createdBy;
        schedule.UpdatedAt = DateTime.UtcNow;
        _db.ProductPriceSchedules.Add(schedule);
        await _db.SaveChangesAsync();
      });

  public async Task CancelPriceScheduleAsync(int id, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var schedule = await _db.ProductPriceSchedules.FindAsync(id);
        if (schedule == null) return;
        schedule.Status    = ScheduleStatusConstants.Cancelled;
        schedule.UpdatedBy = updatedBy;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task<(List<PriceHistoryRowDto> Items, int TotalCount)> GetPriceHistoryAsync(
      string productCode, int page, int pageSize)
      => await ExecuteAsync(async () =>
      {
        var query = _db.ProductPriceHistories
            .Where(h => h.ProductCode == productCode)
            .OrderByDescending(h => h.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new PriceHistoryRowDto
            {
              Id          = h.Id,
              CountryCode = h.CountryCode,
              TierCode    = h.TierCode,
              ChangeType  = h.ChangeType,
              ChangedFrom = h.ChangedFrom,
              ChangedTo   = h.ChangedTo,
              ChangedBy   = h.ChangedBy,
              CreatedAt   = h.CreatedAt
            })
            .ToListAsync();

        return (items, total);
      });

  public async Task DeletePriceTierEntryAsync(int id, string deletedBy)
      => await ExecuteAsync(async () =>
      {
        var tier = await _db.ProductPriceTiers.FindAsync(id);
        if (tier == null) return;

        _db.ProductPriceHistories.Add(new ProductPriceHistory
        {
          ProductCode = tier.ProductCode,
          CountryCode = tier.CountryCode,
          TierCode    = tier.TierCode,
          ChangeType  = PriceChangeTypeConstants.ManualUpdate,
          ChangedFrom = tier.Price,
          ChangedTo   = 0,
          ChangedBy   = deletedBy,
          CreatedAt   = DateTime.UtcNow
        });

        _db.ProductPriceTiers.Remove(tier);
        await _db.SaveChangesAsync();
      });
}
