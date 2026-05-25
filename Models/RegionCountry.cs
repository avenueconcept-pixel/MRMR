namespace MyApp.Models;

public class RegionCountry
{
  public int    RegionId    { get; set; }
  public string CountryCode { get; set; } = string.Empty;

  public Region  Region  { get; set; } = null!;
  public Country Country { get; set; } = null!;
}
