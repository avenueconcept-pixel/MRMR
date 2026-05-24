using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Countries;

public class EditModel : AdminPageModel
{
  private readonly CountryDbHelper  _countryDbHelper;
  private readonly LanguageDbHelper _languageDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string? txtCurrencyCode { get; set; }
  [BindProperty] public string  ddlStatus       { get; set; } = StatusConstants.Active;
  [BindProperty] public string  ddlTimezone     { get; set; } = string.Empty;

  public string                    CountryCode            { get; set; } = string.Empty;
  public string                    CreatedBy              { get; set; } = string.Empty;
  public DateTime                  CreatedAt              { get; set; }
  public string                    UpdatedBy              { get; set; } = string.Empty;
  public DateTime                  UpdatedAt              { get; set; }
  public List<TranslationInputDto> TranslationInputs      { get; set; } = new();
  public List<SelectListItem>      StatusOptions          { get; set; } = new();
  public List<SelectListItem>      TimezoneOptions        { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(CountryDbHelper countryDbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _countryDbHelper  = countryDbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task<IActionResult> OnGetAsync(string countryCode)
  {
    AlertMessageType = "";

    var country = await _countryDbHelper.GetByCodeAsync(countryCode);
    if (country == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminCountry);
    }

    CountryCode     = country.CountryCode;
    txtCurrencyCode = country.CurrencyCode;
    ddlStatus       = country.Status;
    ddlTimezone     = country.Timezone;
    CreatedBy       = country.CreatedBy;
    CreatedAt       = country.CreatedAt;
    UpdatedBy       = country.UpdatedBy;
    UpdatedAt       = country.UpdatedAt;
    StatusOptions   = await SelectListHelper.GetStatusOptions(_translation);
    TimezoneOptions = SelectListHelper.GetTimezoneOptions();
    TranslationInputs = await BuildInputsAsync(country.Translations.ToList());

    var entityName          = CountryCode;
    MsgDeleteConfirmTitle   = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText    = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn     = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn            = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess        = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError          = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete             = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string countryCode)
  {
    CountryCode     = countryCode;
    StatusOptions   = await SelectListHelper.GetStatusOptions(_translation);
    TimezoneOptions = SelectListHelper.GetTimezoneOptions();
    TranslationInputs = await BuildInputsAsync(null);

    if (string.IsNullOrEmpty(ddlTimezone))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var country = new Country
    {
      CountryCode  = countryCode,
      CurrencyCode = txtCurrencyCode?.Trim().ToUpper() ?? string.Empty,
      Status       = ddlStatus,
      Timezone     = ddlTimezone
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new CountryTranslation
        {
          CountryCode  = countryCode,
          LanguageCode = l.LanguageCode,
          CountryName  = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.CountryName))
        .ToList();

    await _countryDbHelper.UpdateAsync(country, translations, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminCountry);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(string countryCode)
  {
    try
    {
      await _countryDbHelper.UpdateStatusAsync(countryCode, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  // existingTranslations = null means read from Request.Form (POST error case)
  private async Task<List<TranslationInputDto>> BuildInputsAsync(IList<CountryTranslation>? existingTranslations)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("Country.NamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = $"{l.LanguageName}",
      Value        = existingTranslations != null
          ? existingTranslations.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.CountryName ?? string.Empty
          : Request.Form[$"txtName_{l.LanguageCode}"].ToString(),
      Placeholder  = placeholder
    }).ToList();
  }
}
