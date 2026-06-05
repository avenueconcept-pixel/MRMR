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
        // Apply all filters at DB level before loading — never load full table into memory
        var mainQuery = _auditDb.PageAccessHistories.AsNoTracking()
            .Where(p =>
                (string.IsNullOrEmpty(username)       || p.Username.Contains(username)) &&
                (string.IsNullOrEmpty(systemType)     || p.SystemType == systemType) &&
                (string.IsNullOrEmpty(pageUrlKeyword) || p.PageUrl.Contains(pageUrlKeyword)) &&
                (startDate == null                    || p.AccessedAt >= startDate) &&
                (endDate   == null                    || p.AccessedAt <= endDate));

        var archiveQuery = _auditDb.PageAccessHistoryArchives.AsNoTracking()
            .Where(a =>
                (string.IsNullOrEmpty(username)       || a.Username.Contains(username)) &&
                (string.IsNullOrEmpty(systemType)     || a.SystemType == systemType) &&
                (string.IsNullOrEmpty(pageUrlKeyword) || a.PageUrl.Contains(pageUrlKeyword)) &&
                (startDate == null                    || a.AccessedAt >= startDate) &&
                (endDate   == null                    || a.AccessedAt <= endDate));

        var mainCount    = await mainQuery.CountAsync();
        var archiveCount = await archiveQuery.CountAsync();
        var totalCount   = mainCount + archiveCount;

        var skip = (page - 1) * pageSize;

        List<PageAccessHistory> pagedItems;

        if (skip < mainCount)
        {
          // Current page starts within main table — take from main first, fill remainder from archive
          var mainItems = await mainQuery
              .OrderByDescending(p => p.AccessedAt)
              .Skip(skip)
              .Take(pageSize)
              .ToListAsync();

          var remaining = pageSize - mainItems.Count;
          if (remaining > 0)
          {
            var archiveRaw = await archiveQuery
                .OrderByDescending(a => a.AccessedAt)
                .Take(remaining)
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

            pagedItems = mainItems.Concat(archiveItems)
                .OrderByDescending(p => p.AccessedAt)
                .ToList();
          }
          else
          {
            pagedItems = mainItems;
          }
        }
        else
        {
          // Current page is entirely within archive table
          var archiveSkip = skip - mainCount;
          var archiveRaw  = await archiveQuery
              .OrderByDescending(a => a.AccessedAt)
              .Skip(archiveSkip)
              .Take(pageSize)
              .ToListAsync();

          pagedItems = archiveRaw.Select(a => new PageAccessHistory
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
        }

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
