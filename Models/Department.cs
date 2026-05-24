using MyApp.Constants;

namespace MyApp.Models;

public class Department
{
  public int      Id        { get; set; }
  public string   DeptName  { get; set; } = string.Empty;
  public string   Status    { get; set; } = StatusConstants.Active;
  public DateTime CreatedAt { get; set; }
  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;
}
