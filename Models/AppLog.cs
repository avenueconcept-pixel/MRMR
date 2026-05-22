namespace MyApp.Models;

public class AppLog
{
  public int Id { get; set; }
  public string LogLevel { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public string? Exception { get; set; }
  public DateTime CreatedAt { get; set; }
}
