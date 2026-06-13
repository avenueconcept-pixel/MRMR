namespace MyApp.Models;

public class UserSession
{
  public int       Id           { get; set; }
  public string    SystemType   { get; set; } = string.Empty;
  public int       UserId       { get; set; }
  public string    Username     { get; set; } = string.Empty;
  public string?   FullName     { get; set; }
  public string?   CountryCode  { get; set; }
  public string    SessionToken { get; set; } = string.Empty;
  public string?   IpAddress    { get; set; }
  public string?   Browser      { get; set; }
  public string?   Os           { get; set; }
  public string?   DeviceType   { get; set; }
  public string?   CurrentPage  { get; set; }
  public DateTime  LastActiveAt { get; set; }
  public DateTime  LoginAt      { get; set; }
  public DateTime? LogoutAt     { get; set; }
  public bool      IsActive     { get; set; } = true;
}
