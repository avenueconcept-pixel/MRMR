using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using System.Runtime.CompilerServices;

namespace MyApp.Helper.DB;

public class PageAccessDbHelper
{
  private readonly AuditDbContext             _auditDb;
  private readonly ILogger<PageAccessDbHelper> _logger;

  public PageAccessDbHelper(AuditDbContext auditDb, ILoggerFactory loggerFactory)
  {
    _auditDb = auditDb;
    _logger  = loggerFactory.CreateLogger<PageAccessDbHelper>();
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

  public async Task LogAsync(PageAccessHistory record)
      => await ExecuteAsync(async () =>
      {
        _auditDb.PageAccessHistories.Add(record);
        await _auditDb.SaveChangesAsync();
      });

  public async Task<(List<PageAccessHistory> Items, int TotalCount)> GetPagedAsync(
      string?   username,
      string?   systemType,
      string?   pageUrlKeyword,
      DateTime? startDate,
      DateTime? endDate,
      int       page,
      int       pageSize)
      => await ExecuteAsync(async () =>
      {
        var mainItems = await _auditDb.PageAccessHistories
            .Where(p =>
                (string.IsNullOrEmpty(username)       || p.Username.Contains(username)) &&
                (string.IsNullOrEmpty(systemType)     || p.SystemType == systemType) &&
                (string.IsNullOrEmpty(pageUrlKeyword) || p.PageUrl.Contains(pageUrlKeyword)) &&
                (startDate == null                    || p.AccessedAt >= startDate) &&
                (endDate   == null                    || p.AccessedAt <= endDate))
            .AsNoTracking()
            .ToListAsync();

        var archiveRaw = await _auditDb.PageAccessHistoryArchives
            .Where(a =>
                (string.IsNullOrEmpty(username)       || a.Username.Contains(username)) &&
                (string.IsNullOrEmpty(systemType)     || a.SystemType == systemType) &&
                (string.IsNullOrEmpty(pageUrlKeyword) || a.PageUrl.Contains(pageUrlKeyword)) &&
                (startDate == null                    || a.AccessedAt >= startDate) &&
                (endDate   == null                    || a.AccessedAt <= endDate))
            .AsNoTracking()
            .ToListAsync();

        var archiveItems = archiveRaw.Select(a => new PageAccessHistory
        {
          Id           = a.Id,
          SystemType   = a.SystemType,
          UserId       = a.UserId,
          Username     = a.Username,
          FullName     = a.FullName,
          SessionToken = a.SessionToken,
          PageUrl      = a.PageUrl,
          HttpMethod   = a.HttpMethod,
          QueryString  = a.QueryString,
          ResponseTime = a.ResponseTime,
          AccessedAt   = a.AccessedAt
        }).ToList();

        var combined = mainItems.Concat(archiveItems)
            .OrderByDescending(p => p.AccessedAt)
            .ToList();

        var totalCount = combined.Count;
        var pagedItems = combined
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedItems, totalCount);
      });

  public async Task ArchiveOldRecordsAsync()
      => await ExecuteAsync(async () =>
      {
        var cutoff     = DateTime.UtcNow.AddDays(-90);
        var archivedAt = DateTime.UtcNow;

        var toArchive = await _auditDb.PageAccessHistories
            .Where(p => p.AccessedAt < cutoff)
            .AsNoTracking()
            .ToListAsync();

        if (toArchive.Count == 0)
          return;

        await using var tx = await _auditDb.Database.BeginTransactionAsync();

        var archiveRows = toArchive.Select(p => new PageAccessHistoryArchive
        {
          Id           = p.Id,
          SystemType   = p.SystemType,
          UserId       = p.UserId,
          Username     = p.Username,
          FullName     = p.FullName,
          SessionToken = p.SessionToken,
          PageUrl      = p.PageUrl,
          HttpMethod   = p.HttpMethod,
          QueryString  = p.QueryString,
          ResponseTime = p.ResponseTime,
          AccessedAt   = p.AccessedAt,
          ArchivedAt   = archivedAt
        }).ToList();

        await _auditDb.PageAccessHistoryArchives.AddRangeAsync(archiveRows);
        await _auditDb.SaveChangesAsync();

        var ids = toArchive.Select(p => p.Id).ToList();
        var toDelete = await _auditDb.PageAccessHistories
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
        _auditDb.PageAccessHistories.RemoveRange(toDelete);
        await _auditDb.SaveChangesAsync();

        await tx.CommitAsync();
      });
}
