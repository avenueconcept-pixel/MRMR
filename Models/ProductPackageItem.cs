namespace MyApp.Models;

public class ProductPackageItem
{
  public int    Id                  { get; set; }
  public string PackageProductCode  { get; set; } = string.Empty;
  public string ItemProductCode     { get; set; } = string.Empty;
  public int    Quantity            { get; set; } = 1;
  public int    SortOrder           { get; set; }

  public Product? PackageProduct { get; set; }
  public Product? ItemProduct    { get; set; }
}
