using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class SystemSettingDbHelper : DbHelper
{
    public SystemSettingDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
        : base(db, audit, loggerFactory) { }

    public async Task<List<SystemSetting>> GetAllAsync()
        => await ExecuteAsync(async () =>
            await _db.SystemSettings
                .OrderBy(s => s.SettingKey)
                .ToListAsync());

    public async Task<SystemSetting?> GetByKeyAsync(string key)
        => await ExecuteAsync(async () =>
            await _db.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == key));

    public async Task UpdateAsync(SystemSetting setting, string updatedBy)
        => await ExecuteAsync(async () =>
        {
            var existing = await _db.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == setting.SettingKey)
                ?? throw new Exception($"Setting '{setting.SettingKey}' not found");

            existing.SettingValue = setting.SettingValue;
            existing.UpdatedBy    = updatedBy;
            existing.UpdatedAt    = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        });
}
