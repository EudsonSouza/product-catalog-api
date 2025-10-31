using ProductCatalog.Domain.Entities;
using ProductCatalog.Services.DTOs;

namespace ProductCatalog.Services.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Find user by email or create new one
    /// </summary>
    Task<User> FindOrCreateUserAsync(GoogleUserInfo googleUserInfo);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Check if user email is whitelisted as admin
    /// </summary>
    bool IsAdminEmail(string email);
}
