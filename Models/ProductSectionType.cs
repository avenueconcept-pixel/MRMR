namespace MyApp.Models;

public class ProductSectionType
{
  public int    Id          { get; set; }
  public string SectionCode { get; set; } = string.Empty;
  public int    SortOrder   { get; set; }
  public string Status      { get; set; } = string.Empty;
  public string CreatedBy   { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string UpdatedBy   { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }
  public ICollection<ProductSectionTypeTranslation> Translations { get; set; } = new List<ProductSectionTypeTranslation>();
}
