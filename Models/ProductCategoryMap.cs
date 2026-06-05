namespace MyApp.Models;

public class ProductCategoryMap
{
  public int    Id           { get; set; }
  public string ProductCode  { get; set; } = string.Empty;
  public string CategoryCode { get; set; } = string.Empty;

  public Product?         Product         { get; set; }
  public ProductCategory? ProductCategory { get; set; }
}
