using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.States;

public class CreateModel : AdminPageModel
{
  private readonly StateDbHelper   _stateDbHelper;
  private readonly CountryDbHelper _countryDbHelper;
  private readonly LanguageDbHelper    _languageDbHelper;
  private readonly TranslationService  _translation;

  [BindProperty] public string txtStateCode   { get; set; } = string.Empty;
  [BindProperty] public string txtSortOrder   { get; set; } = "0";
  [BindProperty] public string ddlCountryCode { get; set; } = string.Empty;
  [BindProperty] public string ddlStatus      { get; set; } = StatusConstants.Active;

  public string                    CountryName       { get; set; } = string.Empty;
  public List<TranslationInputDto> TranslationInputs { get; set; } = new();
  public List<SelectListItem>      StatusOptions     { get; set; } = new();

  public CreateModel(StateDbHelper stateDbHelper, CountryDbHelper countryDbHelper,
      LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _stateDbHelper    = stateDbHelper;
    _countryDbHelper  = countryDbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync(string countryCode)
  {
    AlertMessageType = "";
    ddlCountryCode   = countryCode;
    txtSortOrder     = "0";

    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    var country  = await _countryDbHelper.GetByCodeAsync(countryCode);
    CountryName  = country?.Translations.FirstOrDefault(t => t.LanguageCode == langCode)?.CountryName
                ?? country?.Translations.FirstOrDefault()?.CountryName
                ?? countryCode;

    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(fromForm: false);
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    var country  = await _countryDbHelper.GetByCodeAsync(ddlCountryCode);
    CountryName  = country?.Translations.FirstOrDefault(t => t.LanguageCode == langCode)?.CountryName
                ?? country?.Translations.FirstOrDefault()?.CountryName
                ?? ddlCountryCode;

    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(fromForm: true);

    if (string.IsNullOrWhiteSpace(txtStateCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    if (string.IsNullOrWhiteSpace(ddlCountryCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    int.TryParse(txtSortOrder, out var sortOrder);
    var code = txtStateCode.Trim().ToUpper();

    var state = new State
    {
      CountryCode = ddlCountryCode,
      StateCode   = code,
      SortOrder   = sortOrder,
      Status      = ddlStatus
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new StateTranslation
        {
          LanguageCode = l.LanguageCode,
          StateName    = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.StateName))
        .ToList();

    var result = await _stateDbHelper.AddAsync(state, translations, CurrentUsername);

    if (result == StateAddResult.DuplicateActive)
    {
      SetError(await _translation.GetAsync(MessageConstants.DuplicateError));
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(
        result == StateAddResult.Restored ? MessageConstants.RestoreSuccess : MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminState);
  }

  private async Task<List<TranslationInputDto>> BuildInputsAsync(bool fromForm)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("State.NamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = l.LanguageName,
      Value        = fromForm ? Request.Form[$"txtName_{l.LanguageCode}"].ToString() : string.Empty,
      Placeholder  = placeholder
    }).ToList();
  }

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
