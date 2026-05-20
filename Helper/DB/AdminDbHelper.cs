using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using MyApp.Constants;

namespace MyApp.Helper.DB;

public class AdminDbHelper : DbHelper
{
  public AdminDbHelper(AppDbContext db) : base(db) { }

  public async Task<AdminUser?> GetByUsernameAsync(string username)
      => await _db.AdminUsers
          .FirstOrDefaultAsync(a => a.Username == username && a.Status == UserStatusConstants.Active);

  public async Task<AdminUser?> GetByIdAsync(int id)
      => await _db.AdminUsers.FindAsync(id);

  public async Task<bool> UsernameExistsAsync(string username)
      => await _db.AdminUsers.AnyAsync(a => a.Username == username);

  public async Task UpdateLastLoginAsync(string username)
  {
    var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Username == username);
    if (admin != null)
    {
      admin.LastLogin = DateTime.Now;
      await _db.SaveChangesAsync();
    }
  }

  public async Task UpdateLastLoginLangCodeAsync(string username, string langCode)
  {
    var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Username == username);
    if (admin != null)
    {
      admin.LastLoginLangCode = langCode;
      await _db.SaveChangesAsync();
    }
  }

  public async Task<string> GetLastLoginLangCodeAsync(string username)
  {
    var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Username == username);
    return admin?.LastLoginLangCode ?? string.Empty;
  }
}
