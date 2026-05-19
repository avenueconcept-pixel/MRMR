namespace MyApp.Models
{

  public class EditUserViewModel
  {
    public Guid UserId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
  }

  public class DeleteUserViewModel
  {
    public Guid UserId { get; set; }
  }

  public class UserPermissionsViewModel
  {
    public Guid UserId { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
  }

  public class PermissionInput
  {
    public int MenuId { get; set; }
    public bool View { get; set; }
    public bool Edit { get; set; }
    public bool Delete { get; set; }
  }



}
