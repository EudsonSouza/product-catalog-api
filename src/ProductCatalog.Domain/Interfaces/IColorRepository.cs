using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Interfaces;

public interface IColorRepository : IRepository<Color>
{
    Task<IEnumerable<Color>> GetActiveAsync();
    Task<Color?> GetByNameAsync(string name);
}