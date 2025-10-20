namespace ProductCatalog.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repositories
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IColorRepository Colors { get; }
    ISizeRepository Sizes { get; }

    // Transaction Management
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
