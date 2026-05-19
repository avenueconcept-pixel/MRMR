using MyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Dtos
{
  public class Dto_UserList
  {
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string? Username { get; set; } = "";

    public string? FullName { get; set; } = "";

    public int? LoginStatus { get; set; } = 0;
    public string? StatusName { get; set; } = "";
    public string? DepartmentCode { get; set; } = "";

    public string? Department { get; set; } = "";
    public string? Branch { get; set; } = "";
  }


  public class Dto_Permission
  {
    public int MenuId { get; set; }
    public bool View { get; set; }
    public bool Edit { get; set; }
    public bool Delete { get; set; }
  }


  public class Dto_BranchList
  {
    public Guid BranchId { get; set; } = Guid.NewGuid();
    public string? BranchName { get; set; } = "";
    
    public int? IsActive { get; set; } = 0;
    public bool IsSelected { get; set; }

    public int? UserCount { get; set; } = 0;

  }

}
