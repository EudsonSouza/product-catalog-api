namespace ProductCatalog.Services.DTOs;

public class SessionInfo
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime ExpiresAt { get; set; }
}
