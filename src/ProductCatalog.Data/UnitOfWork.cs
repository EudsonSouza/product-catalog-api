using Microsoft.EntityFrameworkCore.Storage;
using ProductCatalog.Data.Repositories;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ProductCatalogDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy-loaded repositories
    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private IColorRepository? _colors;
    private ISizeRepository? _sizes;

    public UnitOfWork(ProductCatalogDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_context);

    public IColorRepository Colors =>
        _colors ??= new ColorRepository(_context);

    public ISizeRepository Sizes =>
        _sizes ??= new SizeRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}