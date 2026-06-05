namespace MyApp.Models;

public class ProductSection
{
  public int    Id          { get; set; }
  public string ProductCode { get; set; } = string.Empty;
  public string SectionCode { get; set; } = string.Empty;
  public int    SortOrder   { get; set; }

  public Product?                               Product            { get; set; }
  public ProductSectionType?                    ProductSectionType { get; set; }
  public ICollection<ProductSectionTranslation> Translations       { get; set; } = new List<ProductSectionTranslation>();
}
