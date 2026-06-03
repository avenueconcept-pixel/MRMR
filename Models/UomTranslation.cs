namespace MyApp.Models;

public class UomTranslation
{
  public int    Id           { get; set; }
  public string UomCode      { get; set; } = string.Empty;
  public string LanguageCode { get; set; } = string.Empty;
  public string UomName      { get; set; } = string.Empty;
  public UnitOfMeasure? UnitOfMeasure { get; set; }
}
