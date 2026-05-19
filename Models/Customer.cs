
namespace MyApp.Models;

public class Customer
{
  public int Id { get; set; }
  public string FullName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string PasswordHash { get; set; } = string.Empty;
  public string? Phone { get; set; }
  public string? Address { get; set; }
  public bool IsActive { get; set; } = true;
  public DateTime RegisteredAt { get; set; }
  public DateTime? LastLogin { get; set; }
  public string LanguageCode { get; set; } = "en";
}
