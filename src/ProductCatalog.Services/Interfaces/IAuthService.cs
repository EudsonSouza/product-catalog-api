using ProductCatalog.Services.DTOs;

namespace ProductCatalog.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<RegisterResponse?> RegisterAsync(RegisterRequest request);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
