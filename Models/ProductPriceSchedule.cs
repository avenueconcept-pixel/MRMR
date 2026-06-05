namespace MyApp.Models;

public class ProductPriceSchedule
{
  public int       Id           { get; set; }
  public string    ProductCode  { get; set; } = string.Empty;
  public string    CountryCode  { get; set; } = string.Empty;
  public string    TierCode     { get; set; } = string.Empty;
  public string    ScheduleType { get; set; } = string.Empty;
  public DateTime  ValidFrom    { get; set; }
  public DateTime? ValidTo      { get; set; }
  public string    Status       { get; set; } = string.Empty;
  public string    CreatedBy    { get; set; } = string.Empty;
  public DateTime  CreatedAt    { get; set; }
  public string    UpdatedBy    { get; set; } = string.Empty;
  public DateTime  UpdatedAt    { get; set; }

  public Product?   Product   { get; set; }
  public Country?   Country   { get; set; }
  public PriceTier? PriceTier { get; set; }
}
