using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Data.Repositories;

public class ColorRepository : BaseRepository<Color>, IColorRepository
{
    public ColorRepository(ProductCatalogDbContext context) : base(context) { }

    public async Task<IEnumerable<Color>> GetActiveAsync()
    {
        return await DbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Color?> GetByNameAsync(string name)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
