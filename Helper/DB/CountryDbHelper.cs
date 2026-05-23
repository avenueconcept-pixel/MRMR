using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class CountryDbHelper : DbHelper
{
  public CountryDbHelper(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

  public async Task<List<Country>> GetAllCountriesAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var countries = await _db.Countries
            .Where(c => c.Status != UserStatusConstants.Deleted)
            .Include(c => c.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return countries
            .OrderBy(c => c.Translations.FirstOrDefault()?.CountryName ?? c.CountryCode)
            .ToList();
      });

  public async Task<Country?> GetCountryByCodeAsync(string countryCode)
      => await ExecuteAsync(() => _db.Countries
          .Include(c => c.Translations)
          .FirstOrDefaultAsync(c => c.CountryCode == countryCode));

  public async Task AddCountryAsync(Country country, List<CountryTranslation> translations, string createdBy)
      => await ExecuteAsync(async () =>
      {
        country.CreatedAt = DateTime.UtcNow;
        country.CreatedBy = createdBy;
        country.UpdatedAt = DateTime.UtcNow;
        country.UpdatedBy = createdBy;
        _db.Countries.Add(country);
        _db.CountryTranslations.AddRange(translations);
        await _db.SaveChangesAsync();
      });

  public async Task UpdateCountryAsync(Country country, List<CountryTranslation> translations, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Countries.FindAsync(country.CountryCode);
        if (existing == null) return;

        existing.Status = country.Status;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = updatedBy;

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
      });

  public async Task UpdateCountryStatusAsync(string countryCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var country = await _db.Countries.FindAsync(countryCode);
        if (country != null)
        {
          country.Status = status;
          country.UpdatedAt = DateTime.UtcNow;
          country.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
        }
      });
}
