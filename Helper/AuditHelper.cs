using Microsoft.Extensions.Logging;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;
using System.Reflection;

namespace MyApp.Helper;

public class AuditHelper
{
  private readonly AuditDbContext       _auditDb;
  private readonly ILogger<AuditHelper> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public AuditHelper(
    AuditDbContext auditDb,
    ILogger<AuditHelper> logger,
    IHttpContextAccessor httpContextAccessor)
  {
    _auditDb             = auditDb;
    _logger              = logger;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task LogInsertAsync<T>(
    string tableName,
    string recordId,
    T newObject,
    string changedBy)
  {
    try
    {
      var logs  = new List<AuditLog>();
      var props = GetAuditableProperties<T>();

      foreach (var prop in props)
      {
        logs.Add(new AuditLog
        {
          TableName = tableName,
          RecordId  = recordId,
          Action    = AuditConstants.Actions.Insert,
          FieldName = prop.Name,
          OldValue  = null,
          NewValue  = prop.GetValue(newObject)?.ToString(),
          ChangedBy = changedBy,
          ChangedAt = DateTime.UtcNow,
          IpAddress = GetIpAddress()
        });
      }

      if (logs.Any())
      {
        _auditDb.AuditLogs.AddRange(logs);
        await _auditDb.SaveChangesAsync();
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Audit INSERT failed for {Table} {RecordId}", tableName, recordId);
    }
  }

  public async Task LogUpdateAsync<T>(
    string tableName,
    string recordId,
    T oldObject,
    T newObject,
    string changedBy)
  {
    try
    {
      var logs  = new List<AuditLog>();
      var props = GetAuditableProperties<T>();

      foreach (var prop in props)
      {
        var oldVal = prop.GetValue(oldObject)?.ToString();
        var newVal = prop.GetValue(newObject)?.ToString();

        if (oldVal == newVal) continue;

        logs.Add(new AuditLog
        {
          TableName = tableName,
          RecordId  = recordId,
          Action    = AuditConstants.Actions.Update,
          FieldName = prop.Name,
          OldValue  = oldVal,
          NewValue  = newVal,
          ChangedBy = changedBy,
          ChangedAt = DateTime.UtcNow,
          IpAddress = GetIpAddress()
        });
      }

      if (logs.Any())
      {
        _auditDb.AuditLogs.AddRange(logs);
        await _auditDb.SaveChangesAsync();
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Audit UPDATE failed for {Table} {RecordId}", tableName, recordId);
    }
  }

  public async Task LogActionAsync(
    string tableName,
    string recordId,
    string action,
    string changedBy,
    string? remarks = null)
  {
    try
    {
      _auditDb.AuditLogs.Add(new AuditLog
      {
        TableName = tableName,
        RecordId  = recordId,
        Action    = action,
        ChangedBy = changedBy,
        ChangedAt = DateTime.UtcNow,
        IpAddress = GetIpAddress(),
        Remarks   = remarks
      });
      await _auditDb.SaveChangesAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Audit {Action} failed for {Table} {RecordId}", action, tableName, recordId);
    }
  }

  public async Task LogLoginAsync(
    string username,
    string action,
    string? remarks = null)
  {
    try
    {
      _auditDb.AuditLogs.Add(new AuditLog
      {
        TableName = "admin_users",
        RecordId  = username,
        Action    = action,
        ChangedBy = username,
        ChangedAt = DateTime.UtcNow,
        IpAddress = GetIpAddress(),
        Remarks   = remarks
      });
      await _auditDb.SaveChangesAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Audit {Action} failed for {Username}", action, username);
    }
  }

  private static IEnumerable<PropertyInfo> GetAuditableProperties<T>()
    => typeof(T).GetProperties()
        .Where(p =>
          !AuditConstants.ExcludedFields.Contains(p.Name) &&
          (p.PropertyType.IsPrimitive ||
           p.PropertyType == typeof(string) ||
           p.PropertyType == typeof(decimal) ||
           p.PropertyType == typeof(DateTime) ||
           p.PropertyType == typeof(Guid)));

  private string? GetIpAddress()
    => _httpContextAccessor.HttpContext?
        .Connection.RemoteIpAddress?.ToString();
}
