using ProductCatalog.Services.DTOs;

namespace ProductCatalog.Services.Interfaces;

public interface IColorService
{
    Task<ColorDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ColorDto>> GetAllAsync();
    Task<IEnumerable<ColorDto>> GetActiveAsync();
    Task<ColorDto> CreateAsync(CreateColorDto dto);
    Task<ColorDto?> UpdateAsync(Guid id, UpdateColorDto dto);
    Task<bool> DeleteAsync(Guid id);
}
