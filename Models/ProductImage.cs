namespace MyApp.Models;

public class ProductImage
{
  public int     Id             { get; set; }
  public string  ProductCode    { get; set; } = string.Empty;
  public string  CountryCode    { get; set; } = string.Empty;
  public string  LanguageCode   { get; set; } = string.Empty;
  public string  ImageFilename  { get; set; } = string.Empty;
  public int     SortOrder      { get; set; }
  public bool    IsPrimary      { get; set; }
  public string  CreatedBy      { get; set; } = string.Empty;
  public DateTime CreatedAt     { get; set; }

  public Product? Product { get; set; }
  public Country? Country { get; set; }
}
