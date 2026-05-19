namespace MyApp.Constants;

public static class AppConstants
{
  // ─── App Info ─────────────────────────────────────
  public const string AppName = "MyApp";
  public const string AppVersion = "1.0.0";
  public const string AppEmail = "admin@myapp.com";

  // ─── Default Values ───────────────────────────────
  public const string DefaultLanguage = "en";
  public const int DefaultPageSize = 10;
  public const int MaxPageSize = 100;

  // ─── Date Format ──────────────────────────────────
  // ─── Display Format (for showing dates on screen) ─
  public const string DateFormat = "dd/MM/yyyy";
  public const string DateTimeFormat = "dd/MM/yyyy HH:mm:ss";
  public const string TimeFormat = "HH:mm:ss";

  // ─── Input Format (for HTML date input) ───────────
  public const string DateInputFormat = "yyyy-MM-dd";      // ← matches browser
  public const string DateTimeInputFormat = "yyyy-MM-dd HH:mm"; // ← matches browser

  // ─── File Upload ──────────────────────────────────
  public const int MaxFileSizeMB = 10;
  public const string AllowedImageTypes = ".jpg,.jpeg,.png,.gif";
  public const string AllowedDocTypes = ".pdf,.doc,.docx,.xls,.xlsx";

  // ─── Pagination ───────────────────────────────────
  public const int DefaultPage = 1;
}
