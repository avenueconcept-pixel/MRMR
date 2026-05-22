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
}
