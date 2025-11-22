using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.Services;

public class ColorService(IUnitOfWork unitOfWork) : IColorService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ColorDto?> GetByIdAsync(Guid id)
    {
        var color = await _unitOfWork.Colors.GetByIdAsync(id);
        return color?.ToDto();
    }

    public async Task<IEnumerable<ColorDto>> GetAllAsync()
    {
        IEnumerable<Color> colors = await _unitOfWork.Colors.GetAllAsync();
        return colors.Select(c => c.ToDto()).ToList();
    }

    public async Task<IEnumerable<ColorDto>> GetActiveAsync()
    {
        var colors = await _unitOfWork.Colors.GetActiveAsync();
        return colors.Select(c => c.ToDto()).ToList();
    }

    public async Task<ColorDto> CreateAsync(CreateColorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Name is required", nameof(dto));
        }

        Color? existing = await _unitOfWork.Colors.GetByNameAsync(dto.Name.Trim());
        if (existing != null)
        {
            throw new InvalidOperationException($"Color with name '{dto.Name}' already exists");
        }

        DateTime now = DateTime.UtcNow;
        var color = new Color
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            HexCode = dto.HexCode?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _ = await _unitOfWork.Colors.AddAsync(color);
        _ = await _unitOfWork.SaveChangesAsync();

        return color.ToDto();
    }

    public async Task<ColorDto?> UpdateAsync(Guid id, UpdateColorDto dto)
    {
        Color? color = await _unitOfWork.Colors.GetByIdAsync(id);
        if (color == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name) && !string.Equals(dto.Name, color.Name, StringComparison.Ordinal))
        {
            var existing = await _unitOfWork.Colors.GetByNameAsync(dto.Name.Trim());
            if (existing != null && existing.Id != id)
                throw new InvalidOperationException($"Color with name '{dto.Name}' already exists");

            color.Name = dto.Name.Trim();
        }

        if (dto.HexCode != null)
            color.HexCode = string.IsNullOrWhiteSpace(dto.HexCode) ? null : dto.HexCode.Trim();

        if (dto.IsActive.HasValue)
            color.IsActive = dto.IsActive.Value;

        color.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Colors.UpdateAsync(color);
        await _unitOfWork.SaveChangesAsync();

        return color.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var exists = await _unitOfWork.Colors.GetByIdAsync(id);
        if (exists == null)
            return false;

        await _unitOfWork.Colors.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

public static class ColorExtensions
{
    public static ColorDto ToDto(this Color color)
    {
        return new ColorDto(
            color.Id,
            color.Name,
            color.HexCode,
            color.IsActive,
            color.CreatedAt,
            color.UpdatedAt
        );
    }
}
