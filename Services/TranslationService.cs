// Services/TranslationService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyApp.Data;

namespace MyApp.Services;

public class TranslationService
{
  private readonly AppDbContext _db;
  private readonly IMemoryCache _cache;
  private readonly IHttpContextAccessor _httpContext;

  public TranslationService(
      AppDbContext db,
      IMemoryCache cache,
      IHttpContextAccessor httpContext)
  {
    _db = db;
    _cache = cache;
    _httpContext = httpContext;
  }

  // ─── Current language from cookie ─────────────────
  public string CurrentLanguage
  {
    get
    {
      var cookie = _httpContext.HttpContext?.Request.Cookies["lang"];
      return string.IsNullOrEmpty(cookie) ? "en" : cookie;
    }
  }

  // ─── Load all from DB into cache ──────────────────
  private async Task<Dictionary<string, Dictionary<string, string>>> GetAllAsync()
  {
    const string cacheKey = "all_translations";

    if (!_cache.TryGetValue(cacheKey, out Dictionary<string, Dictionary<string, string>>? translations))
    {
      translations = await _db.LanguageResources
          .GroupBy(r => r.LanguageCode)
          .ToDictionaryAsync(
              g => g.Key,
              g => g.ToDictionary(r => r.Key, r => r.Value)
          );

      _cache.Set(cacheKey, translations, TimeSpan.FromHours(1));
    }

    return translations!;
  }

  // ─── Get single translation ───────────────────────
  public async Task<string> GetAsync(string key, string? languageCode = null)
  {
    var lang = languageCode ?? CurrentLanguage;
    var all = await GetAllAsync();

    // Try requested language first
    if (all.TryGetValue(lang, out var langDict) &&
        langDict.TryGetValue(key, out var value))
      return value;

    // Fallback to English
    if (all.TryGetValue("en", out var enDict) &&
        enDict.TryGetValue(key, out var enValue))
      return enValue;

    // Last resort — return key itself
    return key;
  }

  // ─── Clear cache after admin updates translations ─
  public void ClearCache()
      => _cache.Remove("all_translations");
}
