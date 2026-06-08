using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class RankDbHelper : DbHelper
{
  public RankDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<Rank>> GetAllAsync()
      => await ExecuteAsync(async () =>
          await _db.Ranks
              .Where(r => r.Status != StatusConstants.Deleted)
              .OrderBy(r => r.SortOrder)
              .ThenBy(r => r.RankName)
              .ToListAsync());

  public async Task<List<Rank>> GetAllActiveAsync()
      => await ExecuteAsync(async () =>
          await _db.Ranks
              .Where(r => r.Status == StatusConstants.Active)
              .OrderBy(r => r.SortOrder)
              .ThenBy(r => r.RankName)
              .ToListAsync());

  public async Task<Rank?> GetByCodeAsync(string rankCode)
      => await ExecuteAsync(async () =>
          await _db.Ranks
              .FirstOrDefaultAsync(r => r.RankCode == rankCode));

  public async Task<RankAddResult> AddAsync(Rank rank, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Ranks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.RankCode == rank.RankCode);

        if (existing != null)
        {
          if (existing.Status != StatusConstants.Deleted)
            return RankAddResult.DuplicateActive;

          existing.RankName  = rank.RankName;
          existing.SortOrder = rank.SortOrder;
          existing.Status    = StatusConstants.Active;
          existing.UpdatedBy = createdBy;
          existing.UpdatedAt = DateTime.UtcNow;
          await _db.SaveChangesAsync();
          return RankAddResult.Restored;
        }

        rank.CreatedBy = createdBy;
        rank.CreatedAt = DateTime.UtcNow;
        rank.UpdatedBy = createdBy;
        rank.UpdatedAt = DateTime.UtcNow;
        rank.Status    = StatusConstants.Active;
        _db.Ranks.Add(rank);
        await _db.SaveChangesAsync();
        return RankAddResult.Created;
      });

  public async Task UpdateAsync(Rank rank, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Ranks
            .FirstOrDefaultAsync(r => r.RankCode == rank.RankCode);
        if (existing == null) return;

        existing.RankName  = rank.RankName;
        existing.SortOrder = rank.SortOrder;
        existing.Status    = rank.Status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(string rankCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.Ranks
            .FirstOrDefaultAsync(r => r.RankCode == rankCode);
        if (existing == null) return;
        existing.Status    = status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });
}
