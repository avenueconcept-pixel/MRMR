using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Languages;

public class CreateModel : AdminPageModel
{
  private readonly LanguageDbHelper   _languageDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtLanguageCode { get; set; } = string.Empty;
  [BindProperty] public string txtLanguageName { get; set; } = string.Empty;
  [BindProperty] public string txtNativeName   { get; set; } = string.Empty;
  [BindProperty] public int    txtSortOrder    { get; set; }
  [BindProperty] public string ddlStatus       { get; set; } = StatusConstants.Active;

  public List<SelectListItem> StatusOptions { get; set; } = new();

  public CreateModel(LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    StatusOptions    = await SelectListHelper.GetStatusOptions(_translation);
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    if (string.IsNullOrWhiteSpace(txtLanguageCode) ||
        string.IsNullOrWhiteSpace(txtLanguageName) ||
        string.IsNullOrWhiteSpace(txtNativeName))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var language = new Language
    {
      LanguageCode = txtLanguageCode.Trim(),
      LanguageName = txtLanguageName.Trim(),
      NativeName   = txtNativeName.Trim(),
      SortOrder    = txtSortOrder,
      Status       = ddlStatus
    };

    await _languageDbHelper.CreateAsync(language, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminLanguage);
  }
}
