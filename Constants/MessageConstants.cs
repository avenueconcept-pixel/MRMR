namespace MyApp.Constants;

public static class MessageConstants
{
  // ─── Success ──────────────────────────────────────
  public const string SaveSuccess = "MsgSaveSuccess";
  public const string DeleteSuccess = "MsgDeleteSuccess";
  public const string UpdateSuccess = "MsgUpdateSuccess";

  // ─── Error ────────────────────────────────────────
  public const string SaveError = "MsgSaveError";
  public const string DeleteError = "MsgDeleteError";
  public const string NotFound = "MsgNotFound";
  public const string AccessDenied = "MsgAccessDenied";

  // ─── Validation ───────────────────────────────────
  public const string RequiredField = "MsgRequiredField";
  public const string InvalidEmail = "MsgInvalidEmail";
  public const string InvalidPassword = "MsgInvalidPassword";

  // ─── Auth ─────────────────────────────────────────
  public const string LoginFailed = "MsgLoginFailed";
  public const string AccountLocked = "MsgAccountLocked";
  public const string SessionExpired = "MsgSessionExpired";

  public const string InvalidRouteId = "MsgInvalidRouteId";
  public const string InvalidDataAction = "MsgInvalidDataAction";

}

public static class MessageTitle
{
  public const string Error = "Oops...";
  public const string Success = "";
}

public static class MessageType
{
  public const string Success = "success";
  public const string Error = "error";
  public const string Warning = "warning";
}


