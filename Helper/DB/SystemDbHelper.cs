using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class SystemDbHelper : DbHelper
{
  public SystemDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<List<AppSystem>> GetAllAsync()
      => await ExecuteAsync(async () =>
          await _db.AppSystems
              .Where(s => s.Status != StatusConstants.Deleted)
              .OrderBy(s => s.SortOrder)
              .ThenBy(s => s.SystemName)
              .ToListAsync());

  public async Task<List<AppSystem>> GetAllActiveAsync()
      => await ExecuteAsync(async () =>
          await _db.AppSystems
              .Where(s => s.Status == StatusConstants.Active)
              .OrderBy(s => s.SortOrder)
              .ThenBy(s => s.SystemName)
              .ToListAsync());

  public async Task<AppSystem?> GetByCodeAsync(string systemCode)
      => await ExecuteAsync(async () =>
          await _db.AppSystems
              .FirstOrDefaultAsync(s => s.SystemCode == systemCode && s.Status != StatusConstants.Deleted));

  public async Task<SystemAddResult> AddAsync(AppSystem system, string createdBy)
      => await ExecuteAsync(async () =>
      {
        bool exists = await _db.AppSystems.AnyAsync(s =>
            s.SystemCode == system.SystemCode &&
            s.Status     != StatusConstants.Deleted);

        if (exists) return SystemAddResult.DuplicateActive;

        system.CreatedBy = createdBy;
        system.CreatedAt = DateTime.UtcNow;
        system.UpdatedBy = createdBy;
        system.UpdatedAt = DateTime.UtcNow;
        _db.AppSystems.Add(system);
        await _db.SaveChangesAsync();
        return SystemAddResult.Created;
      });

  public async Task UpdateAsync(AppSystem system, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.AppSystems
            .FirstOrDefaultAsync(s => s.SystemCode == system.SystemCode);
        if (existing == null) return;

        existing.SystemName = system.SystemName;
        existing.SortOrder  = system.SortOrder;
        existing.Status     = system.Status;
        existing.UpdatedBy  = updatedBy;
        existing.UpdatedAt  = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(string systemCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.AppSystems
            .FirstOrDefaultAsync(s => s.SystemCode == systemCode);
        if (existing == null) return;
        existing.Status    = status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });
}
