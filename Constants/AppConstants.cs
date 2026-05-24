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

public static class DateTimeExtensions
{
  public static string ToUserLocalTime(this DateTime utcDateTime, string timezoneId, string format)
  {
    try
    {
      var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
      return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz).ToString(format);
    }
    catch
    {
      return utcDateTime.ToString(format);
    }
  }

  public static DateTime ToUtcFromUserTimezone(this DateTime localDateTime, string timezoneId)
  {
    try
    {
      var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
      return TimeZoneInfo.ConvertTimeToUtc(
          DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified), tz);
    }
    catch
    {
      return localDateTime;
    }
  }
}

public static class YesNo
{
  public const string Yes = "Y";
  public const string No = "N";

}



public static class UploadWebPath
{

  public const string Product = "uploads/product";


}

