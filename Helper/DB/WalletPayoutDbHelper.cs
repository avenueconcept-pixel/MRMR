using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class WalletPayoutDbHelper : DbHelper
{
  public WalletPayoutDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<IncentivePeriod> GetOrCreateTodayPeriodAsync(string createdBy)
      => await ExecuteAsync(async () =>
      {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var period = await _db.IncentivePeriods
            .FirstOrDefaultAsync(p => p.PeriodDate == today);
        if (period != null) return period;

        period = new IncentivePeriod
        {
          PeriodDate = today,
          Status     = IncentivePeriodStatusConstants.Open,
          CreatedBy  = createdBy,
          CreatedAt  = DateTime.UtcNow,
          UpdatedBy  = createdBy,
          UpdatedAt  = DateTime.UtcNow
        };
        _db.IncentivePeriods.Add(period);
        await _db.SaveChangesAsync();
        return period;
      });

  public async Task<IncentivePeriod?> GetByDateAsync(DateOnly date)
      => await ExecuteAsync(async () =>
          await _db.IncentivePeriods
              .FirstOrDefaultAsync(p => p.PeriodDate == date));

  public async Task<(List<IncentivePeriodSummaryDto> Items, int Total)> GetAllAsync(
      int page, int pageSize, string? filterStatus)
      => await ExecuteAsync(async () =>
      {
        var q = _db.IncentivePeriods.AsQueryable();
        if (!string.IsNullOrEmpty(filterStatus))
          q = q.Where(p => p.Status == filterStatus);

        var total   = await q.CountAsync();
        var periods = await q
            .OrderByDescending(p => p.PeriodDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var summaries = new List<IncentivePeriodSummaryDto>();
        foreach (var p in periods)
        {
          var payouts = await _db.WalletPayouts
              .Where(w => w.IncentivePeriodId == p.Id)
              .GroupBy(w => w.Status)
              .Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(x => x.AmountUsd) })
              .ToListAsync();

          summaries.Add(new IncentivePeriodSummaryDto
          {
            Id             = p.Id,
            PeriodDate     = p.PeriodDate,
            Status         = p.Status,
            TotalPayouts   = payouts.Sum(x => x.Count),
            PendingCount   = payouts.FirstOrDefault(x => x.Status == WalletPayoutStatusConstants.Pending)?.Count ?? 0,
            CompletedCount = payouts.FirstOrDefault(x => x.Status == WalletPayoutStatusConstants.Completed)?.Count ?? 0,
            FailedCount    = payouts.FirstOrDefault(x => x.Status == WalletPayoutStatusConstants.Failed)?.Count ?? 0,
            TotalAmountUsd = payouts.Sum(x => x.Total),
            ClosedAt       = p.ClosedAt,
            ProcessedAt    = p.ProcessedAt
          });
        }
        return (summaries, total);
      });

  public async Task<IncentivePeriodSummaryDto?> GetPeriodSummaryAsync(int periodId)
      => await ExecuteAsync(async () =>
      {
        var p = await _db.IncentivePeriods.FindAsync(periodId);
        if (p == null) return null;

        var payouts = await _db.WalletPayouts
            .Where(w => w.IncentivePeriodId == p.Id)
            .GroupBy(w => w.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(x => x.AmountUsd) })
            .ToListAsync();

        return new IncentivePeriodSummaryDto
        {
          Id             = p.Id,
          PeriodDate     = p.PeriodDate,
          Status         = p.Status,
          TotalPayouts   = payouts.Sum(x => x.Count),
          PendingCount   = payouts.FirstOrDefault(x => x.Status == WalletPayoutStatusConstants.Pending)?.Count ?? 0,
          CompletedCount = payouts.FirstOrDefault(x => x.Status == WalletPayoutStatusConstants.Completed)?.Count ?? 0,
          FailedCount    = payouts.FirstOrDefault(x => x.Status == WalletPayoutStatusConstants.Failed)?.Count ?? 0,
          TotalAmountUsd = payouts.Sum(x => x.Total),
          ClosedAt       = p.ClosedAt,
          ProcessedAt    = p.ProcessedAt
        };
      });

  public async Task<(List<WalletPayoutRowDto> Items, int Total)> GetPayoutsAsync(
      int periodId, int page, int pageSize, string? filterStatus, string? filterType)
      => await ExecuteAsync(async () =>
      {
        var q = _db.WalletPayouts
            .Include(w => w.Member)
            .Where(w => w.IncentivePeriodId == periodId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filterStatus))
          q = q.Where(w => w.Status == filterStatus);
        if (!string.IsNullOrEmpty(filterType))
          q = q.Where(w => w.IncentiveType == filterType);

        var total = await q.CountAsync();
        var items = await q
            .OrderBy(w => w.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var rows = items.Select(w => new WalletPayoutRowDto
        {
          Id             = w.Id,
          MemberUsername = w.Member?.Username ?? string.Empty,
          MemberFullName = w.Member?.FullName ?? string.Empty,
          IncentiveType  = w.IncentiveType,
          PvAmount       = w.PvAmount,
          AmountUsd      = w.AmountUsd,
          ReferenceId    = w.ReferenceId,
          Remark         = w.Remark,
          Status         = w.Status,
          RetryCount     = w.RetryCount,
          ErrorMessage   = w.ErrorMessage,
          ProcessedAt    = w.ProcessedAt,
          CreatedAt      = w.CreatedAt
        }).ToList();

        return (rows, total);
      });

  public async Task AddPayoutAsync(WalletPayout payout)
      => await ExecuteAsync(async () =>
      {
        payout.Status    = WalletPayoutStatusConstants.Pending;
        payout.CreatedAt = DateTime.UtcNow;
        _db.WalletPayouts.Add(payout);
        await _db.SaveChangesAsync();
      });

  public async Task ClosePeriodAsync(int periodId, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var period = await _db.IncentivePeriods.FindAsync(periodId)
            ?? throw new Exception($"Period {periodId} not found");
        period.Status    = IncentivePeriodStatusConstants.Closed;
        period.ClosedAt  = DateTime.UtcNow;
        period.UpdatedBy = updatedBy;
        period.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task<WalletPayout?> GetNextPendingAsync()
      => await ExecuteAsync(async () =>
          await _db.WalletPayouts
              .Where(w => w.Status == WalletPayoutStatusConstants.Pending
                       && w.RetryCount < 3)
              .OrderBy(w => w.Id)
              .FirstOrDefaultAsync());

  public async Task MarkProcessingAsync(long payoutId)
      => await ExecuteAsync(async () =>
      {
        var payout = await _db.WalletPayouts.FindAsync(payoutId)
            ?? throw new Exception($"Payout {payoutId} not found");
        payout.Status = WalletPayoutStatusConstants.Processing;
        await _db.SaveChangesAsync();
      });

  public async Task MarkCompletedAsync(long payoutId)
      => await ExecuteAsync(async () =>
      {
        var payout = await _db.WalletPayouts.FindAsync(payoutId)
            ?? throw new Exception($"Payout {payoutId} not found");
        payout.Status      = WalletPayoutStatusConstants.Completed;
        payout.ProcessedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task MarkFailedOrRetryAsync(long payoutId, string errorMessage)
      => await ExecuteAsync(async () =>
      {
        var payout = await _db.WalletPayouts.FindAsync(payoutId)
            ?? throw new Exception($"Payout {payoutId} not found");
        payout.RetryCount++;
        payout.ErrorMessage = errorMessage;
        payout.Status = payout.RetryCount >= 3
            ? WalletPayoutStatusConstants.Failed
            : WalletPayoutStatusConstants.Pending;
        await _db.SaveChangesAsync();
      });

  public async Task UpdatePeriodStatusAfterProcessingAsync(int periodId, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var period = await _db.IncentivePeriods.FindAsync(periodId)
            ?? throw new Exception($"Period {periodId} not found");

        var hasFailed = await _db.WalletPayouts
            .AnyAsync(w => w.IncentivePeriodId == periodId
                        && w.Status == WalletPayoutStatusConstants.Failed);
        var hasPending = await _db.WalletPayouts
            .AnyAsync(w => w.IncentivePeriodId == periodId
                        && w.Status == WalletPayoutStatusConstants.Pending);

        period.Status      = (hasFailed || hasPending)
            ? IncentivePeriodStatusConstants.Partial
            : IncentivePeriodStatusConstants.Processed;
        period.ProcessedAt = DateTime.UtcNow;
        period.UpdatedBy   = updatedBy;
        period.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task RetryFailedAsync(int periodId, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var failed = await _db.WalletPayouts
            .Where(w => w.IncentivePeriodId == periodId
                     && w.Status == WalletPayoutStatusConstants.Failed)
            .ToListAsync();
        foreach (var p in failed)
        {
          p.Status       = WalletPayoutStatusConstants.Pending;
          p.RetryCount   = 0;
          p.ErrorMessage = null;
        }

        var period = await _db.IncentivePeriods.FindAsync(periodId);
        if (period != null)
        {
          period.Status    = IncentivePeriodStatusConstants.Closed;
          period.UpdatedBy = updatedBy;
          period.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
      });
}
