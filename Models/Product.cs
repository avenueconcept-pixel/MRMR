namespace MyApp.Models;

public class Product
{
  public int     Id            { get; set; }
  public string  ProductCode   { get; set; } = string.Empty;
  public string  ProductType   { get; set; } = string.Empty;
  public string  ProductNature { get; set; } = string.Empty;
  public string  UomCode       { get; set; } = string.Empty;
  public decimal Pv            { get; set; }
  public int     SortOrder     { get; set; }
  public string  Status        { get; set; } = string.Empty;
  public string  CreatedBy     { get; set; } = string.Empty;
  public DateTime CreatedAt    { get; set; }
  public string  UpdatedBy     { get; set; } = string.Empty;
  public DateTime UpdatedAt    { get; set; }

  public UnitOfMeasure?                    UnitOfMeasure { get; set; }
  public ICollection<ProductTranslation>   Translations  { get; set; } = new List<ProductTranslation>();
  public ICollection<ProductCategoryMap>   CategoryMaps  { get; set; } = new List<ProductCategoryMap>();
  public ICollection<ProductCountry>       Countries     { get; set; } = new List<ProductCountry>();
  public ICollection<ProductPriceTier>     PriceTiers    { get; set; } = new List<ProductPriceTier>();
  public ICollection<ProductSection>       Sections      { get; set; } = new List<ProductSection>();
  public ICollection<ProductImage>         Images        { get; set; } = new List<ProductImage>();
  public ICollection<ProductPackageItem>   PackageItems  { get; set; } = new List<ProductPackageItem>();
}
