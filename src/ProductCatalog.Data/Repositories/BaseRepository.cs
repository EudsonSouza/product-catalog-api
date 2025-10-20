using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Data.Repositories;

public class BaseRepository<T> : IRepository<T> where T : class
{
    private readonly ProductCatalogDbContext _context;
    private readonly DbSet<T> _dbSet;

    protected ProductCatalogDbContext Context => _context;
    protected DbSet<T> DbSet => _dbSet;

    protected BaseRepository(ProductCatalogDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var entry = await DbSet.AddAsync(entity);
        return entry.Entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
            DbSet.Remove(entity);
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await DbSet.FindAsync(id) != null;
    }
}
