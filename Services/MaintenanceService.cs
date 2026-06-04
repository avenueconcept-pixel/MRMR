using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Dtos;

namespace MyApp.Services;

public class MaintenanceService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly IMemoryCache     _cache;

  private const string CacheKeyPrefix = "maintenance_";
  private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

  public MaintenanceService(IServiceProvider serviceProvider, IMemoryCache cache)
  {
    _serviceProvider = serviceProvider;
    _cache           = cache;
  }

  public async Task<MaintenanceStatusDto> GetStatusAsync(string systemCode, string languageCode)
  {
    var cacheKey = $"{CacheKeyPrefix}{systemCode}_{languageCode}";
    if (_cache.TryGetValue(cacheKey, out MaintenanceStatusDto? cached) && cached != null)
      return cached;

    using var scope = _serviceProvider.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var now = DateTime.UtcNow;

    var activeSchedule = await db.MaintenanceSchedules
        .Where(m => m.Status != "deleted"
                 && m.IsActive
                 && m.StartAt <= now
                 && m.EndAt   >= now
                 && m.Systems.Any(s => s.SystemCode == systemCode))
        .Include(m => m.Messages)
        .OrderByDescending(m => m.StartAt)
        .FirstOrDefaultAsync();

    var dto = new MaintenanceStatusDto
    {
      IsUnderMaintenance = activeSchedule != null,
      Message = activeSchedule?.Messages
                    .FirstOrDefault(msg => msg.LanguageCode == languageCode)?.Message
                ?? activeSchedule?.Messages
                    .FirstOrDefault(msg => msg.LanguageCode == "en")?.Message
                ?? string.Empty
    };

    _cache.Set(cacheKey, dto, CacheDuration);
    return dto;
  }

  public void InvalidateCache()
  {
    foreach (var lang in new[] { "en", "zh-Hans" })
    {
      foreach (var sys in new[] { "admin", "customer" })
      {
        _cache.Remove($"{CacheKeyPrefix}{sys}_{lang}");
      }
    }
  }
}
