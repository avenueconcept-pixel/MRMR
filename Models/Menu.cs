using MyApp.Constants;

namespace MyApp.Models;

public class Menu
{
  public int      Id        { get; set; }
  public string   MenuCode  { get; set; } = string.Empty;
  public int?     ParentId  { get; set; }
  public string   MenuName  { get; set; } = string.Empty;
  public string?  MenuIcon  { get; set; }
  public string?  MenuUrl   { get; set; }
  public int      SortOrder { get; set; }
  public int      Level     { get; set; }
  public string   Status    { get; set; } = StatusConstants.Active;
  public DateTime CreatedAt { get; set; }
  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;

  public Menu?                   Parent      { get; set; }
  public ICollection<Menu>       Children    { get; set; } = new List<Menu>();
  public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
