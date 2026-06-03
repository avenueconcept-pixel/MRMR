using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.UnitsOfMeasure;

public class CreateModel : AdminPageModel
{
  private readonly UomDbHelper      _uomDbHelper;
  private readonly LanguageDbHelper _languageDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtUomCode { get; set; } = string.Empty;
  [BindProperty] public string txtUomName { get; set; } = string.Empty;
  [BindProperty] public string ddlStatus  { get; set; } = StatusConstants.Active;

  public List<TranslationInputDto> TranslationInputs { get; set; } = new();
  public List<SelectListItem>      StatusOptions     { get; set; } = new();

  public CreateModel(UomDbHelper uomDbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _uomDbHelper      = uomDbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType  = "";
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(fromForm: false);
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(fromForm: true);

    if (string.IsNullOrWhiteSpace(txtUomCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    var code = txtUomCode.Trim().ToUpper();

    var uom = new UnitOfMeasure
    {
      UomCode = code,
      UomName = txtUomName.Trim(),
      Status  = ddlStatus
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new UomTranslation
        {
          UomCode      = code,
          LanguageCode = l.LanguageCode,
          UomName      = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.UomName))
        .ToList();

    var result = await _uomDbHelper.AddAsync(uom, translations, CurrentUsername);

    if (result == UomAddResult.DuplicateActive)
    {
      SetError(await _translation.GetAsync(MessageConstants.DuplicateError));
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(
        result == UomAddResult.Restored ? MessageConstants.RestoreSuccess : MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminUnitOfMeasure);
  }

  private async Task<List<TranslationInputDto>> BuildInputsAsync(bool fromForm)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("UnitOfMeasure.NamePlaceholder");
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
