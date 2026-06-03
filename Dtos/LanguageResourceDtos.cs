namespace MyApp.Dtos;

public class TranslationRowDto
{
  public string Key          { get; set; } = string.Empty;
  public string LanguageCode { get; set; } = string.Empty;
  public string Value        { get; set; } = string.Empty;
}

public class TranslationGridRowDto
{
  public string                    Key    { get; set; } = string.Empty;
  public Dictionary<string, string> Values { get; set; } = new();
}

public class ImportResultDto
{
  public int          Inserted { get; set; }
  public int          Updated  { get; set; }
  public int          Skipped  { get; set; }
  public List<string> Errors   { get; set; } = new();
}
