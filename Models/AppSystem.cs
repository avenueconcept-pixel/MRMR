using MyApp.Constants;

namespace MyApp.Models;

public class AppSystem
{
  public int      Id         { get; set; }
  public string   SystemCode { get; set; } = string.Empty;
  public string   SystemName { get; set; } = string.Empty;
  public int      SortOrder  { get; set; }
  public string   Status     { get; set; } = StatusConstants.Active;
  public string   CreatedBy  { get; set; } = string.Empty;
  public DateTime CreatedAt  { get; set; }
  public string   UpdatedBy  { get; set; } = string.Empty;
  public DateTime UpdatedAt  { get; set; }
}
