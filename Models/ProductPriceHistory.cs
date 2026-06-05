namespace MyApp.Models;

public class ProductPriceHistory
{
  public long     Id          { get; set; }
  public string   ProductCode { get; set; } = string.Empty;
  public string   CountryCode { get; set; } = string.Empty;
  public string   TierCode    { get; set; } = string.Empty;
  public string   ChangeType  { get; set; } = string.Empty;
  public decimal ChangedFrom { get; set; }
  public decimal ChangedTo   { get; set; }
  public string   ChangedBy   { get; set; } = string.Empty;
  public DateTime CreatedAt   { get; set; }
}
