using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class AdminDbHelper : DbHelper
{
  public AdminDbHelper(AppDbContext db) : base(db) { }

  public async Task<AdminUser?> GetByUsernameAsync(string username)
      => await _db.AdminUsers
          .FirstOrDefaultAsync(a => a.Username == username && a.IsActive);

  public async Task<AdminUser?> GetByIdAsync(int id)
      => await _db.AdminUsers.FindAsync(id);

  public async Task<bool> UsernameExistsAsync(string username)
      => await _db.AdminUsers.AnyAsync(a => a.Username == username);

  public async Task UpdateLastLoginAsync(int adminId)
  {
    var admin = await _db.AdminUsers.FindAsync(adminId);
    if (admin != null)
    {
      admin.LastLogin = DateTime.UtcNow;
      await _db.SaveChangesAsync();
    }
  }
}
