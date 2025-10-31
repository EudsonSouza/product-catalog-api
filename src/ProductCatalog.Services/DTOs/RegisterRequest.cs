using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Services.DTOs;

public record RegisterRequest
{
    private const int UsernameMinLength = 3;
    private const int UsernameMaxLength = 100;
    private const int EmailMaxLength = 256;
    private const int PasswordMinLength = 8;
    private const int PasswordMaxLength = 100;
    private const int NameMaxLength = 100;

    [Required(ErrorMessage = "Username is required")]
    [StringLength(UsernameMaxLength, MinimumLength = UsernameMinLength,
        ErrorMessage = "Username must be between 3 and 100 characters")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(EmailMaxLength, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(PasswordMaxLength, MinimumLength = PasswordMinLength,
        ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; init; } = string.Empty;

    [StringLength(NameMaxLength, ErrorMessage = "First name cannot exceed 100 characters")]
    public string? FirstName { get; init; }

    [StringLength(NameMaxLength, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string? LastName { get; init; }
}
