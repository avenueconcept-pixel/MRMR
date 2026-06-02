using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class MenuDbHelper : DbHelper
{
  public MenuDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  // Flat list for tree building on Index page (all non-deleted)
  public async Task<List<Menu>> GetFlatListAsync()
      => await ExecuteAsync(() => _db.Menus
          .Where(m => m.Status != StatusConstants.Deleted)
          .OrderBy(m => m.Level).ThenBy(m => m.SortOrder)
          .ToListAsync());

  // Top-level with nested children — used by Roles create/edit
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

  // Active only, hierarchy filtered by role — used by sidebar nav
  // Uses flat load + in-memory tree to avoid EF Core filtered-include issues on self-referencing entities
  public async Task<List<Menu>> GetNavMenuAsync(int roleId, bool isSuperAdmin)
      => await ExecuteAsync(async () =>
      {
        var all = await _db.Menus
            .AsNoTracking()
            .Where(m => m.Status == StatusConstants.Active)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

        HashSet<int> permittedIds;
        if (isSuperAdmin)
        {
          permittedIds = all.Select(m => m.Id).ToHashSet();
        }
        else
        {
          var ids = await _db.RoleMenus
              .Where(rm => rm.RoleId == roleId)
              .Select(rm => rm.MenuId)
              .ToListAsync();
          permittedIds = ids.ToHashSet();
        }

        var lookup = all.ToDictionary(m => m.Id);
        var roots  = new List<Menu>();

        foreach (var m in all)
        {
          if (m.ParentId == null)
            roots.Add(m);
          else if (lookup.TryGetValue(m.ParentId.Value, out var parent))
            ((List<Menu>)parent.Children).Add(m);
        }

        if (!isSuperAdmin)
          PruneTree(roots, permittedIds);

        return roots;
      });

  // Removes branches that have no permitted menu in them
  private static void PruneTree(List<Menu> nodes, HashSet<int> permittedIds)
  {
    nodes.RemoveAll(n =>
    {
      PruneTree((List<Menu>)n.Children, permittedIds);
      return !permittedIds.Contains(n.Id) && !n.Children.Any();
    });
  }

  // Active only, nested — used by Roles create/edit (permission assignment)
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

  public async Task<Menu?> GetByIdAsync(int id)
      => await ExecuteAsync(async () => await _db.Menus.FindAsync(id));

  public async Task<Menu?> GetByCodeAsync(string menuCode)
      => await ExecuteAsync(() => _db.Menus
          .FirstOrDefaultAsync(m => m.MenuCode.ToUpper() == menuCode.ToUpper()));

  // Active level=1 (parent) menus — for dropdowns
  public async Task<List<Menu>> GetParentsAsync()
      => await ExecuteAsync(() => _db.Menus
          .Where(m => m.Level == 1 && m.Status == StatusConstants.Active)
          .OrderBy(m => m.SortOrder).ThenBy(m => m.MenuName)
          .ToListAsync());

  // Active level=0 (group) menus — for dropdowns
  public async Task<List<Menu>> GetGroupsAsync()
      => await ExecuteAsync(() => _db.Menus
          .Where(m => m.Level == 0 && m.Status == StatusConstants.Active)
          .OrderBy(m => m.SortOrder).ThenBy(m => m.MenuName)
          .ToListAsync());

  public async Task<bool> HasActiveChildrenAsync(int id)
      => await ExecuteAsync(() => _db.Menus
          .AnyAsync(m => m.ParentId == id && m.Status != StatusConstants.Deleted));

  public async Task<MenuAddResult> CreateAsync(Menu menu, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Menus
            .FirstOrDefaultAsync(m => m.MenuCode.ToUpper() == menu.MenuCode.ToUpper());

        if (existing != null && existing.Status == StatusConstants.Deleted)
        {
          existing.MenuName  = menu.MenuName;
          existing.MenuIcon  = menu.MenuIcon;
          existing.MenuUrl   = menu.MenuUrl;
          existing.ParentId  = menu.ParentId;
          existing.Level     = menu.Level;
          existing.Status    = StatusConstants.Active;
          existing.UpdatedAt = DateTime.Now;
          existing.UpdatedBy = createdBy;
          await _db.SaveChangesAsync();
          return MenuAddResult.Restored;
        }

        if (existing != null) return MenuAddResult.DuplicateActive;

        var maxSort = await _db.Menus
            .Where(m => m.Level == menu.Level)
            .MaxAsync(m => (int?)m.SortOrder) ?? 0;

        menu.SortOrder = maxSort + 1;
        menu.CreatedAt = DateTime.Now;
        menu.CreatedBy = createdBy;
        menu.UpdatedAt = DateTime.Now;
        menu.UpdatedBy = createdBy;

        _db.Menus.Add(menu);
        await _db.SaveChangesAsync();
        return MenuAddResult.Created;
      });

  public async Task UpdateAsync(Menu menu, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Menus.FindAsync(menu.Id)
            ?? throw new InvalidOperationException($"Menu {menu.Id} not found");

        existing.MenuName  = menu.MenuName;
        existing.MenuIcon  = menu.MenuIcon;
        existing.MenuUrl   = menu.MenuUrl;
        existing.ParentId  = menu.ParentId;
        existing.Level     = menu.Level;
        existing.Status    = menu.Status;
        existing.UpdatedAt = DateTime.Now;
        existing.UpdatedBy = updatedBy;

        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var menu = await _db.Menus.FindAsync(id)
            ?? throw new InvalidOperationException($"Menu {id} not found");

        menu.Status    = status;
        menu.UpdatedAt = DateTime.Now;
        menu.UpdatedBy = updatedBy;

        await _db.SaveChangesAsync();
      });

  public async Task SaveSortOrderAsync(List<MenuSortItem> items, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var ids   = items.Select(i => i.Id).ToList();
        var menus = await _db.Menus.Where(m => ids.Contains(m.Id)).ToListAsync();
        var now   = DateTime.Now;

        foreach (var item in items)
        {
          var m = menus.FirstOrDefault(x => x.Id == item.Id);
          if (m == null) continue;
          m.SortOrder = item.SortOrder;
          m.ParentId  = item.ParentId;
          m.Level     = item.Level;
          m.UpdatedAt = now;
          m.UpdatedBy = updatedBy;
        }

        await _db.SaveChangesAsync();
      });
}
