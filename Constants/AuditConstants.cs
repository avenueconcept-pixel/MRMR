namespace MyApp.Constants;

public static class AuditConstants
{
  public static class Actions
  {
    public const string Insert  = "INSERT";
    public const string Update  = "UPDATE";
    public const string Delete  = "DELETE";
    public const string Restore = "RESTORE";
    public const string Login   = "LOGIN";
    public const string Logout  = "LOGOUT";
  }

  public static readonly HashSet<string> ExcludedFields = new()
  {
    "UpdatedAt",
    "UpdatedBy",
    "CreatedAt",
    "CreatedBy"
  };
}
