using MyApp.Constants;

namespace MyApp.Models;

public class Permission
{
  public int    Id             { get; set; }
  public string PermissionCode { get; set; } = string.Empty;
  public string Module         { get; set; } = string.Empty;
  public string PermissionName { get; set; } = string.Empty;
  public int    SortOrder      { get; set; }
  public string Status         { get; set; } = StatusConstants.Active;

}
