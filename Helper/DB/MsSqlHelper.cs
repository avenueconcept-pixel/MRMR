using Microsoft.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace MyApp.Helper.DB;

public abstract class MsSqlHelper
{
    private readonly string  _connectionString;
    private readonly ILogger _logger;

    protected MsSqlHelper(IConfiguration config, ILoggerFactory loggerFactory, string loggerCategory)
    {
        _connectionString = config.GetConnectionString("MsSqlConnection")
            ?? throw new InvalidOperationException("MsSqlConnection connection string is not configured.");
        _logger = loggerFactory.CreateLogger(loggerCategory);
    }

    protected async Task<SqlConnection> OpenAsync(CancellationToken ct = default)
    {
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }

    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> op,
        [CallerMemberName] string caller = "",
        [CallerFilePath]   string file   = "",
        [CallerLineNumber] int    line   = 0)
    {
        try
        {
            return await op();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MsSQL error in {M} ({F}:{L})", caller, Path.GetFileName(file), line);
            throw;
        }
    }

    protected async Task ExecuteAsync(Func<Task> op,
        [CallerMemberName] string caller = "",
        [CallerFilePath]   string file   = "",
        [CallerLineNumber] int    line   = 0)
    {
        try
        {
            await op();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MsSQL error in {M} ({F}:{L})", caller, Path.GetFileName(file), line);
            throw;
        }
    }
}
