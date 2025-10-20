using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
    Task<IEnumerable<Category>> GetByGenderAsync(string gender);
    Task<IEnumerable<Category>> GetActiveAsync();
    Task<Category?> GetWithProductsAsync(Guid id);
}
