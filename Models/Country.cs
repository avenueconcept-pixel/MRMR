using MyApp.Constants;

namespace MyApp.Models;

public class Country
{
  public string CountryCode { get; set; } = string.Empty;
  public string CurrencyCode { get; set; } = string.Empty;
  public string Timezone { get; set; } = string.Empty;
  public string Status { get; set; } = UserStatusConstants.Active;
  public DateTime CreatedAt { get; set; }
  public string CreatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }
  public string UpdatedBy { get; set; } = string.Empty;
  public ICollection<CountryTranslation> Translations { get; set; } = new List<CountryTranslation>();
}
