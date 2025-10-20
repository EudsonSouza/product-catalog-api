using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Data.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ProductCatalogDbContext context) : base(context) { }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
    {
        return await DbSet
            .Where(p => p.IsActive && p.Categories.Any(c => c.Id == categoryId))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByGenderAsync(string gender)
    {
        return await DbSet
            .Where(p => p.Gender.ToString() == gender && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetFeaturedAsync()
    {
        return await DbSet
            .Where(p => p.IsFeatured && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm)
    {
        var normalizedTerm = searchTerm.ToLower().Trim();

        return await DbSet
            .Where(p => p.Name.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase) && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsBySlugAsync(string slug)
    {
        return await DbSet
            .AnyAsync(p => p.Slug == slug);
    }

    public async Task<Product?> GetWithVariantsAsync(Guid id)
    {
        return await DbSet
            .Include(p => p.Variants)
                .ThenInclude(v => v.Color)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Size)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetWithImagesAsync(Guid id)
    {
        return await DbSet
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetCompleteAsync(Guid id)
    {
        return await DbSet
            .Include(p => p.Categories)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Color)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Size)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
