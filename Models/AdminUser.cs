using MyApp.Constants;

namespace MyApp.Models;

public class AdminUser
{
  public int Id { get; set; }
  public string Username { get; set; } = string.Empty;
  public string PasswordHash { get; set; } = string.Empty;
  public string FullName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string Status { get; set; } = UserStatusConstants.Active;
  public DateTime CreatedAt { get; set; }
  public DateTime? LastLogin { get; set; }
  public string LastLoginLangCode { get; set; } = string.Empty;
}
