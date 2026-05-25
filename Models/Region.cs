using MyApp.Constants;

namespace MyApp.Models;

public class Region
{
  public int      Id         { get; set; }
  public string   RegionCode { get; set; } = string.Empty;
  public string   RegionName { get; set; } = string.Empty;
  public string   Status     { get; set; } = StatusConstants.Active;
  public DateTime CreatedAt  { get; set; }
  public string   CreatedBy  { get; set; } = string.Empty;
  public DateTime UpdatedAt  { get; set; }
  public string   UpdatedBy  { get; set; } = string.Empty;

  public ICollection<RegionCountry> RegionCountries { get; set; } = new List<RegionCountry>();
}
