using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.ProductSectionTypes;

public class CreateModel : AdminPageModel
{
  private readonly ProductSectionTypeDbHelper _dbHelper;
  private readonly LanguageDbHelper           _languageDbHelper;
  private readonly TranslationService         _translation;

  [BindProperty] public string txtSectionCode { get; set; } = string.Empty;
  [BindProperty] public int    txtSortOrder   { get; set; } = 0;
  [BindProperty] public string ddlStatus      { get; set; } = StatusConstants.Active;

  public List<TranslationInputDto> TranslationInputs { get; set; } = new();
  public List<SelectListItem>      StatusOptions     { get; set; } = new();

  public CreateModel(ProductSectionTypeDbHelper dbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _dbHelper         = dbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType  = "";
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    ddlStatus         = StatusConstants.Active;
    txtSortOrder      = 0;
    TranslationInputs = await BuildInputsAsync(fromForm: false);
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(fromForm: true);

    if (string.IsNullOrWhiteSpace(txtSectionCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    var code = txtSectionCode.Trim().ToUpper();

    var section = new ProductSectionType
    {
      SectionCode = code,
      SortOrder   = txtSortOrder,
      Status      = ddlStatus
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new ProductSectionTypeTranslation
        {
          SectionCode  = code,
          LanguageCode = l.LanguageCode,
          SectionName  = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.SectionName))
        .ToList();

    var result = await _dbHelper.AddAsync(section, translations, CurrentUsername);

    if (result == ProductSectionTypeAddResult.DuplicateActive)
    {
      SetError(await _translation.GetAsync(MessageConstants.DuplicateError));
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(
        result == ProductSectionTypeAddResult.Restored ? MessageConstants.RestoreSuccess : MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminProductSectionType);
  }

  private async Task<List<TranslationInputDto>> BuildInputsAsync(bool fromForm)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("ProductSectionType.NamePlaceholder");
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
