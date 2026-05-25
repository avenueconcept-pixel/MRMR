using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class RoleDbHelper : DbHelper
{
  public RoleDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<Role>> GetAllAsync()
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Roles
            .Where(r => r.Status != StatusConstants.Deleted)
            .ToListAsync();
        return items.OrderBy(r => r.RoleName).ToList();
      });

  public async Task<List<Role>> GetAllActiveAsync()
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Roles
            .Where(r => r.Status == StatusConstants.Active)
            .ToListAsync();
        return items.OrderBy(r => r.RoleName).ToList();
      });

  public async Task<Role?> GetByIdAsync(int id)
      => await ExecuteAsync(() => _db.Roles
          .Include(r => r.RoleMenus)
          .Include(r => r.RolePermissions)
          .FirstOrDefaultAsync(r => r.Id == id && r.Status != StatusConstants.Deleted));

  public async Task<bool> IsRoleCodeExistsAsync(string roleCode, int excludeId = 0)
      => await ExecuteAsync(() => _db.Roles
          .AnyAsync(r => r.RoleCode == roleCode && r.Id != excludeId && r.Status != StatusConstants.Deleted));

  public async Task<RoleAddResult> CreateAsync(
      Role role,
      List<int> menuIds,
      List<(int PermissionId, bool IsGranted)> permissions,
      string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Roles.FirstOrDefaultAsync(r => r.RoleCode == role.RoleCode);

        if (existing != null && existing.Status == StatusConstants.Deleted)
        {
          existing.RoleName     = role.RoleName;
          existing.Description  = role.Description;
          existing.IsSuperAdmin = role.IsSuperAdmin;
          existing.DataScope    = role.DataScope;
          existing.Status       = StatusConstants.Active;
          existing.UpdatedAt    = DateTime.UtcNow;
          existing.UpdatedBy    = createdBy;

          await SyncMenusAsync(existing.Id, menuIds);
          await SyncPermissionsAsync(existing.Id, permissions);
          await _db.SaveChangesAsync();
          await _audit.LogActionAsync("roles", existing.Id.ToString(), AuditConstants.Actions.Restore, createdBy, remarks: "Restored from deleted");
          return RoleAddResult.Restored;
        }

        if (existing != null)
          return RoleAddResult.DuplicateActive;

        role.CreatedAt = DateTime.UtcNow;
        role.CreatedBy = createdBy;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = createdBy;

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        await SyncMenusAsync(role.Id, menuIds);
        await SyncPermissionsAsync(role.Id, permissions);
        await _db.SaveChangesAsync();

        await _audit.LogInsertAsync("roles", role.Id.ToString(), role, createdBy);
        return RoleAddResult.Created;
      });

  public async Task UpdateAsync(
      Role role,
      List<int> menuIds,
      List<(int PermissionId, bool IsGranted)> permissions,
      string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Roles.FindAsync(role.Id);
        if (existing == null) return;

        var old = new Role
        {
          Id           = existing.Id,
          RoleCode     = existing.RoleCode,
          RoleName     = existing.RoleName,
          IsSuperAdmin = existing.IsSuperAdmin,
          DataScope    = existing.DataScope,
          Status       = existing.Status
        };

        existing.RoleName     = role.RoleName;
        existing.Description  = role.Description;
        existing.IsSuperAdmin = role.IsSuperAdmin;
        existing.DataScope    = role.DataScope;
        existing.Status       = role.Status;
        existing.UpdatedAt    = DateTime.UtcNow;
        existing.UpdatedBy    = updatedBy;

        await SyncMenusAsync(role.Id, menuIds);
        await SyncPermissionsAsync(role.Id, permissions);
        await _db.SaveChangesAsync();

        await _audit.LogUpdateAsync("roles", existing.Id.ToString(), old, existing, updatedBy);
      });

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var role = await _db.Roles.FindAsync(id);
        if (role != null)
        {
          var oldStatus = role.Status;
          role.Status    = status;
          role.UpdatedAt = DateTime.UtcNow;
          role.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
          var action = status == StatusConstants.Deleted
              ? AuditConstants.Actions.Delete
              : AuditConstants.Actions.Update;
          await _audit.LogActionAsync("roles", id.ToString(), action, updatedBy,
              remarks: $"Status changed from {oldStatus} to {status}");
        }
      });

  private async Task SyncMenusAsync(int roleId, List<int> menuIds)
  {
    var existing = await _db.RoleMenus.Where(rm => rm.RoleId == roleId).ToListAsync();
    _db.RoleMenus.RemoveRange(existing);
    _db.RoleMenus.AddRange(menuIds.Select(mid => new RoleMenu { RoleId = roleId, MenuId = mid }));
  }

  private async Task SyncPermissionsAsync(int roleId, List<(int PermissionId, bool IsGranted)> permissions)
  {
    var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
    _db.RolePermissions.RemoveRange(existing);
    _db.RolePermissions.AddRange(permissions.Select(p => new RolePermission
    {
      RoleId       = roleId,
      PermissionId = p.PermissionId,
      IsGranted    = p.IsGranted
    }));
  }
}
