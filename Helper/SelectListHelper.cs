using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper.DB;
using MyApp.Services;
using TimeZoneConverter;

namespace MyApp.Helper;

public static class SelectListHelper
{
  public static async Task<List<SelectListItem>> GetStatusOptions(TranslationService translationService)
      => new()
      {
        new() { Value = StatusConstants.Active,   Text = await translationService.GetAsync("status.active") },
        new() { Value = StatusConstants.Inactive, Text = await translationService.GetAsync("status.inactive") },
      };

  public static List<SelectListItem> GetTimezoneOptions()
      => TimeZoneInfo.GetSystemTimeZones()
          .Select(tz => new SelectListItem
          {
            Value = TZConvert.WindowsToIana(tz.Id),
            Text  = tz.DisplayName
          })
          .DistinctBy(x => x.Value)
          .OrderBy(x => x.Text)
          .ToList();

  public static async Task<List<SelectListItem>> GetLocationTypeOptions(TranslationService translationService)
      => new()
      {
        new() { Value = "hq",        Text = await translationService.GetAsync("LocationType.hq") },
        new() { Value = "branch",    Text = await translationService.GetAsync("LocationType.branch") },
        new() { Value = "outlet",    Text = await translationService.GetAsync("LocationType.outlet") },
        new() { Value = "warehouse", Text = await translationService.GetAsync("LocationType.warehouse") }
      };

  public static async Task<List<SelectListItem>> GetCountryOptions(
      CountryDbHelper countryDb, string languageCode)
  {
    var countries = await countryDb.GetAllActiveAsync(languageCode);
    return countries
        .SelectMany(c => c.Translations.Select(t => new SelectListItem
        {
          Value = c.CountryCode,
          Text  = $"{t.CountryName} ({c.CountryCode})"
        }))
        .ToList();
  }

  public static async Task<List<SelectListItem>> GetDataScopeOptions(TranslationService translationService)
      => new()
      {
        new() { Value = "all",        Text = await translationService.GetAsync("DataScope.all") },
        new() { Value = "own",        Text = await translationService.GetAsync("DataScope.own") },
        new() { Value = "department", Text = await translationService.GetAsync("DataScope.department") },
        new() { Value = "location",   Text = await translationService.GetAsync("DataScope.location") },
      };

  public static async Task<List<SelectListItem>> GetPaymentMethodOptions(
      PaymentMethodDbHelper pmDb,
      string languageCode)
  {
    var methods = await pmDb.GetAllActiveAsync(languageCode);
    return methods
        .SelectMany(pm => pm.Translations)
        .Select(t => new SelectListItem
        {
          Value = t.PaymentCode,
          Text  = t.PaymentName
        })
        .ToList();
  }
}
