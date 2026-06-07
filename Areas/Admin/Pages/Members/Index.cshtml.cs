using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Members;

public class IndexModel : AdminPageModel
{
  private readonly MemberDbHelper  _memberDbHelper;
  private readonly CountryDbHelper _countryDbHelper;
  private readonly TranslationService _translation;

  [BindProperty(SupportsGet = true)] public string? FilterStatus      { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterCountryCode { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterRankCode    { get; set; }
  [BindProperty(SupportsGet = true)] public bool?   FilterIsActivated { get; set; }

  public List<Member>         Items        { get; set; } = new();
  public List<SelectListItem> ddlStatus    { get; set; } = new();
  public List<SelectListItem> ddlCountry   { get; set; } = new();
  public List<SelectListItem> ddlRank      { get; set; } = new();
  public List<SelectListItem> ddlActivated { get; set; } = new();

  public IndexModel(MemberDbHelper memberDbHelper, CountryDbHelper countryDbHelper, TranslationService translation)
  {
    _memberDbHelper  = memberDbHelper;
    _countryDbHelper = countryDbHelper;
    _translation     = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    Items = await _memberDbHelper.GetAllAsync(
        FilterStatus, FilterCountryCode, FilterRankCode, FilterIsActivated, langCode);
    await PopulateFiltersAsync(langCode);
  }

  public async Task<IActionResult> OnGetSearchMembersAsync(string term)
  {
    var results = await _memberDbHelper.SearchAsync(term ?? string.Empty);
    return new JsonResult(results.Select(r => new { id = r.Id, text = $"{r.Username} — {r.FullName}" }));
  }

  public async Task<IActionResult> OnGetBinaryNodeAsync(int rootId)
  {
    var tree = await _memberDbHelper.GetBinaryTreeAsync(rootId, 3);
    if (tree == null)
      return new JsonResult(new { success = false });
    return new JsonResult(new { success = true, node = tree });
  }

  private async Task PopulateFiltersAsync(string langCode)
  {
    ddlStatus = new List<SelectListItem>
    {
      new() { Value = string.Empty, Text = await _translation.GetAsync("Members.Filter.AllStatuses") },
      new() { Value = StatusConstants.Active,   Text = await _translation.GetAsync("status.active") },
      new() { Value = StatusConstants.Inactive, Text = await _translation.GetAsync("status.inactive") }
    };

    var countries = await _countryDbHelper.GetAllActiveAsync(langCode);
    ddlCountry = new List<SelectListItem>
    {
      new() { Value = string.Empty, Text = await _translation.GetAsync("Members.Filter.AllCountries") }
    };
    ddlCountry.AddRange(countries.SelectMany(c => c.Translations.Select(t =>
        new SelectListItem { Value = c.CountryCode, Text = $"{t.CountryName} ({c.CountryCode})" })));

    var ranks = await _memberDbHelper.GetAllRanksAsync();
    ddlRank = new List<SelectListItem>
    {
      new() { Value = string.Empty, Text = await _translation.GetAsync("Members.Filter.AllRanks") }
    };
    ddlRank.AddRange(ranks.Select(r => new SelectListItem { Value = r.RankCode, Text = r.RankName }));

    ddlActivated = new List<SelectListItem>
    {
      new() { Value = string.Empty,  Text = await _translation.GetAsync("Members.Filter.AllActivated") },
      new() { Value = "true",        Text = await _translation.GetAsync("Members.Filter.Activated") },
      new() { Value = "false",       Text = await _translation.GetAsync("Members.Filter.NotActivated") }
    };
  }
}
