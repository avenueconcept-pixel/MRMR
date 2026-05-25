using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class LocationDbHelper : DbHelper
{
  public LocationDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<Location>> GetAllAsync()
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Locations
            .Where(l => l.Status != StatusConstants.Deleted)
            .Include(l => l.Country)
            .ToListAsync();
        return items.OrderBy(l => l.LocationName).ToList();
      });

  public async Task<List<Location>> GetAllActiveAsync()
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Locations
            .Where(l => l.Status == StatusConstants.Active)
            .Include(l => l.Country)
            .ToListAsync();
        return items.OrderBy(l => l.LocationName).ToList();
      });

  public async Task<Location?> GetByIdAsync(int id)
      => await ExecuteAsync(() => _db.Locations
          .Include(l => l.Country)
          .Include(l => l.State).ThenInclude(s => s!.Translations)
          .FirstOrDefaultAsync(l => l.Id == id && l.Status != StatusConstants.Deleted));

  public async Task<List<Location>> GetByCountryAsync(string countryCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.Locations
            .Where(l => l.CountryCode == countryCode && l.Status == StatusConstants.Active)
            .Include(l => l.Country)
            .ToListAsync();
        return items.OrderBy(l => l.LocationName).ToList();
      });

  public async Task<bool> IsLocationCodeExistsAsync(string locationCode, int excludeId = 0)
      => await ExecuteAsync(() => _db.Locations
          .AnyAsync(l => l.LocationCode == locationCode && l.Id != excludeId && l.Status != StatusConstants.Deleted));

  public async Task<LocationAddResult> CreateAsync(Location location, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Locations
            .FirstOrDefaultAsync(l => l.LocationCode == location.LocationCode);

        if (existing != null && existing.Status == StatusConstants.Deleted)
        {
          existing.LocationName = location.LocationName;
          existing.LocationType = location.LocationType;
          existing.CountryCode  = location.CountryCode;
          existing.StateId      = location.StateId;
          existing.City         = location.City;
          existing.Postcode     = location.Postcode;
          existing.Address      = location.Address;
          existing.Status       = StatusConstants.Active;
          existing.UpdatedAt    = DateTime.UtcNow;
          existing.UpdatedBy    = createdBy;
          await _db.SaveChangesAsync();
          await _audit.LogActionAsync("locations", existing.Id.ToString(), AuditConstants.Actions.Restore, createdBy, remarks: "Restored from deleted");
          return LocationAddResult.Restored;
        }

        if (existing != null)
          return LocationAddResult.DuplicateActive;

        location.CreatedAt = DateTime.UtcNow;
        location.CreatedBy = createdBy;
        location.UpdatedAt = DateTime.UtcNow;
        location.UpdatedBy = createdBy;
        _db.Locations.Add(location);
        await _db.SaveChangesAsync();
        await _audit.LogInsertAsync("locations", location.Id.ToString(), location, createdBy);
        return LocationAddResult.Created;
      });

  public async Task UpdateAsync(Location location, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Locations.FindAsync(location.Id);
        if (existing == null) return;

        var old = new Location
        {
          Id           = existing.Id,
          LocationCode = existing.LocationCode,
          LocationName = existing.LocationName,
          LocationType = existing.LocationType,
          CountryCode  = existing.CountryCode,
          StateId      = existing.StateId,
          City         = existing.City,
          Postcode     = existing.Postcode,
          Address      = existing.Address,
          Status       = existing.Status
        };

        existing.LocationName = location.LocationName;
        existing.LocationType = location.LocationType;
        existing.CountryCode  = location.CountryCode;
        existing.StateId      = location.StateId;
        existing.City         = location.City;
        existing.Postcode     = location.Postcode;
        existing.Address      = location.Address;
        existing.Status       = location.Status;
        existing.UpdatedAt    = DateTime.UtcNow;
        existing.UpdatedBy    = updatedBy;

        await _db.SaveChangesAsync();
        await _audit.LogUpdateAsync("locations", existing.Id.ToString(), old, existing, updatedBy);
      });

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var location = await _db.Locations.FindAsync(id);
        if (location != null)
        {
          var oldStatus = location.Status;
          location.Status    = status;
          location.UpdatedAt = DateTime.UtcNow;
          location.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
          var action = status == StatusConstants.Deleted
              ? AuditConstants.Actions.Delete
              : AuditConstants.Actions.Update;
          await _audit.LogActionAsync("locations", id.ToString(), action, updatedBy,
              remarks: $"Status changed from {oldStatus} to {status}");
        }
      });
}
