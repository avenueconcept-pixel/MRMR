using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.IncentivePeriods;

public class ViewModel : AdminPageModel
{
  private readonly WalletPayoutDbHelper _payoutDbHelper;
  private readonly TranslationService   _translation;

  [BindProperty(SupportsGet = true)] public int     PeriodId     { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterType   { get; set; }
  [BindProperty(SupportsGet = true)] public int     CurrentPage  { get; set; } = 1;

  public IncentivePeriodSummaryDto?    Period     { get; set; }
  public List<WalletPayoutRowDto>      Items      { get; set; } = new();
  public int                           TotalCount  { get; set; }
  public int                           PageSize    => 50;
  public int                           TotalPages  => (int)Math.Ceiling((double)TotalCount / PageSize);
  public List<SelectListItem>          ddlStatus   { get; set; } = new();
  public List<SelectListItem>          ddlType     { get; set; } = new();

  public string MsgRetrySuccess { get; set; } = string.Empty;
  public string MsgRetryError   { get; set; } = string.Empty;

  public ViewModel(WalletPayoutDbHelper payoutDbHelper, TranslationService translation)
  {
    _payoutDbHelper = payoutDbHelper;
    _translation    = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    Period = await _payoutDbHelper.GetPeriodSummaryAsync(PeriodId);
    (Items, TotalCount) = await _payoutDbHelper.GetPayoutsAsync(
        PeriodId, CurrentPage, PageSize, FilterStatus, FilterType);

    MsgRetrySuccess = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    MsgRetryError   = await _translation.GetAsync(MessageConstants.SaveError);

    ddlStatus = new List<SelectListItem>
    {
      new() { Value = string.Empty,                                Text = await _translation.GetAsync("Filter.All") },
      new() { Value = WalletPayoutStatusConstants.Pending,         Text = await _translation.GetAsync("WalletPayout.Status.Pending") },
      new() { Value = WalletPayoutStatusConstants.Processing,      Text = await _translation.GetAsync("WalletPayout.Status.Processing") },
      new() { Value = WalletPayoutStatusConstants.Completed,       Text = await _translation.GetAsync("WalletPayout.Status.Completed") },
      new() { Value = WalletPayoutStatusConstants.Failed,          Text = await _translation.GetAsync("WalletPayout.Status.Failed") }
    };

    ddlType = new List<SelectListItem>
    {
      new() { Value = string.Empty,                                 Text = await _translation.GetAsync("Filter.All") },
      new() { Value = IncentiveTypeConstants.RetailProfit,          Text = await _translation.GetAsync("WalletPayout.Type.RetailProfit") },
      new() { Value = IncentiveTypeConstants.PersonalSalesBonus,    Text = await _translation.GetAsync("WalletPayout.Type.PersonalSalesBonus") },
      new() { Value = IncentiveTypeConstants.GroupOverride,         Text = await _translation.GetAsync("WalletPayout.Type.GroupOverride") },
      new() { Value = IncentiveTypeConstants.RankBonus,             Text = await _translation.GetAsync("WalletPayout.Type.RankBonus") },
      new() { Value = IncentiveTypeConstants.Adjustment,            Text = await _translation.GetAsync("WalletPayout.Type.Adjustment") }
    };
  }

  public async Task<IActionResult> OnPostRetryFailedAsync(int periodId)
  {
    try
    {
      await _payoutDbHelper.RetryFailedAsync(periodId, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }
}
