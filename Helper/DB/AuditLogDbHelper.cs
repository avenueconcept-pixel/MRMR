using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using System.Runtime.CompilerServices;

namespace MyApp.Helper.DB;

public class AuditLogDbHelper
{
  private readonly AuditDbContext            _auditDb;
  private readonly ILogger<AuditLogDbHelper> _logger;

  public AuditLogDbHelper(AuditDbContext auditDb, ILoggerFactory loggerFactory)
  {
    _auditDb = auditDb;
    _logger  = loggerFactory.CreateLogger<AuditLogDbHelper>();
  }

  private async Task<T> ExecuteAsync<T>(
      Func<Task<T>> operation,
      [CallerMemberName] string caller     = "",
      [CallerFilePath]   string filePath   = "",
      [CallerLineNumber] int    lineNumber = 0)
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

  private async Task ExecuteAsync(
      Func<Task> operation,
      [CallerMemberName] string caller     = "",
      [CallerFilePath]   string filePath   = "",
      [CallerLineNumber] int    lineNumber = 0)
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

  public async Task<List<AuditLog>> GetByFilterAsync(
      DateTime dateFromUtc,
      DateTime dateToUtc,
      string?  tableName = null,
      string?  action    = null,
      string?  changedBy = null)
      => await ExecuteAsync(async () =>
      {
        var query = _auditDb.AuditLogs
            .Where(l => l.ChangedAt >= dateFromUtc && l.ChangedAt <= dateToUtc);

        if (!string.IsNullOrEmpty(tableName))
          query = query.Where(l => l.TableName == tableName);

        if (!string.IsNullOrEmpty(action))
          query = query.Where(l => l.Action == action);

        if (!string.IsNullOrEmpty(changedBy))
          query = query.Where(l => l.ChangedBy.Contains(changedBy));

        return await query
            .OrderByDescending(l => l.ChangedAt)
            .Take(1000)
            .ToListAsync();
      });

  public async Task<List<AuditLog>> GetByRecordAsync(string tableName, string recordId)
      => await ExecuteAsync(() => _auditDb.AuditLogs
          .Where(l => l.TableName == tableName && l.RecordId == recordId)
          .OrderByDescending(l => l.ChangedAt)
          .ToListAsync());

  public async Task<List<string>> GetDistinctTableNamesAsync()
      => await ExecuteAsync(() => _auditDb.AuditLogs
          .Select(l => l.TableName)
          .Distinct()
          .OrderBy(l => l)
          .ToListAsync());
}
