using MyApp.Constants;

namespace MyApp.Models;

public class EmailTemplate
{
  public int    Id           { get; set; }
  public string LanguageCode { get; set; } = string.Empty;
  public string TemplateKey  { get; set; } = string.Empty;
  public string Subject      { get; set; } = string.Empty;
  public string BodyHtml     { get; set; } = string.Empty;
  public string Status       { get; set; } = StatusConstants.Active;
  public string CreatedBy    { get; set; } = string.Empty;
  public DateTime CreatedAt  { get; set; }
  public string UpdatedBy    { get; set; } = string.Empty;
  public DateTime UpdatedAt  { get; set; }
}
