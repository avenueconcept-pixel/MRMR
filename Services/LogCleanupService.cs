using Npgsql;

namespace MyApp.Services;

public class LogCleanupService : BackgroundService
{
  private readonly string _connectionString;
  private readonly int _retentionDays;
  private readonly ILogger<LogCleanupService> _logger;

  public LogCleanupService(IConfiguration configuration, ILogger<LogCleanupService> logger)
  {
    _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    _retentionDays = configuration.GetValue<int>("LogSettings:RetentionDays", 30);
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    // Wait for app to fully start before first run
    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

    while (!stoppingToken.IsCancellationRequested)
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
      }
      catch (Exception ex) when (ex is not OperationCanceledException)
      {
        _logger.LogError(ex, "Log cleanup failed");
      }

      await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
    }
  }
}
