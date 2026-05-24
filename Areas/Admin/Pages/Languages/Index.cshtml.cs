using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Languages;

public class IndexModel : AdminPageModel
{
  private readonly LanguageDbHelper  _languageDbHelper;
  private readonly TranslationService _translation;

  public List<Language> Languages { get; set; } = new();

  public IndexModel(LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    Languages = await _languageDbHelper.GetAllAsync();
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] int id)
  {
    var language = await _languageDbHelper.GetByIdAsync(id);
    if (language == null)
      return new JsonResult(new { success = false });

    var newStatus = language.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _languageDbHelper.UpdateStatusAsync(id, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
