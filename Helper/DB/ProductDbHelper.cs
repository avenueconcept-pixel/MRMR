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
}
