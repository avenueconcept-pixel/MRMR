using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class MenuDbHelper : DbHelper
{
  public MenuDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<Menu>> GetAllAsync()
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Menus
            .Where(m => m.Status != StatusConstants.Deleted && m.ParentId == null)
            .Include(m => m.Children.Where(c => c.Status != StatusConstants.Deleted))
                .ThenInclude(c => c.Children.Where(gc => gc.Status != StatusConstants.Deleted))
            .Include(m => m.Permissions.Where(p => p.Status != StatusConstants.Deleted))
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
        return items;
      });

  public async Task<List<Menu>> GetAllActiveAsync()
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Menus
            .Where(m => m.Status == StatusConstants.Active && m.ParentId == null)
            .Include(m => m.Children.Where(c => c.Status == StatusConstants.Active))
                .ThenInclude(c => c.Children.Where(gc => gc.Status == StatusConstants.Active))
            .Include(m => m.Permissions.Where(p => p.Status == StatusConstants.Active))
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
        return items;
      });
}
