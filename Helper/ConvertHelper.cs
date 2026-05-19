namespace MyApp.Helpers;

public static class ConvertHelper
{
  /*
  // ─── Null to type
  ConvertHelper.ToString(null);               // ""
ConvertHelper.ToString(null, "N/A");        // "N/A"
ConvertHelper.ToInt(null);                  // 0
ConvertHelper.ToInt(null, 99);              // 99
ConvertHelper.ToDecimal(null);              // 0
ConvertHelper.ToBool(null);                 // false
ConvertHelper.ToDateTime(null);             // DateTime.MinValue

// ─── Null checks
ConvertHelper.IsNull(null);                 // true
ConvertHelper.IsNotNull("hello");           // true

// ─── Int
ConvertHelper.IntToString(1, "D5");         // "00001"
ConvertHelper.IntToDecimal(5);              // 5.0M
ConvertHelper.IntToDouble(5);               // 5.0

// ─── Double
ConvertHelper.DoubleToString(9.99, "F2");   // "9.99"
ConvertHelper.DoubleToDecimal(9.99);        // 9.99M
ConvertHelper.DoubleToInt(9.99);            // 10

// ─── Decimal
ConvertHelper.DecimalToString(9.99M, "C2"); // "RM 9.99"
ConvertHelper.DecimalToDouble(9.99M);       // 9.99
ConvertHelper.DecimalToInt(9.99M);          // 10

// ─── String
ConvertHelper.StringToInt("123");           // 123
ConvertHelper.StringToDecimal("9.99");      // 9.99M
ConvertHelper.StringToDateTime("2026-05-17"); // DateTime

// ─── DateTime
ConvertHelper.DateTimeToString(DateTime.Now);           // "2026-05-17 10:30:00"
ConvertHelper.DateTimeToString(DateTime.Now, "dd/MM/yyyy"); // "17/05/2026"
ConvertHelper.DateToString(DateTime.Now);               // "2026-05-17"
ConvertHelper.TimeToString(DateTime.Now);               // "10:30:00"

  */
  // ═════════════════════════════════════════════════
  // ─── Null to Type ─────────────────────────────────
  // ═════════════════════════════════════════════════

  // ─── To String ────────────────────────────────────

  public static string ToString(object? value)
      => value?.ToString() ?? string.Empty;

  public static string ToString(object? value, string defaultValue)
      => value?.ToString() ?? defaultValue;

  // ─── To Int ───────────────────────────────────────

  public static int ToInt(object? value)
      => int.TryParse(value?.ToString(), out var result) ? result : 0;

  public static int ToInt(object? value, int defaultValue)
      => int.TryParse(value?.ToString(), out var result) ? result : defaultValue;

  // ─── To Decimal ───────────────────────────────────

  public static decimal ToDecimal(object? value)
      => decimal.TryParse(value?.ToString(), out var result) ? result : 0;

  public static decimal ToDecimal(object? value, decimal defaultValue)
      => decimal.TryParse(value?.ToString(), out var result) ? result : defaultValue;

  // ─── To Double ────────────────────────────────────

  public static double ToDouble(object? value)
      => double.TryParse(value?.ToString(), out var result) ? result : 0;

  public static double ToDouble(object? value, double defaultValue)
      => double.TryParse(value?.ToString(), out var result) ? result : defaultValue;

  // ─── To Bool ──────────────────────────────────────

  public static bool ToBool(object? value)
      => bool.TryParse(value?.ToString(), out var result) && result;

  public static bool ToBool(object? value, bool defaultValue)
      => bool.TryParse(value?.ToString(), out var result) ? result : defaultValue;

  // ─── To DateTime ──────────────────────────────────

  public static DateTime ToDateTime(object? value)
      => DateTime.TryParse(value?.ToString(), out var result) ? result : DateTime.MinValue;

  public static DateTime ToDateTime(object? value, DateTime defaultValue)
      => DateTime.TryParse(value?.ToString(), out var result) ? result : defaultValue;

  // ─── To Long ──────────────────────────────────────

  public static long ToLong(object? value)
      => long.TryParse(value?.ToString(), out var result) ? result : 0;

