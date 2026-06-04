using Microsoft.Extensions.Logging;
using Npgsql;

namespace MyApp.Services.Logging;

public class DbLogger : ILogger
{
  private readonly string _connectionString;
  private readonly string _category;

  public DbLogger(string connectionString, string category)
  {
    _connectionString = connectionString;
    _category = category;
  }

  public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

  public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
      Func<TState, Exception?, string> formatter)
  {
    if (!IsEnabled(logLevel)) return;
    if (_category.StartsWith("Microsoft.") || _category.StartsWith("System.") || _category.StartsWith("Npgsql."))
      return;

    var message = formatter(state, exception);
    var exceptionText = exception?.ToString();

    try
    {
      using var conn = new NpgsqlConnection(_connectionString);
      conn.Open();
      using var cmd = new NpgsqlCommand(
          "INSERT INTO app_logs (log_level, category, message, exception, created_at) VALUES (@level, @category, @message, @exception, @created_at)",
          conn);
      cmd.Parameters.AddWithValue("@level", logLevel.ToString());
      cmd.Parameters.AddWithValue("@category", _category);
      cmd.Parameters.AddWithValue("@message", message);
      cmd.Parameters.AddWithValue("@exception", (object?)exceptionText ?? DBNull.Value);
      cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
      cmd.ExecuteNonQuery();
    }
    catch
    {
      // Never let the logger crash the app
    }
  }
}
