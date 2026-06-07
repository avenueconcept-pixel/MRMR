using MyApp.Models;

namespace MyApp.Dtos;

public class WalletSummaryDto
{
  public decimal CashBalance     { get; set; }
  public decimal PurchaseBalance { get; set; }
  public string  CurrencyCode    { get; set; } = string.Empty;
  public string  CurrencySymbol  { get; set; } = string.Empty;
  public decimal ExchangeRate    { get; set; }
}

public class PostTransactionDto
{
  public int      MemberId          { get; set; }
  public string   WalletType        { get; set; } = string.Empty;
  public string   TxnType           { get; set; } = string.Empty;
  public decimal  AmountUsd         { get; set; }
  public string   Direction         { get; set; } = string.Empty;
  public string?  ReferenceId       { get; set; }
  public string?  Remark            { get; set; }
  public int?     IncentivePeriodId { get; set; }
  public DateOnly? PeriodDate       { get; set; }
  public string   CreatedBy         { get; set; } = string.Empty;
}

public class WalletTxnRowDto
{
  public long     Id              { get; set; }
  public string   TxnType         { get; set; } = string.Empty;
  public decimal  AmountUsd       { get; set; }
  public string   Direction       { get; set; } = string.Empty;
  public decimal  BalanceAfter    { get; set; }
  public decimal  DisplayAmount   { get; set; }
  public string   DisplayCurrency { get; set; } = string.Empty;
  public decimal  ExchangeRate    { get; set; }
  public string?  ReferenceId     { get; set; }
  public string?  Remark          { get; set; }
  public string   CreatedBy       { get; set; } = string.Empty;
  public DateTime CreatedAt       { get; set; }
}

public class TransferValidationResult
{
  public bool    IsValid { get; set; }
  public string  Message { get; set; } = string.Empty;
  public Member? Target  { get; set; }
}

public class WalletPayoutRowDto
{
  public long      Id             { get; set; }
  public string    MemberUsername { get; set; } = string.Empty;
  public string    MemberFullName { get; set; } = string.Empty;
  public string    IncentiveType  { get; set; } = string.Empty;
  public decimal   PvAmount       { get; set; }
  public decimal   AmountUsd      { get; set; }
  public string?   ReferenceId    { get; set; }
  public string?   Remark         { get; set; }
  public string    Status         { get; set; } = string.Empty;
  public int       RetryCount     { get; set; }
  public string?   ErrorMessage   { get; set; }
  public DateTime? ProcessedAt    { get; set; }
  public DateTime  CreatedAt      { get; set; }
}

public class IncentivePeriodSummaryDto
{
  public int       Id             { get; set; }
  public DateOnly  PeriodDate     { get; set; }
  public string    Status         { get; set; } = string.Empty;
  public int       TotalPayouts   { get; set; }
  public int       PendingCount   { get; set; }
  public int       CompletedCount { get; set; }
  public int       FailedCount    { get; set; }
  public decimal   TotalAmountUsd { get; set; }
  public DateTime? ClosedAt       { get; set; }
  public DateTime? ProcessedAt    { get; set; }
}
