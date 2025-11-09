using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProductCatalog.Data;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.Services
{
    public class SessionService : ISessionService
    {
        private const string SessionExpirationConfigKey = "Session:ExpirationHours";
        private const string DefaultSessionExpirationHours = "8";

        private readonly ProductCatalogDbContext _context;
        private readonly int _sessionExpirationHours;

        public SessionService(ProductCatalogDbContext context, IConfiguration configuration)
        {
            _context = context;
            _sessionExpirationHours = LoadSessionExpirationFromConfiguration(configuration);
        }

        public async Task<UserSession> CreateSessionAsync(Guid userId, string? ipAddress, string? userAgent)
        {
            var session = BuildSession(userId, ipAddress, userAgent);

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<SessionInfo?> GetSessionAsync(Guid sessionId)
        {
            var session = await FindSessionWithUserAsync(sessionId);

            if (session == null)
            {
                return null;
            }

            if (IsSessionExpired(session))
            {
                await DeleteSessionAsync(sessionId);
                return null;
            }

            return ConvertToSessionInfo(session);
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
            var expiredSessions = await FindExpiredSessionsAsync();

            _context.UserSessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync();

            return expiredSessions.Count;
        }

        private static int LoadSessionExpirationFromConfiguration(IConfiguration configuration)
        {
            var expirationHours = configuration[SessionExpirationConfigKey] ?? DefaultSessionExpirationHours;
            return int.Parse(expirationHours);
        }

        private UserSession BuildSession(Guid userId, string? ipAddress, string? userAgent)
        {
            return new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExpiresAt = CalculateExpirationTime(),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };
        }

        private DateTime CalculateExpirationTime()
        {
            return DateTime.UtcNow.AddHours(_sessionExpirationHours);
        }

        private async Task<UserSession?> FindSessionWithUserAsync(Guid sessionId)
        {
            return await _context.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        private static bool IsSessionExpired(UserSession session)
        {
            return session.ExpiresAt < DateTime.UtcNow;
        }

        private static SessionInfo ConvertToSessionInfo(UserSession session)
        {
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

        private async Task<List<UserSession>> FindExpiredSessionsAsync()
        {
            return await _context.UserSessions
                .Where(s => s.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
