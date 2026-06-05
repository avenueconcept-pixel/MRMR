using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Products;

public class EditModel : AdminPageModel
{
  private readonly ProductDbHelper         _productDbHelper;
  private readonly ProductCategoryDbHelper _categoryDbHelper;
  private readonly CountryDbHelper         _countryDbHelper;
  private readonly LanguageDbHelper        _languageDbHelper;
  private readonly UomDbHelper             _uomDbHelper;
  private readonly TranslationService      _translation;

  [BindProperty] public string  ddlProductType   { get; set; } = string.Empty;
  [BindProperty] public string  ddlProductNature { get; set; } = string.Empty;
  [BindProperty] public string  ddlUom           { get; set; } = string.Empty;
  [BindProperty] public decimal txtPv            { get; set; }
  [BindProperty] public int     txtSortOrder     { get; set; }
  [BindProperty] public string  ddlStatus        { get; set; } = StatusConstants.Active;

  public string CurrentProductCode { get; set; } = string.Empty;

  public List<string>                    SelectedCategoryCodes { get; set; } = new();
  public List<ProductTranslationInputDto> TranslationInputs    { get; set; } = new();
  public List<CountrySelectionDto>        CountrySelections    { get; set; } = new();
  public List<SelectListItem>             CategoriesSelectList { get; set; } = new();
  public List<SelectListItem>             UomOptions           { get; set; } = new();
  public List<SelectListItem>             ProductTypeOptions   { get; set; } = new();
  public List<SelectListItem>             ProductNatureOptions { get; set; } = new();
  public List<SelectListItem>             StatusOptions        { get; set; } = new();
  public List<SelectListItem>             StockStatusOptions   { get; set; } = new();

  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(
      ProductDbHelper         productDbHelper,
      ProductCategoryDbHelper categoryDbHelper,
      CountryDbHelper         countryDbHelper,
      LanguageDbHelper        languageDbHelper,
      UomDbHelper             uomDbHelper,
      TranslationService      translation)
  {
    _productDbHelper  = productDbHelper;
    _categoryDbHelper = categoryDbHelper;
    _countryDbHelper  = countryDbHelper;
    _languageDbHelper = languageDbHelper;
    _uomDbHelper      = uomDbHelper;
    _translation      = translation;
  }

  public async Task<IActionResult> OnGetAsync(string productCode)
  {
    var product = await _productDbHelper.GetByCodeAsync(productCode);
    if (product == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminProducts);
    }

    CurrentProductCode    = product.ProductCode;
    ddlProductType        = product.ProductType;
    ddlProductNature      = product.ProductNature;
    ddlUom                = product.UomCode;
    txtPv                 = product.Pv;
    txtSortOrder          = product.SortOrder;
    ddlStatus             = product.Status;
    CreatedBy             = product.CreatedBy;
    CreatedAt             = product.CreatedAt;
    UpdatedBy             = product.UpdatedBy;
    UpdatedAt             = product.UpdatedAt;
    SelectedCategoryCodes = product.CategoryMaps.Select(m => m.CategoryCode).ToList();

    await PopulateDropdownsAsync();
    TranslationInputs = await BuildTranslationInputsAsync(product.Translations.ToList());
    CountrySelections = await BuildCountrySelectionsAsync(product.Countries.ToList());

