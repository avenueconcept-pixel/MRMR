using MyApp.Data;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace MyApp.Helper.DB;

public class DbHelper
{
  protected readonly AppDbContext _db;
  private readonly ILogger _logger;

  public DbHelper(AppDbContext db, ILoggerFactory loggerFactory)
  {
    _db = db;
    _logger = loggerFactory.CreateLogger(GetType());
  }

  protected async Task<T> ExecuteAsync<T>(
      Func<Task<T>> operation,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string filePath = "",
      [CallerLineNumber] int lineNumber = 0)
  {
    try
    {
      return await operation();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "DB error in {Method} ({File}:{Line})", caller, Path.GetFileName(filePath), lineNumber);
      throw;
    }
  }

  protected async Task ExecuteAsync(
      Func<Task> operation,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string filePath = "",
      [CallerLineNumber] int lineNumber = 0)
  {
    try
    {
      await operation();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "DB error in {Method} ({File}:{Line})", caller, Path.GetFileName(filePath), lineNumber);
      throw;
    }
  }
}
