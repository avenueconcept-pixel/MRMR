using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.ExchangeRates;

public class HistoryModel : AdminPageModel
{
  private readonly ExchangeRateDbHelper _exchangeRateDbHelper;
  private readonly TranslationService   _translation;

  [BindProperty(SupportsGet = true)] public string? FilterCurrency { get; set; }
  [BindProperty(SupportsGet = true)] public int     CurrentPage    { get; set; } = 1;

  public List<ExchangeRate> Items      { get; set; } = new();
  public int                TotalCount { get; set; }
  public int                PageSize   => 50;
  public int                TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

  public HistoryModel(ExchangeRateDbHelper exchangeRateDbHelper, TranslationService translation)
  {
    _exchangeRateDbHelper = exchangeRateDbHelper;
    _translation          = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    (Items, TotalCount) = await _exchangeRateDbHelper.GetHistoryPagedAsync(
        FilterCurrency ?? string.Empty, CurrentPage, PageSize);
  }
}
