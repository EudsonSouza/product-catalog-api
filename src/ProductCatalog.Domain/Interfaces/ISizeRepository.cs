using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Interfaces;

public interface ISizeRepository : IRepository<Size>
{
    Task<IEnumerable<Size>> GetActiveAsync();
    Task<Size?> GetByNameAsync(string name);
}