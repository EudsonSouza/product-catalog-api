using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProductCatalog.Data;

namespace ProductCatalog.API.Health;

public sealed class DbHealthCheck : IHealthCheck
{
    private readonly ProductCatalogDbContext _db;

    public DbHealthCheck(ProductCatalogDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            sw.Stop();

            IReadOnlyDictionary<string, object> data = new Dictionary<string, object>
            {
                ["responseTimeMs"] = sw.ElapsedMilliseconds
            };

            return HealthCheckResult.Healthy("DB query OK.", data);
        }
        catch (Exception ex)
        {
            sw.Stop();

            IReadOnlyDictionary<string, object> data = new Dictionary<string, object>
            {
                ["responseTimeMs"] = sw.ElapsedMilliseconds,
                ["exception"] = ex.Message,
                ["exceptionType"] = ex.GetType().FullName ?? "UnknownException"
            };

            return HealthCheckResult.Unhealthy("DB query failed.", ex, data);
        }
    }
}
