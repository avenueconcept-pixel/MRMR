using Microsoft.Extensions.Logging;

namespace MyApp.Services.Logging;

public class DbLoggerProvider : ILoggerProvider
{
  private readonly string _connectionString;

  public DbLoggerProvider(string connectionString)
  {
    _connectionString = connectionString;
  }

  public ILogger CreateLogger(string categoryName)
      => new DbLogger(_connectionString, categoryName);

  public void Dispose() { }
}
