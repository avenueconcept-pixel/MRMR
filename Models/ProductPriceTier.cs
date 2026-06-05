namespace MyApp.Models;

public class ProductPriceTier
{
  public int     Id          { get; set; }
  public string  ProductCode { get; set; } = string.Empty;
  public string  CountryCode { get; set; } = string.Empty;
  public string  TierCode    { get; set; } = string.Empty;
  public string? VariantCode { get; set; }
  public decimal Price       { get; set; }

  public Product?   Product   { get; set; }
  public Country?   Country   { get; set; }
  public PriceTier? PriceTier { get; set; }
}
