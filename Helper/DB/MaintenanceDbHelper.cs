using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class MaintenanceDbHelper : DbHelper
{
  public MaintenanceDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<List<MaintenanceSchedule>> GetAllAsync()
      => await ExecuteAsync(async () =>
          await _db.MaintenanceSchedules
              .Where(m => m.Status != StatusConstants.Deleted)
              .Include(m => m.Systems)
              .Include(m => m.Messages)
              .OrderByDescending(m => m.StartAt)
              .ToListAsync());

  public async Task<MaintenanceSchedule?> GetByIdAsync(int id)
      => await ExecuteAsync(async () =>
          await _db.MaintenanceSchedules
              .Include(m => m.Systems)
              .Include(m => m.Messages)
              .FirstOrDefaultAsync(m => m.Id == id && m.Status != StatusConstants.Deleted));

  public async Task AddAsync(
      MaintenanceSchedule           schedule,
      List<string>                  systemCodes,
      List<MaintenanceScheduleMessage> messages,
      string                        createdBy)
      => await ExecuteAsync(async () =>
      {
        schedule.CreatedBy = createdBy;
        schedule.CreatedAt = DateTime.UtcNow;
        schedule.UpdatedBy = createdBy;
        schedule.UpdatedAt = DateTime.UtcNow;
        _db.MaintenanceSchedules.Add(schedule);
        await _db.SaveChangesAsync();

        foreach (var code in systemCodes)
        {
          _db.MaintenanceScheduleSystems.Add(new MaintenanceScheduleSystem
          {
            MaintenanceId = schedule.Id,
            SystemCode    = code
          });
        }

        foreach (var msg in messages)
        {
          msg.MaintenanceId = schedule.Id;
          _db.MaintenanceScheduleMessages.Add(msg);
        }

        await _db.SaveChangesAsync();
      });

  public async Task UpdateAsync(
      MaintenanceSchedule           schedule,
      List<string>                  systemCodes,
      List<MaintenanceScheduleMessage> messages,
      string                        updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.MaintenanceSchedules
            .Include(m => m.Systems)
            .Include(m => m.Messages)
            .FirstOrDefaultAsync(m => m.Id == schedule.Id);
        if (existing == null) return;

        existing.Title     = schedule.Title;
        existing.StartAt   = schedule.StartAt;
        existing.EndAt     = schedule.EndAt;
        existing.IsActive  = schedule.IsActive;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;

        _db.MaintenanceScheduleSystems.RemoveRange(existing.Systems);
        _db.MaintenanceScheduleMessages.RemoveRange(existing.Messages);
        await _db.SaveChangesAsync();

        foreach (var code in systemCodes)
        {
          _db.MaintenanceScheduleSystems.Add(new MaintenanceScheduleSystem
          {
            MaintenanceId = existing.Id,
            SystemCode    = code
          });
        }

        foreach (var msg in messages)
        {
          msg.MaintenanceId = existing.Id;
          _db.MaintenanceScheduleMessages.Add(msg);
        }

        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.MaintenanceSchedules.FindAsync(id);
        if (existing == null) return;
        existing.Status    = status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task UpdateIsActiveAsync(int id, bool isActive, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.MaintenanceSchedules.FindAsync(id);
        if (existing == null) return;
        existing.IsActive  = isActive;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });
}
