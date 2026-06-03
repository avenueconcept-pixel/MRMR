namespace MyApp.Constants;

public static class CookieConstants
{
  public const string Language = "lang";
  public const string AdminAuth = "AdminAuth";
  public const string CustomerAuth = "CustomerAuth";

  public static class SessionKeys
  {
    public const string UserId        = "UserId";
    public const string Username      = "Username";
    public const string LoginLanguage = "LoginLanguage";
    public const string FullName      = "FullName";
    public const string Timezone      = "Timezone";
    public const string RoleId                 = "RoleId";
    public const string IsSuperAdmin           = "IsSuperAdmin";
    public const string IsForceChangePassword  = "IsForceChangePassword";
  }
}
