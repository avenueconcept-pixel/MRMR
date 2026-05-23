using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Services;

namespace MyApp.Helper;

public static class SelectListHelper
{
  public static async Task<List<SelectListItem>> GetStatusOptions(TranslationService translationService)
      => new()
      {
        new() { Value = UserStatusConstants.Active,   Text = await translationService.GetAsync("status.active") },
        new() { Value = UserStatusConstants.Inactive, Text = await translationService.GetAsync("status.inactive") }
      };
}
