namespace MyApp.Models;

public class IncentivePeriod
{
  public int       Id          { get; set; }
  public DateOnly  PeriodDate  { get; set; }
  public string    Status      { get; set; } = string.Empty;
  public DateTime? ClosedAt    { get; set; }
  public DateTime? ProcessedAt { get; set; }
  public string    CreatedBy   { get; set; } = string.Empty;
  public DateTime  CreatedAt   { get; set; }
  public string    UpdatedBy   { get; set; } = string.Empty;
  public DateTime  UpdatedAt   { get; set; }

  public ICollection<WalletPayout> Payouts { get; set; } = new List<WalletPayout>();
}
