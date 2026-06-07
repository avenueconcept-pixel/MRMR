using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Wallets;

public class HistoryModel : AdminPageModel
{
  private readonly WalletDbHelper    _walletDbHelper;
  private readonly TranslationService _translation;

  [BindProperty(SupportsGet = true)] public string? FilterUsername     { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterWalletType   { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterTxnType      { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterCurrencyCode { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterStartDate    { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterEndDate      { get; set; }
  [BindProperty(SupportsGet = true)] public int     CurrentPage        { get; set; } = 1;

  public List<AdminWalletTxnRowDto> Items      { get; set; } = new();
  public int                        TotalCount  { get; set; }
  public int                        PageSize    => 50;
  public int                        TotalPages  => (int)Math.Ceiling((double)TotalCount / PageSize);

  public List<SelectListItem> ddlWalletType { get; set; } = new();
  public List<SelectListItem> ddlTxnType   { get; set; } = new();

  public HistoryModel(WalletDbHelper walletDbHelper, TranslationService translation)
  {
    _walletDbHelper = walletDbHelper;
    _translation    = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";

    DateTime? filterStartUtc = null;
    if (!string.IsNullOrEmpty(FilterStartDate) &&
        DateTime.TryParseExact(FilterStartDate, AppConstants.DateInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var startLocal))
      filterStartUtc = startLocal.ToUtcFromUserTimezone(UserTimezone);

    DateTime? filterEndUtc = null;
    if (!string.IsNullOrEmpty(FilterEndDate) &&
        DateTime.TryParseExact(FilterEndDate, AppConstants.DateInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var endLocal))
      filterEndUtc = endLocal.AddDays(1).AddSeconds(-1).ToUtcFromUserTimezone(UserTimezone);

    (Items, TotalCount) = await _walletDbHelper.GetAllTransactionsAsync(
        FilterUsername, FilterWalletType, FilterTxnType, FilterCurrencyCode,
        filterStartUtc, filterEndUtc, CurrentPage, PageSize);

    ddlWalletType = new List<SelectListItem>
    {
      new() { Value = string.Empty,                 Text = await _translation.GetAsync("Filter.All") },
      new() { Value = WalletTypeConstants.Cash,     Text = await _translation.GetAsync("Wallets.Type.Cash") },
      new() { Value = WalletTypeConstants.Purchase, Text = await _translation.GetAsync("Wallets.Type.Purchase") }
    };

    ddlTxnType = new List<SelectListItem>
    {
      new() { Value = string.Empty,                         Text = await _translation.GetAsync("Filter.All") },
      new() { Value = CashTxnTypeConstants.Commission,      Text = await _translation.GetAsync("Wallets.TxnType.Commission") },
      new() { Value = CashTxnTypeConstants.Adjustment,      Text = await _translation.GetAsync("Wallets.TxnType.Adjustment") },
      new() { Value = CashTxnTypeConstants.Withdrawal,      Text = await _translation.GetAsync("Wallets.TxnType.Withdrawal") },
      new() { Value = CashTxnTypeConstants.TransferIn,      Text = await _translation.GetAsync("Wallets.TxnType.TransferIn") },
      new() { Value = CashTxnTypeConstants.TransferOut,     Text = await _translation.GetAsync("Wallets.TxnType.TransferOut") },
      new() { Value = PurchaseTxnTypeConstants.TopUp,       Text = await _translation.GetAsync("Wallets.TxnType.TopUp") },
      new() { Value = PurchaseTxnTypeConstants.Payment,     Text = await _translation.GetAsync("Wallets.TxnType.Payment") }
    };
  }
}
