using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;

namespace MyApp.Helper.DB;

public class DashboardDbHelper : DbHelper
{
  public DashboardDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<int> GetAdminUserCountAsync()
      => await ExecuteAsync(async () => await _db.AdminUsers
          .CountAsync(u => u.Status == StatusConstants.Active));

  public async Task<int> GetRoleCountAsync()
      => await ExecuteAsync(async () => await _db.Roles
          .CountAsync(r => r.Status == StatusConstants.Active));

  public async Task<int> GetCountryCountAsync()
      => await ExecuteAsync(async () => await _db.Countries
          .CountAsync(c => c.Status == StatusConstants.Active));

  public async Task<int> GetLanguageCountAsync()
      => await ExecuteAsync(async () => await _db.Languages
          .CountAsync(l => l.Status == StatusConstants.Active));
}
