namespace MyApp.Models;

public class ExchangeRate
{
  public int      Id                { get; set; }
  public string   CurrencyCode      { get; set; } = string.Empty;
  public decimal  RateToBase        { get; set; }
  public DateTime EffectiveDatetime { get; set; }
  public string   CreatedBy         { get; set; } = string.Empty;
  public DateTime CreatedAt         { get; set; }
}
