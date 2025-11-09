using ProductCatalog.Domain.Entities;
using ProductCatalog.Services.DTOs;

namespace ProductCatalog.Services.Interfaces;

public interface ISessionService
{
    /// <summary>
    /// Create a new session for a user
    /// </summary>
    Task<UserSession> CreateSessionAsync(Guid userId, string? ipAddress, string? userAgent);

    /// <summary>
    /// Get session by ID and validate expiration
    /// </summary>
    Task<SessionInfo?> GetSessionAsync(Guid sessionId);

    /// <summary>
    /// Delete a session (logout)
    /// </summary>
    Task<bool> DeleteSessionAsync(Guid sessionId);

    /// <summary>
    /// Delete all expired sessions (cleanup job)
    /// </summary>
    Task<int> DeleteExpiredSessionsAsync();
}
