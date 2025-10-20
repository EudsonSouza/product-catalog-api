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
    private readonly DbContextOptions<ProductCatalogDbContext> _options;
    public ProductCatalogDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        // Create unique database name for each test class instance
        _databaseName = $"TestDb_{Guid.NewGuid()}";

        _options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .EnableSensitiveDataLogging()
            .Options;

        Context = new ProductCatalogDbContext(_options);
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Clears the change tracker to avoid entity tracking conflicts between tests
    /// </summary>
    public void ClearChangeTracker()
    {
        Context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Recreates the context to ensure a clean state
    /// </summary>
    public void RecreateContext()
    {
        Context.Dispose();
        Context = new ProductCatalogDbContext(_options);
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
