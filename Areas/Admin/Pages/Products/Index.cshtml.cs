using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Products;

public class IndexModel : AdminPageModel
{
  private readonly ProductDbHelper         _productDbHelper;
  private readonly ProductCategoryDbHelper _categoryDbHelper;
  private readonly CountryDbHelper         _countryDbHelper;
  private readonly TranslationService      _translation;

  [BindProperty(SupportsGet = true)] public string? FilterStatus       { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterType         { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterCategoryCode { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterCountryCode  { get; set; }

  public List<Product>        Items       { get; set; } = new();
  public List<SelectListItem> ddlStatus   { get; set; } = new();
  public List<SelectListItem> ddlType     { get; set; } = new();
  public List<SelectListItem> ddlCategory { get; set; } = new();
  public List<SelectListItem> ddlCountry  { get; set; } = new();

  public IndexModel(
      ProductDbHelper         productDbHelper,
      ProductCategoryDbHelper categoryDbHelper,
      CountryDbHelper         countryDbHelper,
      TranslationService      translation)
  {
    _productDbHelper  = productDbHelper;
    _categoryDbHelper = categoryDbHelper;
    _countryDbHelper  = countryDbHelper;
    _translation      = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";

    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;

    Items = await _productDbHelper.GetAllAsync(langCode, FilterStatus, FilterType, FilterCategoryCode, FilterCountryCode);

    var allLabel   = await _translation.GetAsync("Filter.All");
    var categories = await _categoryDbHelper.GetAllActiveAsync(langCode);
    var countries  = await _countryDbHelper.GetAllActiveAsync(langCode);

    ddlStatus = new List<SelectListItem>
    {
      new() { Value = string.Empty,              Text = allLabel },
      new() { Value = StatusConstants.Active,   Text = await _translation.GetAsync("status.active") },
      new() { Value = StatusConstants.Inactive, Text = await _translation.GetAsync("status.inactive") }
    };

    ddlType = new List<SelectListItem>
    {
      new() { Value = string.Empty,                   Text = allLabel },
      new() { Value = ProductTypeConstants.Standard, Text = await _translation.GetAsync("Products.Type.Standard") },
      new() { Value = ProductTypeConstants.Package,  Text = await _translation.GetAsync("Products.Type.Package") }
    };

    ddlCategory = new List<SelectListItem> { new() { Value = string.Empty, Text = allLabel } };
    ddlCategory.AddRange(categories.Select(c => new SelectListItem
    {
      Value = c.CategoryCode,
      Text  = c.Translations.FirstOrDefault()?.CategoryName ?? c.CategoryCode
    }));

    ddlCountry = new List<SelectListItem> { new() { Value = string.Empty, Text = allLabel } };
    ddlCountry.AddRange(countries.Select(c => new SelectListItem
    {
      Value = c.CountryCode,
      Text  = c.Translations.FirstOrDefault()?.CountryName ?? c.CountryCode
    }));
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] string productCode)
  {
    var product = await _productDbHelper.GetByCodeAsync(productCode);
    if (product == null)
      return new JsonResult(new { success = false });

    var newStatus = product.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _productDbHelper.UpdateStatusAsync(productCode, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
