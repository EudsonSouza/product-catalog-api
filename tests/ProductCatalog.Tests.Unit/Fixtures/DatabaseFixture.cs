using Microsoft.EntityFrameworkCore;
using ProductCatalog.Data;

namespace ProductCatalog.Tests.Unit.Fixtures;

/// <summary>
/// Base fixture for database tests using InMemory database
/// Each test class gets its own isolated database instance
/// </summary>
public class DatabaseFixture : IDisposable
{
    private readonly string _databaseName;
    public ProductCatalogDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        // Create unique database name for each test class instance
        _databaseName = $"TestDb_{Guid.NewGuid()}";

        var options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .EnableSensitiveDataLogging()
            .Options;

        Context = new ProductCatalogDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        GC.SuppressFinalize(this);
    }

    public ProductCatalogDbContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        return new ProductCatalogDbContext(options);
    }
}
