namespace MyApp.Models;

public class CountryTranslation
{
  public string CountryCode { get; set; } = string.Empty;
  public string LanguageCode { get; set; } = string.Empty;
  public string CountryName { get; set; } = string.Empty;
  public Country Country { get; set; } = null!;
}
