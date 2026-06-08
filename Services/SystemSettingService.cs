using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyApp.Data;

namespace MyApp.Services;

public class SystemSettingService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "system_settings";

    public SystemSettingService(AppDbContext db, IMemoryCache cache)
    {
        _db    = db;
        _cache = cache;
    }

    private async Task<Dictionary<string, string>> GetAllCachedAsync()
    {
        if (!_cache.TryGetValue(CacheKey, out Dictionary<string, string>? settings))
        {
            settings = await _db.SystemSettings
                .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
            _cache.Set(CacheKey, settings, TimeSpan.FromHours(1));
        }
        return settings!;
    }

    public async Task<string> GetAsync(string key, string defaultValue = "")
    {
        var all = await GetAllCachedAsync();
        return all.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public async Task<int> GetAsIntAsync(string key, int defaultValue = 0)
    {
        var value = await GetAsync(key);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task<decimal> GetAsDecimalAsync(string key, decimal defaultValue = 0)
    {
        var value = await GetAsync(key);
        return decimal.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task<bool> GetAsBoolAsync(string key, bool defaultValue = false)
    {
        var value = await GetAsync(key);
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value == "1"
            || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    public void ClearCache() => _cache.Remove(CacheKey);
}
