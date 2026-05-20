using MyApp.Constants;

namespace MyApp.Models;

public class Language
{
  public int Id { get; set; }
  public string LanguageCode { get; set; } = string.Empty;
  public string LanguageName { get; set; } = string.Empty;
  public string NativeName { get; set; } = string.Empty;
  public int SortOrder { get; set; }
  public string Status { get; set; } = UserStatusConstants.Active;
  public DateTime CreatedAt { get; set; }
}
