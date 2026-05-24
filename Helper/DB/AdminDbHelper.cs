using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Models;
using MyApp.Constants;
using Microsoft.Extensions.Logging;

namespace MyApp.Helper.DB;

public class AdminDbHelper : DbHelper
{
  public AdminDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<AdminUser?> GetByUsernameAsync(string username)
      => await ExecuteAsync(() => _db.AdminUsers
          .Include(a => a.Country)
          .FirstOrDefaultAsync(a => a.Username == username && a.Status == StatusConstants.Active));

  public async Task<AdminUser?> GetByIdAsync(int id)
      => await ExecuteAsync(async () => await _db.AdminUsers.FindAsync(id));

  public async Task<bool> UsernameExistsAsync(string username)
      => await ExecuteAsync(() => _db.AdminUsers.AnyAsync(a => a.Username == username));

  public async Task UpdateLastLoginAsync(string username)
      => await ExecuteAsync(async () =>
      {
        var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Username == username);
        if (admin != null)
        {
          admin.LastLogin = DateTime.UtcNow;
          await _db.SaveChangesAsync();
        }
      });

  public async Task UpdateLoginInfoAsync(string username, string langCode)
      => await ExecuteAsync(async () =>
      {
        var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Username == username);
        if (admin != null)
        {
          admin.LastLogin = DateTime.UtcNow;
          admin.LastLoginLangCode = langCode;
          await _db.SaveChangesAsync();
        }
      });

  public async Task UpdateLastLoginLangCodeAsync(string username, string langCode)
      => await ExecuteAsync(async () =>
      {
        var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Username == username);
        if (admin != null)
        {
          admin.LastLoginLangCode = langCode;
          await _db.SaveChangesAsync();
        }
      });

  public async Task<string> GetLastLoginLangCodeAsync(string username)
      => await ExecuteAsync(async () =>
      {
        var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Username == username);
        return admin?.LastLoginLangCode ?? string.Empty;
      });

  public async Task UpdateAsync(AdminUser adminUser)
      => await ExecuteAsync(async () =>
      {
        _db.AdminUsers.Update(adminUser);
        await _db.SaveChangesAsync();
      });
}
