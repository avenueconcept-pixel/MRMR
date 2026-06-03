using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class PriceTierDbHelper : DbHelper
{
  public PriceTierDbHelper(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

  public async Task<List<PriceTier>> GetAllAsync()
      => await ExecuteAsync(async () =>
          await _db.PriceTiers
              .Where(p => p.Status != StatusConstants.Deleted)
              .OrderBy(p => p.SortOrder)
              .ThenBy(p => p.TierName)
              .ToListAsync());

  public async Task<List<PriceTier>> GetAllActiveAsync()
      => await ExecuteAsync(async () =>
          await _db.PriceTiers
              .Where(p => p.Status == StatusConstants.Active)
              .OrderBy(p => p.SortOrder)
              .ThenBy(p => p.TierName)
              .ToListAsync());

  public async Task<PriceTier?> GetByCodeAsync(string tierCode)
      => await ExecuteAsync(async () =>
          await _db.PriceTiers
              .FirstOrDefaultAsync(p => p.TierCode == tierCode));

  public async Task<PriceTierAddResult> AddAsync(PriceTier tier, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.PriceTiers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.TierCode == tier.TierCode);

        if (existing != null)
        {
          if (existing.Status != StatusConstants.Deleted)
            return PriceTierAddResult.DuplicateActive;

          existing.TierName  = tier.TierName;
          existing.SortOrder = tier.SortOrder;
          existing.Status    = StatusConstants.Active;
          existing.UpdatedBy = createdBy;
          existing.UpdatedAt = DateTime.UtcNow;
          await _db.SaveChangesAsync();
          return PriceTierAddResult.Restored;
        }

        tier.CreatedBy = createdBy;
        tier.CreatedAt = DateTime.UtcNow;
        tier.UpdatedBy = createdBy;
        tier.UpdatedAt = DateTime.UtcNow;
        tier.Status    = StatusConstants.Active;
        _db.PriceTiers.Add(tier);
        await _db.SaveChangesAsync();
        return PriceTierAddResult.Created;
      });

  public async Task UpdateAsync(PriceTier tier, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.PriceTiers
            .FirstOrDefaultAsync(p => p.TierCode == tier.TierCode);
        if (existing == null) return;

        existing.TierName  = tier.TierName;
        existing.SortOrder = tier.SortOrder;
        existing.Status    = tier.Status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task UpdateStatusAsync(string tierCode, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.PriceTiers
            .FirstOrDefaultAsync(p => p.TierCode == tierCode);
        if (existing == null) return;
        existing.Status    = status;
        existing.UpdatedBy = updatedBy;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });
}
