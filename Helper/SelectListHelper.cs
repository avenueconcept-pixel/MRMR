using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
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
}
