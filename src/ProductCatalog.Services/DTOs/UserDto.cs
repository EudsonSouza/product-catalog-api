namespace ProductCatalog.Services.DTOs;

public record UserDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
