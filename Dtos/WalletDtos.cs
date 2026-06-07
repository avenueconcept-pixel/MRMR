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
  public int     MemberId    { get; set; }
  public string  WalletType  { get; set; } = string.Empty;
  public string  TxnType     { get; set; } = string.Empty;
  public decimal AmountUsd   { get; set; }
  public string  Direction   { get; set; } = string.Empty;
  public string? ReferenceId { get; set; }
  public string? Remark      { get; set; }
  public string  CreatedBy   { get; set; } = string.Empty;
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
