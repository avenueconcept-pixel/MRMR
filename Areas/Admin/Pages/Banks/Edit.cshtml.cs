using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Banks;

public class EditModel : AdminPageModel
{
  private readonly BankDbHelper     _bankDbHelper;
  private readonly CountryDbHelper  _countryDbHelper;
  private readonly LanguageDbHelper _languageDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string ddlCountryCode { get; set; } = string.Empty;
  [BindProperty] public string txtSwiftCode   { get; set; } = string.Empty;
  [BindProperty] public string txtLocalCode   { get; set; } = string.Empty;
  [BindProperty] public string txtWebsite     { get; set; } = string.Empty;
  [BindProperty] public string txtLogo        { get; set; } = string.Empty;
  [BindProperty] public string ddlStatus      { get; set; } = StatusConstants.Active;

  public string                    BankCode          { get; set; } = string.Empty;
  public string                    CreatedBy         { get; set; } = string.Empty;
  public DateTime                  CreatedAt         { get; set; }
  public string                    UpdatedBy         { get; set; } = string.Empty;
  public DateTime                  UpdatedAt         { get; set; }
  public List<SelectListItem>      CountryOptions    { get; set; } = new();
  public List<TranslationInputDto> TranslationInputs { get; set; } = new();
  public List<TranslationInputDto> ShortNameInputs   { get; set; } = new();
  public List<SelectListItem>      StatusOptions     { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(BankDbHelper bankDbHelper, CountryDbHelper countryDbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _bankDbHelper     = bankDbHelper;
    _countryDbHelper  = countryDbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task<IActionResult> OnGetAsync(string bankCode)
  {
    AlertMessageType = "";

    var bank = await _bankDbHelper.GetByCodeAsync(bankCode);
    if (bank == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminBank);
    }

    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;

    BankCode          = bank.BankCode;
    ddlCountryCode    = bank.CountryCode;
    txtSwiftCode      = bank.SwiftCode   ?? string.Empty;
    txtLocalCode      = bank.LocalCode   ?? string.Empty;
    txtWebsite        = bank.Website     ?? string.Empty;
    txtLogo           = bank.Logo        ?? string.Empty;
    ddlStatus         = bank.Status;
    CreatedBy         = bank.CreatedBy;
    CreatedAt         = bank.CreatedAt;
    UpdatedBy         = bank.UpdatedBy;
    UpdatedAt         = bank.UpdatedAt;

    CountryOptions    = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(bank.Translations.ToList());
    ShortNameInputs   = await BuildShortNameInputsAsync(bank.Translations.ToList());

    var entityName        = bank.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.BankName ?? bankCode;
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string bankCode)
  {
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    BankCode          = bankCode;
    CountryOptions    = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(null);
    ShortNameInputs   = await BuildShortNameInputsAsync(null);

    var bank = new Bank
    {
      BankCode    = bankCode,
      CountryCode = ddlCountryCode,
      SwiftCode   = string.IsNullOrWhiteSpace(txtSwiftCode) ? null : txtSwiftCode.Trim(),
      LocalCode   = string.IsNullOrWhiteSpace(txtLocalCode) ? null : txtLocalCode.Trim(),
      Website     = string.IsNullOrWhiteSpace(txtWebsite)   ? null : txtWebsite.Trim(),
      Logo        = string.IsNullOrWhiteSpace(txtLogo)       ? null : txtLogo.Trim(),
      Status      = ddlStatus
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new BankTranslation
        {
          BankCode     = bankCode,
          LanguageCode = l.LanguageCode,
          BankName     = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim(),
          ShortName    = Request.Form[$"txtShortName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.BankName))
        .ToList();

    await _bankDbHelper.UpdateAsync(bank, translations, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminBank);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(string bankCode)
  {
    try
    {
      await _bankDbHelper.UpdateStatusAsync(bankCode, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task<List<TranslationInputDto>> BuildInputsAsync(IList<BankTranslation>? existing)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("Bank.NamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = l.LanguageName,
      Value        = existing != null
          ? existing.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.BankName ?? string.Empty
          : (Request.HasFormContentType ? Request.Form[$"txtName_{l.LanguageCode}"].ToString() : string.Empty),
      Placeholder  = placeholder
    }).ToList();
  }

  private async Task<List<TranslationInputDto>> BuildShortNameInputsAsync(IList<BankTranslation>? existing)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("Bank.ShortNamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = l.LanguageName,
      Value        = existing != null
          ? existing.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.ShortName ?? string.Empty
          : (Request.HasFormContentType ? Request.Form[$"txtShortName_{l.LanguageCode}"].ToString() : string.Empty),
      Placeholder  = placeholder
    }).ToList();
  }
}
