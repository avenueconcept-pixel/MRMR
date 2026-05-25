using MyApp.Constants;

namespace MyApp.Models;

public class State
{
  public int    Id          { get; set; }
  public string CountryCode { get; set; } = string.Empty;
  public string StateCode   { get; set; } = string.Empty;
  public int    SortOrder   { get; set; } = 0;
  public string Status      { get; set; } = StatusConstants.Active;
  public DateTime CreatedAt { get; set; }
  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;

  public ICollection<StateTranslation> Translations { get; set; } = new List<StateTranslation>();
  public Country Country { get; set; } = null!;
}
