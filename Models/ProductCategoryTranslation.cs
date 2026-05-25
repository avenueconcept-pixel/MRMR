namespace MyApp.Models;

public class ProductCategoryTranslation
{
  public string          CategoryCode  { get; set; } = string.Empty;
  public string          LanguageCode  { get; set; } = string.Empty;
  public string          CategoryName  { get; set; } = string.Empty;

  public ProductCategory ProductCategory { get; set; } = null!;
}
