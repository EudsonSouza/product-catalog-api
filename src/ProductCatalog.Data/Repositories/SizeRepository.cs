using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Data.Repositories;

public class SizeRepository : BaseRepository<Size>, ISizeRepository
{
    public SizeRepository(ProductCatalogDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Size>> GetActiveAsync()
    {
        return await DbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Size?> GetByNameAsync(string name)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
