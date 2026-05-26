using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper.DB;
using MyApp.Models;
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

  public static async Task<List<SelectListItem>> GetMenuLevelOptions(TranslationService translationService)
      => new()
      {
        new() { Value = "0", Text = await translationService.GetAsync("Menu.Level.Group") },
        new() { Value = "1", Text = await translationService.GetAsync("Menu.Level.Parent") },
        new() { Value = "2", Text = await translationService.GetAsync("Menu.Level.Child") },
      };

  public static List<SelectListItem> GetParentMenuOptions(List<Menu> parents, string placeholder)
  {
    var items = new List<SelectListItem> { new() { Value = string.Empty, Text = placeholder } };
    items.AddRange(parents.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.MenuName }));
    return items;
  }

  public static List<SelectListItem> GetGroupMenuOptions(List<Menu> groups, string placeholder)
  {
    var items = new List<SelectListItem> { new() { Value = string.Empty, Text = placeholder } };
    items.AddRange(groups.Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.MenuName }));
    return items;
  }

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
