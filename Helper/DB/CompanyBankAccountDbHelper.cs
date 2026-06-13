using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class CompanyBankAccountDbHelper : DbHelper
{
    public CompanyBankAccountDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
        : base(db, audit, loggerFactory) { }

    public async Task<List<CompanyBankAccount>> GetAllAsync(string languageCode)
        => await ExecuteAsync(async () =>
            await _db.CompanyBankAccounts
                .Where(a => a.Status != StatusConstants.Deleted)
                .Include(a => a.Country)
                    .ThenInclude(c => c!.Translations.Where(t => t.LanguageCode == languageCode))
                .OrderBy(a => a.CountryCode)
                .ThenBy(a => a.BankName)
                .ToListAsync());

    public async Task<List<CompanyBankAccount>> GetAllActiveAsync(string languageCode)
        => await ExecuteAsync(async () =>
            await _db.CompanyBankAccounts
                .Where(a => a.Status == StatusConstants.Active)
                .Include(a => a.Country)
                    .ThenInclude(c => c!.Translations.Where(t => t.LanguageCode == languageCode))
                .OrderBy(a => a.CountryCode)
                .ThenBy(a => a.BankName)
                .ToListAsync());

    public async Task<List<CompanyBankAccount>> GetAllActiveByCountryAsync(string countryCode, string languageCode)
        => await ExecuteAsync(async () =>
            await _db.CompanyBankAccounts
                .Where(a => a.Status == StatusConstants.Active && a.CountryCode == countryCode)
                .Include(a => a.Country)
                    .ThenInclude(c => c!.Translations.Where(t => t.LanguageCode == languageCode))
                .OrderBy(a => a.BankName)
                .ToListAsync());

    public async Task<CompanyBankAccount?> GetByIdAsync(int id)
        => await ExecuteAsync(async () =>
            await _db.CompanyBankAccounts
                .Include(a => a.Country)
                .FirstOrDefaultAsync(a => a.Id == id));

    public async Task AddAsync(CompanyBankAccount account, string createdBy)
        => await ExecuteAsync(async () =>
        {
            account.CreatedBy = createdBy;
            account.CreatedAt = DateTime.UtcNow;
            account.UpdatedBy = createdBy;
            account.UpdatedAt = DateTime.UtcNow;
            _db.CompanyBankAccounts.Add(account);
            await _db.SaveChangesAsync();
            await _audit.LogInsertAsync("company_bank_accounts", account.Id.ToString(), account, createdBy);
        });

    public async Task UpdateAsync(CompanyBankAccount account, string updatedBy)
        => await ExecuteAsync(async () =>
        {
            var existing = await _db.CompanyBankAccounts.FindAsync(account.Id);
            if (existing == null) return;

            var old = new CompanyBankAccount
            {
                CountryCode   = existing.CountryCode,
                BankName      = existing.BankName,
                AccountName   = existing.AccountName,
                AccountNumber = existing.AccountNumber,
                Branch        = existing.Branch,
                Currency      = existing.Currency,
                Remarks       = existing.Remarks,
                Status        = existing.Status
            };

            existing.CountryCode   = account.CountryCode;
            existing.BankName      = account.BankName;
            existing.AccountName   = account.AccountName;
            existing.AccountNumber = account.AccountNumber;
            existing.Branch        = account.Branch;
            existing.Currency      = account.Currency;
            existing.Remarks       = account.Remarks;
            existing.Status        = account.Status;
            existing.UpdatedBy     = updatedBy;
            existing.UpdatedAt     = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _audit.LogUpdateAsync("company_bank_accounts", existing.Id.ToString(), old, existing, updatedBy);
        });

    public async Task UpdateStatusAsync(int id, string status, string updatedBy)
        => await ExecuteAsync(async () =>
        {
            var existing = await _db.CompanyBankAccounts.FindAsync(id);
            if (existing == null) return;
            existing.Status    = status;
            existing.UpdatedBy = updatedBy;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _audit.LogActionAsync("company_bank_accounts", id.ToString(), status, updatedBy);
        });

    public async Task DeleteAsync(int id, string deletedBy)
        => await UpdateStatusAsync(id, StatusConstants.Deleted, deletedBy);
}
