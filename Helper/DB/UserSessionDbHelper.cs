using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using System.Runtime.CompilerServices;

namespace MyApp.Helper.DB;

public class UserSessionDbHelper
{
  private readonly AuditDbContext              _auditDb;
  private readonly ILogger<UserSessionDbHelper> _logger;

  public UserSessionDbHelper(AuditDbContext auditDb, ILoggerFactory loggerFactory)
  {
    _auditDb = auditDb;
    _logger  = loggerFactory.CreateLogger<UserSessionDbHelper>();
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

  public async Task CreateSessionAsync(UserSession session)
      => await ExecuteAsync(async () =>
      {
        _auditDb.UserSessions.Add(session);
        await _auditDb.SaveChangesAsync();
      });

  public async Task UpdateCurrentPageAsync(string sessionToken, string currentPage)
      => await ExecuteAsync(async () =>
      {
        var session = await _auditDb.UserSessions
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);
        if (session != null)
        {
          session.CurrentPage  = currentPage;
          session.LastActiveAt = DateTime.UtcNow;
          await _auditDb.SaveChangesAsync();
        }
      });

  public async Task EndSessionAsync(string sessionToken)
      => await ExecuteAsync(async () =>
      {
        var session = await _auditDb.UserSessions
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);
        if (session != null)
        {
          session.IsActive  = false;
          session.LogoutAt  = DateTime.UtcNow;
          await _auditDb.SaveChangesAsync();
        }
      });

  public async Task<List<UserSession>> GetActiveSessionsAsync()
      => await ExecuteAsync(async () =>
      {
        var cutoff = DateTime.UtcNow.AddMinutes(-30);
        return await _auditDb.UserSessions
            .Where(s => s.IsActive && s.LastActiveAt >= cutoff)
            .OrderByDescending(s => s.LastActiveAt)
            .ToListAsync();
      });

  public async Task<Dictionary<string, int>> GetActiveSessionsCountBySystemAsync()
      => await ExecuteAsync(async () =>
      {
        var cutoff = DateTime.UtcNow.AddMinutes(-30);
        return await _auditDb.UserSessions
            .Where(s => s.IsActive && s.LastActiveAt >= cutoff)
            .GroupBy(s => s.SystemType)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
      });

  public async Task<List<UserSession>> GetRecentLoginsAsync(int count = 20)
      => await ExecuteAsync(async () => await _auditDb.UserSessions
          .OrderByDescending(s => s.LoginAt)
          .Take(count)
          .ToListAsync());

  public async Task CleanupIdleSessionsAsync()
      => await ExecuteAsync(async () =>
      {
        var cutoff = DateTime.UtcNow.AddMinutes(-30);
        var idle   = await _auditDb.UserSessions
            .Where(s => s.IsActive && s.LastActiveAt < cutoff)
            .ToListAsync();
        foreach (var s in idle)
        {
          s.IsActive = false;
          s.LogoutAt = DateTime.UtcNow;
        }
        if (idle.Count > 0)
          await _auditDb.SaveChangesAsync();
      });
}
