using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Product>> GetByGenderAsync(string gender);
    Task<IEnumerable<Product>> GetFeaturedAsync();
    Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm);
    Task<bool> ExistsBySlugAsync(string slug);
    Task<Product?> GetWithVariantsAsync(Guid id);
    Task<Product?> GetWithImagesAsync(Guid id);
    Task<Product?> GetCompleteAsync(Guid id);
}
