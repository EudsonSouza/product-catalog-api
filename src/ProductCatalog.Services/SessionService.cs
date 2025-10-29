using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProductCatalog.Data;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.Services;

public class SessionService : ISessionService
{
    private readonly ProductCatalogDbContext _context;
    private readonly int _sessionExpirationHours;

    public SessionService(ProductCatalogDbContext context, IConfiguration configuration)
    {
        _context = context;
        _sessionExpirationHours = int.Parse(configuration["Session:ExpirationHours"] ?? "8");
    }

    public async Task<UserSession> CreateSessionAsync(Guid userId, string? ipAddress, string? userAgent)
    {
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddHours(_sessionExpirationHours),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<SessionInfo?> GetSessionAsync(Guid sessionId)
    {
        var session = await _context.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            return null;

        // Check if session is expired
        if (session.ExpiresAt < DateTime.UtcNow)
        {
            await DeleteSessionAsync(sessionId);
            return null;
        }

        return new SessionInfo
        {
            SessionId = session.Id,
            UserId = session.UserId,
            Email = session.User.Email,
            Name = session.User.Name,
            PictureUrl = session.User.PictureUrl,
            IsAdmin = session.User.IsAdmin,
            ExpiresAt = session.ExpiresAt
        };
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        var session = await _context.UserSessions.FindAsync(sessionId);
        if (session == null)
            return false;

        _context.UserSessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteExpiredSessionsAsync()
    {
        var expiredSessions = await _context.UserSessions
            .Where(s => s.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        _context.UserSessions.RemoveRange(expiredSessions);
        await _context.SaveChangesAsync();

        return expiredSessions.Count;
    }
}
