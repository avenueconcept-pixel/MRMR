using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class AdminUserDbHelper : DbHelper
{
  public AdminUserDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<AdminUser>> GetAllAsync()
      => await ExecuteAsync(async () => await _db.AdminUsers
          .Where(u => u.Status != StatusConstants.Deleted)
          .Include(u => u.Role)
          .Include(u => u.Department)
          .OrderBy(u => u.FullName)
          .ToListAsync());

  public async Task<AdminUser?> GetByIdAsync(int id)
      => await ExecuteAsync(() => _db.AdminUsers
          .Include(u => u.Role)
          .Include(u => u.Department)
          .Include(u => u.Region)
          .FirstOrDefaultAsync(u => u.Id == id && u.Status != StatusConstants.Deleted));

  public async Task<bool> IsUsernameExistsAsync(string username, int? excludeId = null)
      => await ExecuteAsync(() => _db.AdminUsers
          .AnyAsync(u => u.Username == username &&
                         (excludeId == null || u.Id != excludeId) &&
                         u.Status != StatusConstants.Deleted));

  public async Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
      => await ExecuteAsync(() => _db.AdminUsers
          .AnyAsync(u => u.Email == email &&
                         (excludeId == null || u.Id != excludeId) &&
                         u.Status != StatusConstants.Deleted));

  public async Task AddAsync(AdminUser user)
      => await ExecuteAsync(async () =>
      {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _db.AdminUsers.Add(user);
        await _db.SaveChangesAsync();
      });

  public async Task UpdateAsync(AdminUser user)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.AdminUsers.FindAsync(user.Id);
        if (existing == null) return;

        existing.FullName              = user.FullName;
        existing.Email                 = user.Email;
        existing.RoleId                = user.RoleId;
        existing.DeptId                = user.DeptId;
        existing.CountryCode           = user.CountryCode;
        existing.RegionId              = user.RegionId;
        existing.MobileCountryCode     = user.MobileCountryCode;
        existing.MobileNo              = user.MobileNo;
        existing.IsForceChangePassword = user.IsForceChangePassword;
        existing.ProfileImage          = user.ProfileImage;
        existing.Status                = user.Status;
        existing.UpdatedAt             = DateTime.UtcNow;
        existing.UpdatedBy             = user.UpdatedBy;
        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var user = await _db.AdminUsers.FindAsync(id);
        if (user != null)
        {
          user.Status    = status;
          user.UpdatedAt = DateTime.UtcNow;
          user.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
        }
      });

  public async Task UpdatePasswordAsync(int id, string hashedPassword, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var user = await _db.AdminUsers.FindAsync(id);
        if (user != null)
        {
          user.PasswordHash           = hashedPassword;
          user.IsForceChangePassword  = true;
          user.UpdatedAt              = DateTime.UtcNow;
          user.UpdatedBy              = updatedBy;
          await _db.SaveChangesAsync();
        }
      });

  public async Task ForceChangePasswordAsync(int userId, string newHashedPassword, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var user = await _db.AdminUsers.FindAsync(userId);
        if (user == null) return;
        user.PasswordHash          = newHashedPassword;
        user.IsForceChangePassword = false;
        user.UpdatedAt             = DateTime.UtcNow;
        user.UpdatedBy             = updatedBy;
        await _db.SaveChangesAsync();
      });

  public async Task ChangePasswordAsync(int userId, string newHashedPassword, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var user = await _db.AdminUsers.FindAsync(userId);
        if (user == null) return;
        user.PasswordHash = newHashedPassword;
        user.UpdatedAt    = DateTime.UtcNow;
        user.UpdatedBy    = updatedBy;
        await _db.SaveChangesAsync();
      });

  public async Task<string?> GetPasswordHashAsync(int userId)
      => await ExecuteAsync(async () =>
      {
        var user = await _db.AdminUsers.FindAsync(userId);
        return user?.PasswordHash;
      });

  public async Task SoftDeleteAsync(int id, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var user = await _db.AdminUsers.FindAsync(id);
        if (user != null)
        {
          user.Status    = StatusConstants.Deleted;
          user.UpdatedAt = DateTime.UtcNow;
          user.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
        }
      });
}
