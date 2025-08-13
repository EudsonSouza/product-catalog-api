using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductCatalog.Data;

static string NormalizePgConnection(string? raw)
{
    if (string.IsNullOrWhiteSpace(raw))
        throw new InvalidOperationException("Connection string not found.");

    if (raw.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
        raw.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        return raw;

    if (raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var url = raw.Replace("postgresql://", "postgres://", StringComparison.OrdinalIgnoreCase);
        var uri = new Uri(url);

        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var host = uri.Host;
        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

        string sslmode = "Require";
        if (!string.IsNullOrEmpty(uri.Query))
        {
            foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 2 && kv[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                    sslmode = Uri.UnescapeDataString(kv[1]);
            }
        }

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslmode};Trust Server Certificate=true";
    }

    throw new ArgumentException("Unsupported connection string format.");
}

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var raw =
            ctx.Configuration.GetConnectionString("DefaultConnection")
         ?? ctx.Configuration.GetConnectionString("Default")
         ?? ctx.Configuration["ConnectionStrings__DefaultConnection"]
         ?? ctx.Configuration["DATABASE_URL"]
         ?? throw new InvalidOperationException("Connection string not found.");

        var cs = NormalizePgConnection(raw);

        services.AddDbContext<ProductCatalogDbContext>(o => o.UseNpgsql(cs));
    })
    .Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
Console.WriteLine("Applying EF Core migrations...");
await db.Database.MigrateAsync();
Console.WriteLine("EF Core migrations applied successfully.");
