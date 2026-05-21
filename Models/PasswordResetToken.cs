namespace MyApp.Models;

public class PasswordResetToken
{
  public int Id { get; set; }
  public string UserType { get; set; } = string.Empty;
  public int UserId { get; set; }
  public string Token { get; set; } = string.Empty;
  public DateTime ExpiresAt { get; set; }
  public bool IsUsed { get; set; } = false;
  public DateTime CreatedAt { get; set; }
}
