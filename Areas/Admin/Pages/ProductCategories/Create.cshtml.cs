using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.ProductCategories;

public class CreateModel : AdminPageModel
{
  private readonly ProductCategoryDbHelper _dbHelper;
  private readonly LanguageDbHelper        _languageDbHelper;
  private readonly TranslationService      _translation;

  [BindProperty] public string txtCategoryCode { get; set; } = string.Empty;
  [BindProperty] public string ddlStatus       { get; set; } = StatusConstants.Active;

  public List<TranslationInputDto> TranslationInputs { get; set; } = new();
  public List<SelectListItem>      StatusOptions     { get; set; } = new();

  public CreateModel(ProductCategoryDbHelper dbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _dbHelper         = dbHelper;
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

    if (string.IsNullOrWhiteSpace(txtCategoryCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    var code = txtCategoryCode.Trim().ToUpper();

    var category = new ProductCategory
    {
      CategoryCode = code,
      Status       = ddlStatus
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new ProductCategoryTranslation
        {
          CategoryCode = code,
          LanguageCode = l.LanguageCode,
          CategoryName = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.CategoryName))
        .ToList();

    var result = await _dbHelper.AddAsync(category, translations, CurrentUsername);

    if (result == ProductCategoryAddResult.DuplicateActive)
    {
      SetError(await _translation.GetAsync(MessageConstants.DuplicateError));
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(
        result == ProductCategoryAddResult.Restored ? MessageConstants.RestoreSuccess : MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminProductCategory);
  }

  private async Task<List<TranslationInputDto>> BuildInputsAsync(bool fromForm)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("ProductCategory.NamePlaceholder");
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
