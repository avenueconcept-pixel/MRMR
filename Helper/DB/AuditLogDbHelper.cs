using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class AuditLogDbHelper
{
  private readonly AuditDbContext           _auditDb;
  private readonly ILogger<AuditLogDbHelper> _logger;

  public AuditLogDbHelper(AuditDbContext auditDb, ILogger<AuditLogDbHelper> logger)
  {
    _auditDb = auditDb;
    _logger  = logger;
  }

  public async Task<List<AuditLog>> GetByFilterAsync(
    DateTime dateFromUtc,
    DateTime dateToUtc,
    string?  tableName = null,
    string?  action    = null,
    string?  changedBy = null)
  {
    try
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
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "AuditLogDbHelper error in {Method}", nameof(GetByFilterAsync));
      throw;
    }
  }

  public async Task<List<AuditLog>> GetByRecordAsync(string tableName, string recordId)
  {
    try
    {
      return await _auditDb.AuditLogs
          .Where(l => l.TableName == tableName && l.RecordId == recordId)
          .OrderByDescending(l => l.ChangedAt)
          .ToListAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "AuditLogDbHelper error in {Method}", nameof(GetByRecordAsync));
      throw;
    }
  }

  public async Task<List<string>> GetDistinctTableNamesAsync()
  {
    try
    {
      return await _auditDb.AuditLogs
          .Select(l => l.TableName)
          .Distinct()
          .OrderBy(l => l)
          .ToListAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "AuditLogDbHelper error in {Method}", nameof(GetDistinctTableNamesAsync));
      throw;
    }
  }
}
