namespace MyApp.Models;

public class ProductTranslation
{
  public int    Id               { get; set; }
  public string ProductCode      { get; set; } = string.Empty;
  public string LanguageCode     { get; set; } = string.Empty;
  public string ProductName      { get; set; } = string.Empty;
  public string ShortDescription { get; set; } = string.Empty;

  public Product? Product { get; set; }
}
