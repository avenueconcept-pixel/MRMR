namespace MyApp.Models;

public class CashWalletTransactionArchive
{
  public long     Id              { get; set; }
  public int      MemberId        { get; set; }
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
