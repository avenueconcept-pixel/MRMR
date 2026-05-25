using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class RegionDbHelper : DbHelper
{
  public RegionDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<Region>> GetAllAsync()
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Regions
            .Where(r => r.Status != StatusConstants.Deleted)
            .Include(r => r.RegionCountries)
            .ToListAsync();
        return items.OrderBy(r => r.RegionName).ToList();
      });

  public async Task<List<Region>> GetAllActiveAsync()
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Regions
            .Where(r => r.Status == StatusConstants.Active)
            .Include(r => r.RegionCountries)
            .ToListAsync();
        return items.OrderBy(r => r.RegionName).ToList();
      });

  public async Task<Region?> GetByIdAsync(int id)
      => await ExecuteAsync(() => _db.Regions
          .Include(r => r.RegionCountries)
          .FirstOrDefaultAsync(r => r.Id == id && r.Status != StatusConstants.Deleted));

  public async Task<List<string>> GetCountryCodesByRegionAsync(int regionId)
      => await ExecuteAsync(() => _db.RegionCountries
          .Where(rc => rc.RegionId == regionId)
          .Select(rc => rc.CountryCode)
          .ToListAsync());

  public async Task<bool> IsRegionCodeExistsAsync(string regionCode, int excludeId = 0)
      => await ExecuteAsync(() => _db.Regions
          .AnyAsync(r => r.RegionCode == regionCode && r.Id != excludeId && r.Status != StatusConstants.Deleted));

  public async Task<RegionAddResult> CreateAsync(Region region, List<string> countryCodes, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Regions
            .FirstOrDefaultAsync(r => r.RegionCode == region.RegionCode);

        if (existing != null && existing.Status == StatusConstants.Deleted)
        {
          existing.RegionName = region.RegionName;
          existing.Status     = StatusConstants.Active;
          existing.UpdatedAt  = DateTime.UtcNow;
          existing.UpdatedBy  = createdBy;

          var existingCountries = await _db.RegionCountries
              .Where(rc => rc.RegionId == existing.Id)
              .ToListAsync();
          _db.RegionCountries.RemoveRange(existingCountries);

          _db.RegionCountries.AddRange(countryCodes.Select(code => new RegionCountry
          {
            RegionId    = existing.Id,
            CountryCode = code
          }));

          await _db.SaveChangesAsync();
          await _audit.LogActionAsync("regions", existing.Id.ToString(), AuditConstants.Actions.Restore, createdBy, remarks: "Restored from deleted");
          return RegionAddResult.Restored;
        }

        if (existing != null)
          return RegionAddResult.DuplicateActive;

        region.CreatedAt = DateTime.UtcNow;
        region.CreatedBy = createdBy;
        region.UpdatedAt = DateTime.UtcNow;
        region.UpdatedBy = createdBy;

        _db.Regions.Add(region);
        await _db.SaveChangesAsync();

        _db.RegionCountries.AddRange(countryCodes.Select(code => new RegionCountry
        {
          RegionId    = region.Id,
          CountryCode = code
        }));
        await _db.SaveChangesAsync();

        await _audit.LogInsertAsync("regions", region.Id.ToString(), region, createdBy);
        return RegionAddResult.Created;
      });

  public async Task UpdateAsync(Region region, List<string> countryCodes, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Regions.FindAsync(region.Id);
        if (existing == null) return;

        var old = new Region
        {
          Id         = existing.Id,
          RegionCode = existing.RegionCode,
          RegionName = existing.RegionName,
          Status     = existing.Status
        };

        existing.RegionName = region.RegionName;
        existing.Status     = region.Status;
        existing.UpdatedAt  = DateTime.UtcNow;
        existing.UpdatedBy  = updatedBy;

        var existingCountries = await _db.RegionCountries
            .Where(rc => rc.RegionId == region.Id)
            .ToListAsync();
        _db.RegionCountries.RemoveRange(existingCountries);

        _db.RegionCountries.AddRange(countryCodes.Select(code => new RegionCountry
        {
          RegionId    = region.Id,
          CountryCode = code
        }));

        await _db.SaveChangesAsync();
        await _audit.LogUpdateAsync("regions", existing.Id.ToString(), old, existing, updatedBy);
      });

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var region = await _db.Regions.FindAsync(id);
        if (region != null)
        {
          var oldStatus = region.Status;
          region.Status    = status;
          region.UpdatedAt = DateTime.UtcNow;
          region.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
          var action = status == StatusConstants.Deleted
              ? AuditConstants.Actions.Delete
              : AuditConstants.Actions.Update;
          await _audit.LogActionAsync("regions", id.ToString(), action, updatedBy,
              remarks: $"Status changed from {oldStatus} to {status}");
        }
      });
}
