using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyApp.Data;

namespace MyApp.Services;

public class AppSettingsService
{
  private readonly AppDbContext _db;
  private readonly IMemoryCache _cache;
  private const string CacheKey = "AppSettings";

  public AppSettingsService(AppDbContext db, IMemoryCache cache)
  {
    _db    = db;
    _cache = cache;
  }

  public async Task<string> GetAsync(string systemCode, string key)
  {
    var all = await GetAllAsync();
    return all.TryGetValue($"{systemCode}:{key}", out var val) ? val : string.Empty;
  }

  public async Task<Dictionary<string, string>> GetAllAsync()
      => await _cache.GetOrCreateAsync(CacheKey, async entry =>
      {
        entry.SlidingExpiration = TimeSpan.FromHours(1);
        var settings = await _db.AppSettings.ToListAsync();
        return settings.ToDictionary(s => $"{s.SystemCode}:{s.SettingKey}", s => s.SettingValue);
      }) ?? new();

  public void InvalidateCache() => _cache.Remove(CacheKey);
}
