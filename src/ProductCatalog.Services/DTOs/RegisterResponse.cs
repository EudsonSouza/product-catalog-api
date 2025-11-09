namespace ProductCatalog.Services.DTOs;

public record RegisterResponse
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string Message { get; init; }
}
