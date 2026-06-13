using MyApp.Constants;

namespace MyApp.Models;

public class AdminUser
{
  public int      Id              { get; set; }
  public string   Username        { get; set; } = string.Empty;
  public string   PasswordHash    { get; set; } = string.Empty;
  public string   FullName        { get; set; } = string.Empty;
  public string   Email           { get; set; } = string.Empty;
  public int      RoleId          { get; set; }
  public string   CountryCode     { get; set; } = string.Empty;
  public string?  MobileCountryCode     { get; set; }
  public string?  MobileNo              { get; set; }
  public bool     IsForceChangePassword { get; set; } = false;
  public string?  ProfileImage          { get; set; }
  public string   Status                { get; set; } = StatusConstants.Active;
  public DateTime? LastLoginAt          { get; set; }
  public string   LastLoginLang         { get; set; } = string.Empty;
  public DateTime CreatedAt       { get; set; }
  public string   CreatedBy       { get; set; } = string.Empty;
  public DateTime UpdatedAt       { get; set; }
  public string   UpdatedBy       { get; set; } = string.Empty;

  public Role?       Role       { get; set; }
  public Country?    Country    { get; set; }
}
