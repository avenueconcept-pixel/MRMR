namespace MyApp.Models;

public class ProductPriceHistory
{
  public long     Id          { get; set; }
  public string   ProductCode { get; set; } = string.Empty;
  public string   CountryCode { get; set; } = string.Empty;
  public string   TierCode    { get; set; } = string.Empty;
  public string   ChangeType  { get; set; } = string.Empty;
  public DateTime ChangedFrom { get; set; }
  public DateTime ChangedTo   { get; set; }
  public string   ChangedBy   { get; set; } = string.Empty;
  public DateTime CreatedAt   { get; set; }
}
