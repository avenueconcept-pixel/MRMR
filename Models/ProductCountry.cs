namespace MyApp.Models;

public class ProductCountry
{
  public int     Id          { get; set; }
  public string  ProductCode { get; set; } = string.Empty;
  public string  CountryCode { get; set; } = string.Empty;
  public bool    IsEnabled   { get; set; } = true;
  public string  StockStatus { get; set; } = string.Empty;

  public Product? Product { get; set; }
  public Country? Country { get; set; }
}
