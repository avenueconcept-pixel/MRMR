using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class AppSettingsDbHelper : DbHelper
{
  public AppSettingsDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<string> GetAsync(string systemCode, string key)
      => await ExecuteAsync(async () =>
      {
        var row = await _db.AppSettings
            .FirstOrDefaultAsync(s => s.SystemCode == systemCode && s.SettingKey == key);
        return row?.SettingValue ?? string.Empty;
      });

  public async Task SetAsync(string systemCode, string key, string value, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var row = await _db.AppSettings
            .FirstOrDefaultAsync(s => s.SystemCode == systemCode && s.SettingKey == key);

        if (row == null)
        {
          row = new AppSetting
          {
            SystemCode   = systemCode,
            SettingKey   = key,
            SettingValue = value,
            UpdatedBy    = updatedBy,
            UpdatedAt    = DateTime.UtcNow
          };
          _db.AppSettings.Add(row);
        }
        else
        {
          row.SettingValue = value;
          row.UpdatedBy    = updatedBy;
          row.UpdatedAt    = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
      });

  public async Task<BrandingDto> GetBrandingAsync()
      => await ExecuteAsync(async () =>
      {
        var rows = await _db.AppSettings
            .Where(s => (s.SystemCode == "global"   && s.SettingKey == "logo_path")
                     || (s.SystemCode == "admin"    && (s.SettingKey == "system_name" || s.SettingKey == "footer_text"))
                     || (s.SystemCode == "customer" && (s.SettingKey == "system_name" || s.SettingKey == "footer_text")))
            .ToListAsync();

        string Get(string sc, string sk) =>
            rows.FirstOrDefault(r => r.SystemCode == sc && r.SettingKey == sk)?.SettingValue
            ?? string.Empty;

        return new BrandingDto
        {
          LogoPath           = Get("global",   "logo_path"),
          AdminSystemName    = Get("admin",    "system_name"),
          CustomerSystemName = Get("customer", "system_name"),
          AdminFooterText    = Get("admin",    "footer_text"),
          CustomerFooterText = Get("customer", "footer_text")
        };
      });
}
