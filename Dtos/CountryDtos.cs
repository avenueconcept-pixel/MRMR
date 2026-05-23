namespace MyApp.Dtos;

public enum CountryAddResult
{
  Created,
  Restored,
  DuplicateActive
}

public class TranslationInputDto
{
  public string LanguageCode { get; set; } = string.Empty;
  public string Label       { get; set; } = string.Empty;
  public string Value       { get; set; } = string.Empty;
  public string Placeholder { get; set; } = string.Empty;
}
