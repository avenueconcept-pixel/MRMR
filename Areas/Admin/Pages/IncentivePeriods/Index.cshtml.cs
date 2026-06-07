using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.IncentivePeriods;

public class IndexModel : AdminPageModel
{
  private readonly WalletPayoutDbHelper _payoutDbHelper;
  private readonly TranslationService   _translation;

  [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
  [BindProperty(SupportsGet = true)] public int     CurrentPage  { get; set; } = 1;

  public List<IncentivePeriodSummaryDto> Items      { get; set; } = new();
  public int                             TotalCount  { get; set; }
  public int                             PageSize    => 30;
  public int                             TotalPages  => (int)Math.Ceiling((double)TotalCount / PageSize);
  public List<SelectListItem>            ddlStatus   { get; set; } = new();

  public string MsgRetrySuccess { get; set; } = string.Empty;
  public string MsgRetryError   { get; set; } = string.Empty;

  public IndexModel(WalletPayoutDbHelper payoutDbHelper, TranslationService translation)
  {
    _payoutDbHelper = payoutDbHelper;
    _translation    = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    (Items, TotalCount) = await _payoutDbHelper.GetAllAsync(CurrentPage, PageSize, FilterStatus);

    MsgRetrySuccess = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    MsgRetryError   = await _translation.GetAsync(MessageConstants.SaveError);

    ddlStatus = new List<SelectListItem>
    {
      new() { Value = string.Empty,                               Text = await _translation.GetAsync("Filter.All") },
      new() { Value = IncentivePeriodStatusConstants.Open,        Text = await _translation.GetAsync("WalletPayout.Status.Pending") },
      new() { Value = IncentivePeriodStatusConstants.Closed,      Text = await _translation.GetAsync("IncentivePeriods.Status.Closed") },
      new() { Value = IncentivePeriodStatusConstants.Processed,   Text = await _translation.GetAsync("IncentivePeriods.Status.Processed") },
      new() { Value = IncentivePeriodStatusConstants.Partial,     Text = await _translation.GetAsync("IncentivePeriods.Status.Partial") }
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
