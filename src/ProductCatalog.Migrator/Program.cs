using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductCatalog.Data;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var cs =
            ctx.Configuration.GetConnectionString("DefaultConnection")
         ?? ctx.Configuration.GetConnectionString("Default")
         ?? ctx.Configuration["ConnectionStrings__DefaultConnection"]
         ?? throw new InvalidOperationException("Connection string not found.");

        services.AddDbContext<ProductCatalogDbContext>(o => o.UseNpgsql(cs));
    })
    .Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
Console.WriteLine("Applying EF Core migrations...");
await db.Database.MigrateAsync();
Console.WriteLine("EF Core migrations applied successfully.");
