namespace MyApp.Models;

public class BankTranslation
{
  public string BankCode     { get; set; } = string.Empty;
  public string LanguageCode { get; set; } = string.Empty;
  public string BankName     { get; set; } = string.Empty;
  public string ShortName    { get; set; } = string.Empty;

  public Bank? Bank { get; set; }
}