    var entityName        = product.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.ProductName ?? productCode;
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string productCode)
  {
    CurrentProductCode    = productCode;
    SelectedCategoryCodes = Request.Form["selectedCategories"]
        .Where(x => !string.IsNullOrEmpty(x)).Select(x => x!).ToList();
    await PopulateDropdownsAsync();
    TranslationInputs = await BuildTranslationInputsAsync(null);
    CountrySelections = await BuildCountrySelectionsAsync(null);

    var product = new Product
    {
      ProductCode   = productCode,
      ProductType   = ddlProductType,
      ProductNature = ddlProductNature,
      UomCode       = ddlUom,
      Pv            = txtPv,
      SortOrder     = txtSortOrder,
      Status        = ddlStatus
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages.Select(l => new ProductTranslation
    {
      LanguageCode     = l.LanguageCode,
      ProductName      = Request.Form[$"txtProductName_{l.LanguageCode}"].ToString().Trim(),
      ShortDescription = Request.Form[$"txtShortDesc_{l.LanguageCode}"].ToString().Trim()
    }).ToList();

    var categoryCodes = Request.Form["selectedCategories"]
        .Where(x => !string.IsNullOrEmpty(x)).Select(x => x!).ToList();

    var allCountries = await _countryDbHelper.GetAllActiveAsync("en");
    var countries = allCountries
        .Where(c => Request.Form.ContainsKey($"countryCheck_{c.CountryCode}"))
        .Select(c => new ProductCountry
        {
          CountryCode = c.CountryCode,
          IsEnabled   = true,
          StockStatus = Request.Form[$"countryStock_{c.CountryCode}"].FirstOrDefault() ?? StockStatusConstants.Available
        })
        .ToList();

    await _productDbHelper.UpdateAsync(product, translations, categoryCodes, countries, new List<ProductPriceTier>(), CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminProductsEdit, new { productCode });
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(string productCode)
  {
    try
    {
      await _productDbHelper.UpdateStatusAsync(productCode, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task PopulateDropdownsAsync()
  {
    var langCode   = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    var uoms       = await _uomDbHelper.GetAllActiveAsync(langCode);
    var categories = await _categoryDbHelper.GetAllActiveAsync(langCode);

    UomOptions = new List<SelectListItem> { new() { Value = string.Empty, Text = await _translation.GetAsync("Products.SelectUom") } };
    UomOptions.AddRange(uoms.Select(u => new SelectListItem
    {
      Value = u.UomCode,
      Text  = u.Translations.FirstOrDefault()?.UomName ?? u.UomCode
    }));

    CategoriesSelectList = categories.Select(c => new SelectListItem
    {
      Value    = c.CategoryCode,
      Text     = c.Translations.FirstOrDefault()?.CategoryName ?? c.CategoryCode,
      Selected = SelectedCategoryCodes.Contains(c.CategoryCode)
    }).ToList();

    ProductTypeOptions = new List<SelectListItem>
    {
      new() { Value = ProductTypeConstants.Standard, Text = await _translation.GetAsync("Products.Type.Standard") },
      new() { Value = ProductTypeConstants.Package,  Text = await _translation.GetAsync("Products.Type.Package") }
    };

    ProductNatureOptions = new List<SelectListItem>
    {
      new() { Value = ProductNatureConstants.Physical, Text = await _translation.GetAsync("Products.Nature.Physical") },
      new() { Value = ProductNatureConstants.Digital,  Text = await _translation.GetAsync("Products.Nature.Digital") }
    };

    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    StockStatusOptions = new List<SelectListItem>
    {
      new() { Value = StockStatusConstants.Available,    Text = await _translation.GetAsync("Products.StockStatus.Available") },
      new() { Value = StockStatusConstants.LimitedStock, Text = await _translation.GetAsync("Products.StockStatus.LimitedStock") },
      new() { Value = StockStatusConstants.SoldOut,      Text = await _translation.GetAsync("Products.StockStatus.SoldOut") }
    };
  }

  private async Task<List<ProductTranslationInputDto>> BuildTranslationInputsAsync(IList<ProductTranslation>? existing)
  {
    var languages = await _languageDbHelper.GetAllActiveAsync();
    return languages.Select(l =>
    {
      var t = existing?.FirstOrDefault(x => x.LanguageCode == l.LanguageCode);
      return new ProductTranslationInputDto
      {
        LanguageCode     = l.LanguageCode,
        LanguageName     = l.LanguageName,
        ProductName      = existing != null
            ? t?.ProductName ?? string.Empty
            : Request.Form[$"txtProductName_{l.LanguageCode}"].ToString(),
        ShortDescription = existing != null
            ? t?.ShortDescription ?? string.Empty
            : Request.Form[$"txtShortDesc_{l.LanguageCode}"].ToString()
      };
    }).ToList();
  }

  private async Task<List<CountrySelectionDto>> BuildCountrySelectionsAsync(IList<ProductCountry>? existing)
  {
    var langCode  = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    var countries = await _countryDbHelper.GetAllActiveAsync(langCode);
    return countries.Select(c =>
    {
      if (existing != null)
      {
        var pc = existing.FirstOrDefault(x => x.CountryCode == c.CountryCode);
        return new CountrySelectionDto
        {
          CountryCode = c.CountryCode,
          CountryName = c.Translations.FirstOrDefault()?.CountryName ?? c.CountryCode,
          IsSelected  = pc != null,
          StockStatus = pc?.StockStatus ?? StockStatusConstants.Available
        };
      }
      return new CountrySelectionDto
      {
        CountryCode = c.CountryCode,
        CountryName = c.Translations.FirstOrDefault()?.CountryName ?? c.CountryCode,
        IsSelected  = Request.Form.ContainsKey($"countryCheck_{c.CountryCode}"),
        StockStatus = Request.Form[$"countryStock_{c.CountryCode}"].FirstOrDefault() ?? StockStatusConstants.Available
      };
    }).ToList();
  }
}
