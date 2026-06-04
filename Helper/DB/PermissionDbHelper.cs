using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class PermissionDbHelper : DbHelper
{
  public PermissionDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<Permission>> GetAllAsync()
      => await ExecuteAsync(() => _db.Permissions
          .Where(p => p.Status != StatusConstants.Deleted)
          .Include(p => p.Menu)
          .OrderBy(p => p.Module).ThenBy(p => p.SortOrder)
          .ToListAsync());

  public async Task<List<Permission>> GetActiveByModuleAsync(string module)
      => await ExecuteAsync(() => _db.Permissions
          .Where(p => p.Module == module && p.Status == StatusConstants.Active)
          .OrderBy(p => p.SortOrder)
          .ToListAsync());
}
