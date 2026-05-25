using MyApp.Constants;

namespace MyApp.Models;

public class Location
{
  public int      Id           { get; set; }
  public string   LocationCode { get; set; } = string.Empty;
  public string   LocationName { get; set; } = string.Empty;
  public string   LocationType { get; set; } = string.Empty;
  public string   CountryCode  { get; set; } = string.Empty;
  public int?     StateId      { get; set; }
  public string?  City         { get; set; }
  public string?  Postcode     { get; set; }
  public string?  Address      { get; set; }
  public string   Status       { get; set; } = StatusConstants.Active;
  public DateTime CreatedAt    { get; set; }
  public string   CreatedBy    { get; set; } = string.Empty;
  public DateTime UpdatedAt    { get; set; }
  public string   UpdatedBy    { get; set; } = string.Empty;

  public Country? Country { get; set; }
  public State?   State   { get; set; }
}
