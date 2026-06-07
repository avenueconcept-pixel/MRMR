using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.ExchangeRates;

public class IndexModel : AdminPageModel
{
  private readonly ExchangeRateDbHelper _exchangeRateDbHelper;
  private readonly TranslationService   _translation;

  public List<ExchangeRate> Items { get; set; } = new();

  [BindProperty] public string  txtCurrencyCode     { get; set; } = string.Empty;
  [BindProperty] public decimal txtRateToBase        { get; set; }
  [BindProperty] public string  txtEffectiveDatetime { get; set; } = string.Empty;

  public string MsgSaveSuccess { get; set; } = string.Empty;
  public string MsgSaveError   { get; set; } = string.Empty;

  public IndexModel(ExchangeRateDbHelper exchangeRateDbHelper, TranslationService translation)
  {
    _exchangeRateDbHelper = exchangeRateDbHelper;
    _translation          = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    Items          = await _exchangeRateDbHelper.GetLatestAllAsync();
    MsgSaveSuccess = await _translation.GetAsync(MessageConstants.SaveSuccess);
    MsgSaveError   = await _translation.GetAsync(MessageConstants.SaveError);
  }

  public async Task<IActionResult> OnPostAddAsync()
  {
    try
    {
      if (!DateTime.TryParseExact(txtEffectiveDatetime, AppConstants.DateTimeInputFormat,
              null, System.Globalization.DateTimeStyles.None, out var effectiveLocal))
        effectiveLocal = DateTime.Now;

      var effectiveUtc = effectiveLocal.ToUtcFromUserTimezone(UserTimezone);

      var rate = new ExchangeRate
      {
        CurrencyCode      = txtCurrencyCode.Trim().ToUpper(),
        RateToBase        = txtRateToBase,
        EffectiveDatetime = effectiveUtc,
        CreatedBy         = CurrentUsername,
        CreatedAt         = DateTime.UtcNow
      };
      await _exchangeRateDbHelper.AddAsync(rate);
      var msg = await _translation.GetAsync(MessageConstants.SaveSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }
}
