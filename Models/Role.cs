using MyApp.Constants;

namespace MyApp.Models;

public class Role
{
  public int      Id            { get; set; }
  public string   RoleCode      { get; set; } = string.Empty;
  public string   RoleName      { get; set; } = string.Empty;
  public string?  Description   { get; set; }
  public bool     IsSuperAdmin  { get; set; } = false;
  public string   DataScope     { get; set; } = string.Empty;
  public string   Status        { get; set; } = StatusConstants.Active;
  public DateTime CreatedAt     { get; set; }
  public string   CreatedBy     { get; set; } = string.Empty;
  public DateTime UpdatedAt     { get; set; }
  public string   UpdatedBy     { get; set; } = string.Empty;

  public ICollection<RoleMenu>       RoleMenus       { get; set; } = new List<RoleMenu>();
  public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
