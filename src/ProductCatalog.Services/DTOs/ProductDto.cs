using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Services.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string Slug,
    IReadOnlyList<Guid> CategoryIds,
    Gender Gender,
    decimal? BasePrice,
    bool IsActive,
    bool IsFeatured,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateProductDto(
    string Name,
    string? Description,
    IReadOnlyList<Guid> CategoryIds,
    Gender Gender,
    decimal? BasePrice
);

public record UpdateProductDto(
    string? Name,
    string? Description,
    IReadOnlyList<Guid>? CategoryIds,
    Gender? Gender,
    decimal? BasePrice,
    bool? IsActive,
    bool? IsFeatured
);

public record ProductQueryDto(
    string? Search,
    Guid? CategoryId,
    Gender? Gender,
    bool? IsActive,
    bool? IsFeatured,
    int Page = 1,
    int PageSize = 10
);
