using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class BankDbHelper : DbHelper
{
  public BankDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<Bank>> GetAllAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var banks = await _db.Banks
            .Where(b => b.Status != StatusConstants.Deleted)
            .Include(b => b.Translations.Where(t => t.LanguageCode == languageCode))
            .Include(b => b.Country)
                .ThenInclude(c => c!.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return banks
            .OrderBy(b => b.Translations.FirstOrDefault()?.BankName ?? b.BankCode)
            .ToList();
      });

  public async Task<Bank?> GetByCodeAsync(string bankCode)
      => await ExecuteAsync(() => _db.Banks
          .Include(b => b.Translations)
          .Include(b => b.Country)
          .FirstOrDefaultAsync(b => b.BankCode == bankCode && b.Status != StatusConstants.Deleted));

  public async Task<List<Bank>> GetAllActiveAsync(string languageCode)
      => await ExecuteAsync(async () =>
      {
        var banks = await _db.Banks
            .Where(b => b.Status == StatusConstants.Active)
            .Include(b => b.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return banks
            .OrderBy(b => b.Translations.FirstOrDefault()?.BankName ?? b.BankCode)
            .ToList();
      });

  public async Task<List<Bank>> GetAllActiveByCountryAsync(string countryCode, string languageCode)
      => await ExecuteAsync(async () =>
      {
        var banks = await _db.Banks
            .Where(b => b.Status == StatusConstants.Active && b.CountryCode == countryCode)
            .Include(b => b.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return banks
            .OrderBy(b => b.Translations.FirstOrDefault()?.BankName ?? b.BankCode)
            .ToList();
      });

  public async Task<BankAddResult> AddAsync(Bank bank, List<BankTranslation> translations, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Banks
            .FirstOrDefaultAsync(b => b.BankCode == bank.BankCode);

        if (existing != null && existing.Status == StatusConstants.Deleted)
        {
          existing.CountryCode = bank.CountryCode;
          existing.SwiftCode   = bank.SwiftCode;
          existing.LocalCode   = bank.LocalCode;
          existing.Website     = bank.Website;
          existing.Logo        = bank.Logo;
          existing.Status      = StatusConstants.Active;
          existing.UpdatedAt   = DateTime.UtcNow;
          existing.UpdatedBy   = createdBy;

          var existingTranslations = await _db.BankTranslations
              .Where(t => t.BankCode == bank.BankCode)
              .ToListAsync();

          foreach (var translation in translations)
          {
            var existingT = existingTranslations.FirstOrDefault(x => x.LanguageCode == translation.LanguageCode);
            if (existingT != null)
            {
              existingT.BankName  = translation.BankName;
              existingT.ShortName = translation.ShortName;
            }
            else
              _db.BankTranslations.Add(translation);
          }

          await _db.SaveChangesAsync();
          await _audit.LogActionAsync("banks", existing.BankCode, AuditConstants.Actions.Restore, createdBy, remarks: "Restored from deleted");
          return BankAddResult.Restored;
        }

        if (existing != null)
          return BankAddResult.DuplicateActive;

        bank.CreatedAt = DateTime.UtcNow;
        bank.CreatedBy = createdBy;
        bank.UpdatedAt = DateTime.UtcNow;
        bank.UpdatedBy = createdBy;
        _db.Banks.Add(bank);
        _db.BankTranslations.AddRange(translations);
        await _db.SaveChangesAsync();
        await _audit.LogInsertAsync("banks", bank.BankCode, bank, createdBy);
        return BankAddResult.Created;
      });

  public async Task UpdateAsync(Bank bank, List<BankTranslation> translations, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Banks.FindAsync(bank.BankCode);
        if (existing == null) return;

        var old = new Bank
        {
          BankCode    = existing.BankCode,
          CountryCode = existing.CountryCode,
          SwiftCode   = existing.SwiftCode,
          LocalCode   = existing.LocalCode,
          Website     = existing.Website,
          Logo        = existing.Logo,
          Status      = existing.Status
        };

        existing.CountryCode = bank.CountryCode;
        existing.SwiftCode   = bank.SwiftCode;
        existing.LocalCode   = bank.LocalCode;
        existing.Website     = bank.Website;
        existing.Logo        = bank.Logo;
        existing.Status      = bank.Status;
        existing.UpdatedAt   = DateTime.UtcNow;
        existing.UpdatedBy   = updatedBy;

        var existingTranslations = await _db.BankTranslations
            .Where(t => t.BankCode == bank.BankCode)
            .ToListAsync();

        foreach (var translation in translations)
        {
          var existingT = existingTranslations.FirstOrDefault(x => x.LanguageCode == translation.LanguageCode);
          if (existingT != null)
          {
            existingT.BankName  = translation.BankName;
            existingT.ShortName = translation.ShortName;
          }
          else
            _db.BankTranslations.Add(translation);
        }

        await _db.SaveChangesAsync();
        await _audit.LogUpdateAsync("banks", existing.BankCode, old, existing, updatedBy);
      });

  public async Task UpdateStatusAsync(string bankCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var bank = await _db.Banks.FindAsync(bankCode);
        if (bank != null)
        {
          var oldStatus = bank.Status;
          bank.Status    = status;
          bank.UpdatedAt = DateTime.UtcNow;
          bank.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
          var action = status == StatusConstants.Deleted
              ? AuditConstants.Actions.Delete
              : AuditConstants.Actions.Update;
          await _audit.LogActionAsync("banks", bankCode, action, updatedBy,
              remarks: $"Status changed from {oldStatus} to {status}");
        }
      });
}