  public static long ToLong(object? value, long defaultValue)
      => long.TryParse(value?.ToString(), out var result) ? result : defaultValue;

  // ─── To Float ─────────────────────────────────────

  public static float ToFloat(object? value)
      => float.TryParse(value?.ToString(), out var result) ? result : 0;

  public static float ToFloat(object? value, float defaultValue)
      => float.TryParse(value?.ToString(), out var result) ? result : defaultValue;


  // ═════════════════════════════════════════════════
  // ─── Null Checks ──────────────────────────────────
  // ═════════════════════════════════════════════════

  public static bool IsNull(object? value)
      => value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString());

  public static bool IsNotNull(object? value)
      => !IsNull(value);


  // ═════════════════════════════════════════════════
  // ─── Int Conversions ──────────────────────────────
  // ═════════════════════════════════════════════════

  public static string IntToString(int value)
      => value.ToString();

  public static string IntToString(int value, string format)
      => value.ToString(format);              // "D5" → "00001"

  public static double IntToDouble(int value)
      => Convert.ToDouble(value);

  public static decimal IntToDecimal(int value)
      => Convert.ToDecimal(value);

  public static long IntToLong(int value)
      => Convert.ToInt64(value);

  public static float IntToFloat(int value)
      => Convert.ToSingle(value);


  // ═════════════════════════════════════════════════
  // ─── Double Conversions ───────────────────────────
  // ═════════════════════════════════════════════════

  public static string DoubleToString(double value)
      => value.ToString();

  public static string DoubleToString(double value, string format)
      => value.ToString(format);              // "F2" → "9.99"

  public static int DoubleToInt(double value)
      => Convert.ToInt32(value);

  public static decimal DoubleToDecimal(double value)
      => Convert.ToDecimal(value);

  public static float DoubleToFloat(double value)
      => Convert.ToSingle(value);

  public static long DoubleToLong(double value)
      => Convert.ToInt64(value);


  // ═════════════════════════════════════════════════
  // ─── Decimal Conversions ──────────────────────────
  // ═════════════════════════════════════════════════

  public static string DecimalToString(decimal value)
      => value.ToString();

  public static string DecimalToString(decimal value, string format)
      => value.ToString(format);              // "C2" → "RM 9.99"

  public static int DecimalToInt(decimal value)
      => Convert.ToInt32(value);

  public static double DecimalToDouble(decimal value)
      => Convert.ToDouble(value);

  public static float DecimalToFloat(decimal value)
      => Convert.ToSingle(value);

  public static long DecimalToLong(decimal value)
      => Convert.ToInt64(value);


  // ═════════════════════════════════════════════════
  // ─── String Conversions ───────────────────────────
  // ═════════════════════════════════════════════════

  public static int StringToInt(string value)
      => int.TryParse(value, out var result) ? result : 0;

  public static int StringToInt(string value, int defaultValue)
      => int.TryParse(value, out var result) ? result : defaultValue;

  public static double StringToDouble(string value)
      => double.TryParse(value, out var result) ? result : 0;

  public static decimal StringToDecimal(string value)
      => decimal.TryParse(value, out var result) ? result : 0;

  public static bool StringToBool(string value)
      => bool.TryParse(value, out var result) && result;

  public static DateTime StringToDateTime(string value)
      => DateTime.TryParse(value, out var result) ? result : DateTime.MinValue;

  public static long StringToLong(string value)
      => long.TryParse(value, out var result) ? result : 0;

  public static float StringToFloat(string value)
      => float.TryParse(value, out var result) ? result : 0;


  // ═════════════════════════════════════════════════
  // ─── DateTime Conversions ─────────────────────────
  // ═════════════════════════════════════════════════

  public static string DateTimeToString(DateTime value)
      => value.ToString("yyyy-MM-dd HH:mm:ss");

  public static string DateTimeToString(DateTime value, string format)
      => value.ToString(format);              // "dd/MM/yyyy" → "17/05/2026"

  public static string DateToString(DateTime value)
      => value.ToString("yyyy-MM-dd");

  public static string TimeToString(DateTime value)
      => value.ToString("HH:mm:ss");
}
