using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Models
{
  public class MenuItems
  {
    [Key]
    public int MenuId { get; set; }
    public string Title { get; set; }
    public string WordCode { get; set; }
    public string IconClass { get; set; }
    public string Route { get; set; }
    public string MatchPrefix { get; set; }
    public int? ParentId { get; set; }
    public int? IsActive { get; set; }
    public int SupportView { get; set; }
    public int SupportEdit { get; set; }
    public int SupportDelete { get; set; }
    public List<MenuItems> Children { get; set; } = new();
  }

  public class MenuPermissions
  {
    [Key]
    public Guid PermissionId { get; set; }
    public int MenuId { get; set; }
    public Guid  UserId { get; set; } // or RoleId

    public int ViewAccess { get; set; }
    public int EditAccess { get; set; }
    public int DeleteAccess { get; set; }
  }


}
