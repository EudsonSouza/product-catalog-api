namespace ProductCatalog.Services.DTOs;

public class GoogleUserInfo
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public string GoogleId { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
}
