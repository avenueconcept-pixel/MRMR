using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class ExchangeRateDbHelper : DbHelper
{
  public ExchangeRateDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<ExchangeRate?> GetCurrentRateAsync(string currencyCode)
      => await ExecuteAsync(async () =>
          await _db.ExchangeRates
              .Where(r => r.CurrencyCode == currencyCode && r.EffectiveDatetime <= DateTime.UtcNow)
              .OrderByDescending(r => r.EffectiveDatetime)
              .FirstOrDefaultAsync());

  public async Task<List<ExchangeRate>> GetLatestAllAsync()
      => await ExecuteAsync(async () =>
      {
        var now = DateTime.UtcNow;
        var currencies = await _db.ExchangeRates
            .Select(r => r.CurrencyCode)
            .Distinct()
            .ToListAsync();

        var result = new List<ExchangeRate>();
        foreach (var code in currencies)
        {
          var latest = await _db.ExchangeRates
              .Where(r => r.CurrencyCode == code && r.EffectiveDatetime <= now)
              .OrderByDescending(r => r.EffectiveDatetime)
              .FirstOrDefaultAsync();
          if (latest != null) result.Add(latest);
        }
        return result.OrderBy(r => r.CurrencyCode).ToList();
      });

  public async Task<(List<ExchangeRate> Items, int Total)> GetHistoryPagedAsync(
      string currencyCode, int page, int pageSize)
      => await ExecuteAsync(async () =>
      {
        var q = _db.ExchangeRates.AsQueryable();
        if (!string.IsNullOrEmpty(currencyCode))
          q = q.Where(r => r.CurrencyCode == currencyCode);
        q = q.OrderByDescending(r => r.EffectiveDatetime);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
      });

  public async Task AddAsync(ExchangeRate rate)
      => await ExecuteAsync(async () =>
      {
        rate.CreatedAt = DateTime.UtcNow;
        if (rate.EffectiveDatetime == default)
          rate.EffectiveDatetime = DateTime.UtcNow;
        _db.ExchangeRates.Add(rate);
        await _db.SaveChangesAsync();
      });
}
