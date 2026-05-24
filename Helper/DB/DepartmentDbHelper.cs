using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public enum DeptAddResult
{
  Created,
  Restored,
  DuplicateActive
}

public class DepartmentDbHelper : DbHelper
{
  public DepartmentDbHelper(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

  public async Task<List<Department>> GetAllAsync()
      => await ExecuteAsync<List<Department>>(() => _db.Departments
          .Where(d => d.Status != StatusConstants.Deleted)
          .OrderBy(d => d.DeptName)
          .ToListAsync());

  public async Task<List<Department>> GetAllActiveAsync()
      => await ExecuteAsync<List<Department>>(() => _db.Departments
          .Where(d => d.Status == StatusConstants.Active)
          .OrderBy(d => d.DeptName)
          .ToListAsync());

  public async Task<Department?> GetByIdAsync(int id)
      => await ExecuteAsync<Department?>(async () => await _db.Departments.FindAsync(id));

  public async Task<bool> IsDeptNameExistsAsync(string deptName, int excludeId = 0)
      => await ExecuteAsync<bool>(() => _db.Departments
          .AnyAsync(d => d.DeptName.ToLower() == deptName.Trim().ToLower()
                      && d.Id != excludeId
                      && d.Status != StatusConstants.Deleted));

  public async Task<DeptAddResult> CreateAsync(Department dept, string createdBy)
      => await ExecuteAsync<DeptAddResult>(async () =>
      {
        var existing = await _db.Departments
            .FirstOrDefaultAsync(d => d.DeptName.ToLower() == dept.DeptName.Trim().ToLower());

        if (existing != null)
        {
          if (existing.Status == StatusConstants.Deleted)
          {
            existing.DeptName  = dept.DeptName.Trim();
            existing.Status    = dept.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = createdBy;
            await _db.SaveChangesAsync();
            return DeptAddResult.Restored;
          }
          return DeptAddResult.DuplicateActive;
        }

        var entity = new Department
        {
          DeptName  = dept.DeptName.Trim(),
          Status    = dept.Status,
          CreatedAt = DateTime.UtcNow,
          CreatedBy = createdBy,
          UpdatedAt = DateTime.UtcNow,
          UpdatedBy = createdBy
        };
        _db.Departments.Add(entity);
        await _db.SaveChangesAsync();
        return DeptAddResult.Created;
      });

  public async Task UpdateAsync(Department dept, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Departments.FindAsync(dept.Id);
        if (existing == null) return;

        existing.DeptName  = dept.DeptName;
        existing.Status    = dept.Status;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = updatedBy;
        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var dept = await _db.Departments.FindAsync(id);
        if (dept != null)
        {
          dept.Status    = status;
          dept.UpdatedAt = DateTime.UtcNow;
          dept.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
        }
      });
}
