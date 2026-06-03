namespace MyApp.Models;

public class PriceTier
{
  public int    Id        { get; set; }
  public string TierCode  { get; set; } = string.Empty;
  public string TierName  { get; set; } = string.Empty;
  public int    SortOrder { get; set; }
  public string Status    { get; set; } = string.Empty;
  public string CreatedBy { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string UpdatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }
}
