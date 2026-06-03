namespace MyApp.Models;

public class UnitOfMeasure
{
  public int    Id        { get; set; }
  public string UomCode   { get; set; } = string.Empty;
  public string UomName   { get; set; } = string.Empty;
  public string Status    { get; set; } = string.Empty;
  public string CreatedBy { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string UpdatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }
  public ICollection<UomTranslation> Translations { get; set; } = new List<UomTranslation>();
}
