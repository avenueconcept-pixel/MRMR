using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Members;

public class WalletModel : AdminPageModel
{
  private readonly WalletDbHelper     _walletDbHelper;
  private readonly MemberDbHelper     _memberDbHelper;
  private readonly TranslationService _translation;

  public int     MemberId        { get; set; }
  public string  MemberUsername  { get; set; } = string.Empty;
  public string  MemberFullName  { get; set; } = string.Empty;
  public decimal CashBalance     { get; set; }
  public decimal PurchaseBalance { get; set; }
  public string  CurrencyCode    { get; set; } = string.Empty;
  public string  CurrencySymbol  { get; set; } = string.Empty;
  public decimal ExchangeRate    { get; set; }

  [BindProperty(SupportsGet = true)] public int    CashPage     { get; set; } = 1;
  [BindProperty(SupportsGet = true)] public int    PurchasePage { get; set; } = 1;
  [BindProperty(SupportsGet = true)] public string Tab          { get; set; } = "cash";
  public int PageSize => 20;

  public List<CashWalletTransaction>     CashTransactions     { get; set; } = new();
  public int                             CashTotal            { get; set; }
  public int                             CashTotalPages       => (int)Math.Ceiling((double)CashTotal / PageSize);
  public List<PurchaseWalletTransaction> PurchaseTransactions { get; set; } = new();
  public int                             PurchaseTotal        { get; set; }
  public int                             PurchaseTotalPages   => (int)Math.Ceiling((double)PurchaseTotal / PageSize);

  // Adjustment form
  [BindProperty] public string  ddlAdjWalletType  { get; set; } = string.Empty;
  [BindProperty] public string  ddlAdjDirection   { get; set; } = string.Empty;
  [BindProperty] public decimal txtAmountUsd      { get; set; }
  [BindProperty] public string  txtRemark         { get; set; } = string.Empty;
  [BindProperty] public string? txtIdempotencyKey { get; set; }

  // Transfer form
  [BindProperty] public int?    ToMemberId        { get; set; }
  [BindProperty] public decimal txtTransferAmount { get; set; }
  [BindProperty] public string  txtTransferRemark { get; set; } = string.Empty;

  public string MsgUpdateSuccess { get; set; } = string.Empty;
  public string MsgSaveError     { get; set; } = string.Empty;

  public WalletModel(WalletDbHelper walletDbHelper, MemberDbHelper memberDbHelper, TranslationService translation)
  {
    _walletDbHelper = walletDbHelper;
    _memberDbHelper = memberDbHelper;
    _translation    = translation;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";
    var member = await _memberDbHelper.GetByIdAsync(id);
    if (member == null) return RedirectToPage(Routes.AdminMembers);

    MemberId       = member.Id;
    MemberUsername = member.Username;
    MemberFullName = member.FullName;

    var summary = await _walletDbHelper.GetSummaryAsync(id);
    CashBalance     = summary.CashBalance;
    PurchaseBalance = summary.PurchaseBalance;
    CurrencyCode    = summary.CurrencyCode;
    CurrencySymbol  = summary.CurrencySymbol;
    ExchangeRate    = summary.ExchangeRate;

    (CashTransactions, CashTotal)         = await _walletDbHelper.GetCashHistoryAsync(id, CashPage, PageSize);
    (PurchaseTransactions, PurchaseTotal) = await _walletDbHelper.GetPurchaseHistoryAsync(id, PurchasePage, PageSize);

    MsgUpdateSuccess = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    MsgSaveError     = await _translation.GetAsync(MessageConstants.SaveError);

    return Page();
  }

  public async Task<IActionResult> OnPostAdjustmentAsync(int id)
  {
    try
    {
      if (txtAmountUsd <= 0)
      {
        var err = await _translation.GetAsync("Wallet.Error.InvalidAmount");
        return new JsonResult(new { success = false, message = err });
      }

      await _walletDbHelper.PostAdjustmentAsync(
          id, ddlAdjWalletType, txtAmountUsd, ddlAdjDirection,
          txtRemark.Trim(), CurrentUsername, txtIdempotencyKey);
      var msg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch (InvalidOperationException ex) when (ex.Message == "duplicate_adjustment")
    {
      var msg = await _translation.GetAsync("Wallet.Error.DuplicateAdjustment");
      return new JsonResult(new { success = false, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostTransferAsync(int id)
  {
    try
    {
      if (!ToMemberId.HasValue || txtTransferAmount <= 0)
      {
        var err = await _translation.GetAsync("Wallet.Error.InvalidAmount");
        return new JsonResult(new { success = false, message = err });
      }

      var validation = await _walletDbHelper.ValidateTransferAsync(id, ToMemberId.Value, txtTransferAmount);
      if (!validation.IsValid)
        return new JsonResult(new { success = false, message = validation.Message });

      await _walletDbHelper.PostTransferAsync(id, ToMemberId.Value, txtTransferAmount, CurrentUsername);
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
