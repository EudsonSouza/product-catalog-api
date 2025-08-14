using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductCatalog.Data;
using ProductCatalog.Data.Helpers;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var rawConnectionString = ctx.Configuration.GetConnectionString("DefaultConnection")
                                ?? ctx.Configuration.GetConnectionString("Default")
                                ?? ctx.Configuration["ConnectionStrings__DefaultConnection"]
                                ?? ctx.Configuration["DATABASE_URL"]
                                ?? throw new InvalidOperationException("Connection string not found.");

        var connectionString = DatabaseConnectionHelper.ConvertToNpgsqlConnectionString(rawConnectionString);

        services.AddDbContext<ProductCatalogDbContext>(options => options.UseNpgsql(connectionString));
    })
    .Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

Console.WriteLine("🔄 Applying EF Core migrations...");
await db.Database.MigrateAsync();
Console.WriteLine("✅ EF Core migrations applied successfully.");

Console.WriteLine("🌱 Starting database seed...");
await ProductCatalog.Data.Seed.ProductCatalogSeeder.SeedAsync(db);
Console.WriteLine("🎉 Database setup completed!");
