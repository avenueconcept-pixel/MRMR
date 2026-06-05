namespace MyApp.Models;

public class ProductSectionTranslation
{
  public int    Id               { get; set; }
  public int    ProductSectionId { get; set; }
  public string LanguageCode     { get; set; } = string.Empty;
  public string Content          { get; set; } = string.Empty;

  public ProductSection? ProductSection { get; set; }
}
