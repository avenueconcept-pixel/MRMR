using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class LogDbHelper : DbHelper
{
  public LogDbHelper(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

  public async Task<List<AppLog>> GetRecentAsync(int count = 500)
      => await ExecuteAsync(() => _db.AppLogs
          .OrderByDescending(l => l.CreatedAt)
          .Take(count)
          .ToListAsync());

  public async Task<List<AppLog>> GetByFilterAsync(DateTime dateFromUtc, DateTime dateToUtc, string? logLevel = null)
      => await ExecuteAsync(async () =>
      {
        var query = _db.AppLogs
            .Where(l => l.CreatedAt >= dateFromUtc && l.CreatedAt <= dateToUtc);

        if (!string.IsNullOrEmpty(logLevel))
          query = query.Where(l => l.LogLevel == logLevel);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(1000)
            .ToListAsync();
      });

  public async Task<List<string>> GetDistinctLogLevelsAsync()
      => await ExecuteAsync(() => _db.AppLogs
          .Select(l => l.LogLevel)
          .Distinct()
          .OrderBy(l => l)
          .ToListAsync());
}
