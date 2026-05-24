using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class CountryDbHelper : DbHelper
{
  public CountryDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<Country>> GetAllAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var countries = await _db.Countries
            .Where(c => c.Status != StatusConstants.Deleted)
            .Include(c => c.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return countries
            .OrderBy(c => c.Translations.FirstOrDefault()?.CountryName ?? c.CountryCode)
            .ToList();
      });

  public async Task<Country?> GetByCodeAsync(string countryCode)
      => await ExecuteAsync(() => _db.Countries
          .Include(c => c.Translations)
          .FirstOrDefaultAsync(c => c.CountryCode == countryCode && c.Status != StatusConstants.Deleted));

  public async Task<CountryAddResult> AddAsync(Country country, List<CountryTranslation> translations, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Countries
            .FirstOrDefaultAsync(c => c.CountryCode == country.CountryCode);

        if (existing != null && existing.Status == StatusConstants.Deleted)
        {
          existing.CurrencyCode = country.CurrencyCode;
          existing.Timezone     = country.Timezone;
          existing.Status       = StatusConstants.Active;
          existing.UpdatedAt    = DateTime.UtcNow;
          existing.UpdatedBy    = createdBy;

          var existingTranslations = await _db.CountryTranslations
              .Where(t => t.CountryCode == country.CountryCode)
              .ToListAsync();

          foreach (var translation in translations)
          {
            var existingT = existingTranslations.FirstOrDefault(x => x.LanguageCode == translation.LanguageCode);
            if (existingT != null)
              existingT.CountryName = translation.CountryName;
            else
              _db.CountryTranslations.Add(translation);
          }

          await _db.SaveChangesAsync();
          await _audit.LogActionAsync("countries", existing.CountryCode, AuditConstants.Actions.Restore, createdBy, remarks: "Restored from deleted");
          return CountryAddResult.Restored;
        }

        if (existing != null)
          return CountryAddResult.DuplicateActive;

        country.CreatedAt = DateTime.UtcNow;
        country.CreatedBy = createdBy;
        country.UpdatedAt = DateTime.UtcNow;
        country.UpdatedBy = createdBy;
        _db.Countries.Add(country);
        _db.CountryTranslations.AddRange(translations);
        await _db.SaveChangesAsync();
        await _audit.LogInsertAsync("countries", country.CountryCode, country, createdBy);
        return CountryAddResult.Created;
      });

  public async Task UpdateAsync(Country country, List<CountryTranslation> translations, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Countries.FindAsync(country.CountryCode);
        if (existing == null) return;

        var old = new Country
        {
          CountryCode  = existing.CountryCode,
          CurrencyCode = existing.CurrencyCode,
          Timezone     = existing.Timezone,
          Status       = existing.Status
        };

        existing.CurrencyCode = country.CurrencyCode;
        existing.Status       = country.Status;
        existing.Timezone     = country.Timezone;
        existing.UpdatedAt    = DateTime.UtcNow;
        existing.UpdatedBy    = updatedBy;

        var existingTranslations = await _db.CountryTranslations
            .Where(t => t.CountryCode == country.CountryCode)
            .ToListAsync();

        foreach (var translation in translations)
        {
          var existingT = existingTranslations.FirstOrDefault(x => x.LanguageCode == translation.LanguageCode);
          if (existingT != null)
            existingT.CountryName = translation.CountryName;
          else
            _db.CountryTranslations.Add(translation);
        }

        await _db.SaveChangesAsync();
        await _audit.LogUpdateAsync("countries", existing.CountryCode, old, existing, updatedBy);
      });

  public async Task UpdateStatusAsync(string countryCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var country = await _db.Countries.FindAsync(countryCode);
        if (country != null)
        {
          var oldStatus = country.Status;
          country.Status    = status;
          country.UpdatedAt = DateTime.UtcNow;
          country.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
          var action = status == StatusConstants.Deleted
              ? AuditConstants.Actions.Delete
              : AuditConstants.Actions.Update;
          await _audit.LogActionAsync("countries", countryCode, action, updatedBy,
              remarks: $"Status changed from {oldStatus} to {status}");
        }
      });
}
