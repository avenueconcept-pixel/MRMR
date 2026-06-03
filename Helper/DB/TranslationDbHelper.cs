using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class TranslationDbHelper : DbHelper
{
  public TranslationDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<List<LanguageResource>> GetAllAsync()
      => await ExecuteAsync(async () => await _db.LanguageResources
          .OrderBy(r => r.Key)
          .ThenBy(r => r.LanguageCode)
          .ToListAsync());

  public async Task<List<string>> GetDistinctKeysAsync()
      => await ExecuteAsync(async () => await _db.LanguageResources
          .Select(r => r.Key)
          .Distinct()
          .OrderBy(k => k)
          .ToListAsync());

  public async Task<List<string>> GetActiveLanguageCodesAsync()
      => await ExecuteAsync(async () => await _db.LanguageResources
          .Select(r => r.LanguageCode)
          .Distinct()
          .OrderBy(c => c)
          .ToListAsync());

  public async Task<bool> KeyExistsAsync(string key)
      => await ExecuteAsync(async () => await _db.LanguageResources.AnyAsync(r => r.Key == key));

  public async Task UpsertAsync(string key, string languageCode, string value, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.LanguageResources
            .FirstOrDefaultAsync(r => r.Key == key && r.LanguageCode == languageCode);
        if (existing != null)
        {
          existing.Value = value;
        }
        else
        {
          _db.LanguageResources.Add(new LanguageResource
          {
            Key          = key,
            LanguageCode = languageCode,
            Value        = value
          });
        }
        await _db.SaveChangesAsync();
      });

  public async Task UpsertBatchAsync(List<TranslationRowDto> rows, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        foreach (var row in rows)
        {
          if (string.IsNullOrWhiteSpace(row.Value)) continue;
          var existing = await _db.LanguageResources
              .FirstOrDefaultAsync(r => r.Key == row.Key && r.LanguageCode == row.LanguageCode);
          if (existing != null)
          {
            existing.Value = row.Value;
          }
          else
          {
            _db.LanguageResources.Add(new LanguageResource
            {
              Key          = row.Key,
              LanguageCode = row.LanguageCode,
              Value        = row.Value
            });
          }
        }
        await _db.SaveChangesAsync();
      });

  public async Task DeleteByKeyAsync(string key, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var rows = await _db.LanguageResources
            .Where(r => r.Key == key)
            .ToListAsync();
        _db.LanguageResources.RemoveRange(rows);
        await _db.SaveChangesAsync();
      });
}
