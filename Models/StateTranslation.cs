namespace MyApp.Models;

public class StateTranslation
{
  public int    StateId      { get; set; }
  public string LanguageCode { get; set; } = string.Empty;
  public string StateName    { get; set; } = string.Empty;

  public State State { get; set; } = null!;
}
