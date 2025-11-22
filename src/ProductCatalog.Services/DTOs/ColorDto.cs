namespace ProductCatalog.Services.DTOs;

public record ColorDto(
    Guid Id,
    string Name,
    string? HexCode,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateColorDto(
    string Name,
    string? HexCode = null,
    bool IsActive = true
);

public record UpdateColorDto(
    string? Name,
    string? HexCode,
    bool? IsActive
);
