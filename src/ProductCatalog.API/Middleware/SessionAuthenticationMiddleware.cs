using System.Security.Claims;
using ProductCatalog.API.Configuration;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.API.Middleware;

public class SessionAuthenticationMiddleware
{
    private const string SessionAuthenticationType = "Session";
    private const string AdminRoleName = "Admin";
    private const string AdminRoleClaimValue = "admin";
    private const string SessionIdClaimType = "session_id";

    private static readonly string[] PublicPaths =
    [
        "/health/",
        "/swagger",
        "/openapi",
        "/api/auth/google/login",
        "/api/auth/google/callback",
        "/api/products",
        "/api/categories"
    ];

    private readonly RequestDelegate _next;
    private readonly SessionSettings _sessionSettings;

    public SessionAuthenticationMiddleware(RequestDelegate next, SessionSettings sessionSettings)
    {
        _next = next;
        _sessionSettings = sessionSettings;
    }

    public async Task InvokeAsync(HttpContext context, ISessionService sessionService)
    {
        if (ShouldSkipAuthentication(context))
        {
            await _next(context);
            return;
        }

        await AuthenticateSessionAsync(context, sessionService);
        await _next(context);
    }

    private static bool ShouldSkipAuthentication(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        return IsPublicEndpoint(path);
    }

    private async Task AuthenticateSessionAsync(HttpContext context, ISessionService sessionService)
    {
        if (!TryExtractSessionId(context, out var sessionId))
            return;

        var sessionInfo = await sessionService.GetSessionAsync(sessionId);

        if (sessionInfo is null)
        {
            InvalidateSessionCookie(context);
            return;
        }

        SetUserPrincipal(context, sessionInfo);
    }

    private bool TryExtractSessionId(HttpContext context, out Guid sessionId)
    {
        sessionId = Guid.Empty;

        if (!context.Request.Cookies.TryGetValue(_sessionSettings.CookieName, out var sessionIdString))
            return false;

        return Guid.TryParse(sessionIdString, out sessionId);
    }

    private void InvalidateSessionCookie(HttpContext context) =>
        context.Response.Cookies.Delete(_sessionSettings.CookieName);

    private static void SetUserPrincipal(HttpContext context, SessionInfo sessionInfo)
    {
        var claims = BuildClaims(sessionInfo);
        var identity = new ClaimsIdentity(claims, SessionAuthenticationType);
        context.User = new ClaimsPrincipal(identity);
    }

    private static List<Claim> BuildClaims(SessionInfo sessionInfo)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sessionInfo.UserId.ToString()),
            new(ClaimTypes.Email, sessionInfo.Email),
            new(ClaimTypes.Name, sessionInfo.Name),
            new(SessionIdClaimType, sessionInfo.SessionId.ToString())
        };

        if (sessionInfo.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, AdminRoleName));
            claims.Add(new Claim("role", AdminRoleClaimValue));
        }

        return claims;
    }

    private static bool IsPublicEndpoint(string path) =>
        PublicPaths.Any(publicPath => path.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase));
}
