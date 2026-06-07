namespace MyApp.Models;

public class WalletPayout
{
  public long      Id                { get; set; }
  public int       IncentivePeriodId { get; set; }
  public DateOnly  PeriodDate        { get; set; }
  public int       MemberId          { get; set; }
  public string    IncentiveType     { get; set; } = string.Empty;
  public decimal   PvAmount          { get; set; }
  public decimal   AmountUsd         { get; set; }
  public string?   ReferenceId       { get; set; }
  public string?   Remark            { get; set; }
  public string    Status            { get; set; } = string.Empty;
  public int       RetryCount        { get; set; }
  public string?   ErrorMessage      { get; set; }
  public DateTime? ProcessedAt       { get; set; }
  public DateTime  CreatedAt         { get; set; }

  public IncentivePeriod? IncentivePeriod { get; set; }
  public Member?           Member          { get; set; }
}
