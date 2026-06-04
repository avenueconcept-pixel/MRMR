namespace MyApp.Models;

public class ProductSectionTypeTranslation
{
  public int    Id           { get; set; }
  public string SectionCode  { get; set; } = string.Empty;
  public string LanguageCode { get; set; } = string.Empty;
  public string SectionName  { get; set; } = string.Empty;
  public ProductSectionType? ProductSectionType { get; set; }
}
