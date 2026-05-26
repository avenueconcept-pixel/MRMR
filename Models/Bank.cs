using MyApp.Constants;

namespace MyApp.Models;

public class Bank
{
  public string   BankCode    { get; set; } = string.Empty;
  public string   CountryCode { get; set; } = string.Empty;
  public string?  SwiftCode   { get; set; }
  public string?  LocalCode   { get; set; }
  public string?  Website     { get; set; }
  public string?  Logo        { get; set; }
  public string   Status      { get; set; } = StatusConstants.Active;
  public DateTime CreatedAt   { get; set; }
  public string   CreatedBy   { get; set; } = string.Empty;
  public DateTime UpdatedAt   { get; set; }
  public string   UpdatedBy   { get; set; } = string.Empty;

  public Country?                      Country      { get; set; }
  public ICollection<BankTranslation>  Translations { get; set; } = new List<BankTranslation>();
}
