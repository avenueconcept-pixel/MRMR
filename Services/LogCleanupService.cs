using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Helper.DB;
using MyApp.Models;
using Npgsql;

namespace MyApp.Services;

public class LogCleanupService : BackgroundService
{
  private readonly string                      _connectionString;
  private readonly int                         _retentionDays;
  private readonly ILogger<LogCleanupService>  _logger;
  private readonly IServiceProvider            _serviceProvider;

  private DateTime _lastLogCleanup     = DateTime.MinValue;
  private DateTime _lastSessionCleanup = DateTime.MinValue;
  private DateTime _lastArchiveRun     = DateTime.MinValue;
  private DateTime _lastPayoutRun      = DateTime.MinValue;

  public LogCleanupService(
      IConfiguration configuration,
      ILogger<LogCleanupService> logger,
      IServiceProvider serviceProvider)
  {
    _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    _retentionDays    = configuration.GetValue<int>("LogSettings:RetentionDays", 30);
    _logger           = logger;
    _serviceProvider  = serviceProvider;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

    while (!stoppingToken.IsCancellationRequested)
    {
      // Session cleanup — every 15 minutes
      if (DateTime.UtcNow - _lastSessionCleanup > TimeSpan.FromMinutes(15))
      {
        try
        {
          using var scope         = _serviceProvider.CreateScope();
          var sessionHelper       = scope.ServiceProvider.GetRequiredService<UserSessionDbHelper>();
          await sessionHelper.CleanupIdleSessionsAsync();
          _lastSessionCleanup     = DateTime.UtcNow;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
          _logger.LogError(ex, "Session cleanup failed");
        }
      }

      // Log cleanup — every 24 hours
      if (DateTime.UtcNow - _lastLogCleanup > TimeSpan.FromHours(24))
      {
        try
        {
          var cutoff = DateTime.Now.AddDays(-_retentionDays);
          using var conn = new NpgsqlConnection(_connectionString);
          await conn.OpenAsync(stoppingToken);
          using var cmd = new NpgsqlCommand("DELETE FROM app_logs WHERE created_at < @cutoff", conn);
          cmd.Parameters.AddWithValue("@cutoff", cutoff);
          var deleted = await cmd.ExecuteNonQueryAsync(stoppingToken);
          if (deleted > 0)
            _logger.LogInformation("Log cleanup: removed {Count} records older than {Days} days", deleted, _retentionDays);
          _lastLogCleanup = DateTime.UtcNow;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
          _logger.LogError(ex, "Log cleanup failed");
        }
      }

      // Page access + wallet archive — once daily at midnight
      if (DateTime.UtcNow.Date > _lastArchiveRun.Date)
      {
        try
        {
          using var scope       = _serviceProvider.CreateScope();
          var pageAccessHelper  = scope.ServiceProvider.GetRequiredService<MyApp.Helper.DB.PageAccessDbHelper>();
          await pageAccessHelper.ArchiveOldRecordsAsync();

          var walletDbHelper = scope.ServiceProvider.GetRequiredService<MyApp.Helper.DB.WalletDbHelper>();
          await walletDbHelper.ArchiveOldTransactionsAsync();

          _lastArchiveRun = DateTime.UtcNow;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
          _logger.LogError(ex, "Archive job failed");
        }
      }

      // Payout processor — every 5 minutes, processes closed periods only
      if (DateTime.UtcNow >= _lastPayoutRun.AddMinutes(5))
      {
        _lastPayoutRun = DateTime.UtcNow;
        try
        {
          using var payoutScope  = _serviceProvider.CreateScope();
          var payoutDbHelper     = payoutScope.ServiceProvider.GetRequiredService<WalletPayoutDbHelper>();
          var walletDbHelper     = payoutScope.ServiceProvider.GetRequiredService<WalletDbHelper>();

          var closedPeriods = await GetClosedPeriodsAsync(payoutScope);
          foreach (var period in closedPeriods)
          {
            WalletPayout? payout;
            while ((payout = await payoutDbHelper.GetNextPendingAsync()) != null
                   && payout.IncentivePeriodId == period.Id)
            {
              try
              {
                await payoutDbHelper.MarkProcessingAsync(payout.Id);

                await walletDbHelper.PostTransactionAsync(new PostTransactionDto
                {
                  MemberId          = payout.MemberId,
                  WalletType        = WalletTypeConstants.Cash,
                  TxnType           = CashTxnTypeConstants.Commission,
                  AmountUsd         = payout.AmountUsd,
                  Direction         = WalletDirectionConstants.In,
                  ReferenceId       = payout.ReferenceId,
                  Remark            = payout.Remark,
                  IncentivePeriodId = payout.IncentivePeriodId,
                  PeriodDate        = payout.PeriodDate,
                  CreatedBy         = "system"
                });

                await payoutDbHelper.MarkCompletedAsync(payout.Id);
              }
              catch (Exception ex)
              {
                await payoutDbHelper.MarkFailedOrRetryAsync(payout.Id, ex.Message);
              }
            }

            await payoutDbHelper.UpdatePeriodStatusAfterProcessingAsync(period.Id, "system");
          }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
          _logger.LogError(ex, "Payout processor failed");
        }
      }

      await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
    }
  }

  private async Task<List<IncentivePeriod>> GetClosedPeriodsAsync(IServiceScope scope)
  {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    return await db.IncentivePeriods
        .Where(p => p.Status == IncentivePeriodStatusConstants.Closed)
        .OrderBy(p => p.PeriodDate)
        .ToListAsync();
  }
}
