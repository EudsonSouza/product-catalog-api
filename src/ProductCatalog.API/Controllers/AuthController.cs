using Microsoft.AspNetCore.Mvc;
using ProductCatalog.API.Configuration;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string PkceVerifierCookieName = "pkce_verifier";
    private const string PkceStateCookieName = "pkce_state";
    private const string ReturnUrlCookieName = "return_url";
    private const string CallbackPath = "/api/auth/google/callback";
    private const string DefaultReturnUrl = "/";

    private const int PkceCookieExpirationMinutes = 5;

    private const string MissingPkceDataError = "Missing PKCE data";
    private const string InvalidStateError = "Invalid state parameter";
    private const string GoogleAuthenticationFailedError = "Failed to authenticate with Google";
    private const string NotAuthenticatedError = "Not authenticated";
    private const string SessionExpiredError = "Session expired or invalid";
    private const string LogoutSuccessMessage = "Logged out successfully";

    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly IUserService _userService;
    private readonly ISessionService _sessionService;
    private readonly SessionSettings _sessionSettings;
    private readonly FrontendSettings _frontendSettings;

    public AuthController(
        IGoogleOAuthService googleOAuthService,
        IUserService userService,
        ISessionService sessionService,
        SessionSettings sessionSettings,
        FrontendSettings frontendSettings)
    {
        _googleOAuthService = googleOAuthService;
        _userService = userService;
        _sessionService = sessionService;
        _sessionSettings = sessionSettings;
        _frontendSettings = frontendSettings;
    }

    /// <summary>
    /// Initiate Google OAuth flow
    /// </summary>
    [HttpGet("google/login")]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
    {
        var pkceData = _googleOAuthService.GeneratePkceData();
        var redirectUri = BuildCallbackUri();

        StorePkceDataInCookies(pkceData);
        StoreReturnUrlIfProvided(returnUrl);

        var authUrl = _googleOAuthService.BuildAuthorizationUrl(
            pkceData.CodeChallenge,
            pkceData.State,
            redirectUri
        );

        return Redirect(authUrl);
    }

    /// <summary>
    /// OAuth callback endpoint
    /// </summary>
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        var pkceValidationResult = ValidatePkceData(state);
        if (pkceValidationResult != null)
        {
            return pkceValidationResult;
        }

        var codeVerifier = Request.Cookies[PkceVerifierCookieName]!;
        var redirectUri = BuildCallbackUri();

        var googleUserInfo = await _googleOAuthService.ExchangeCodeForUserInfoAsync(code, codeVerifier, redirectUri);
        if (googleUserInfo == null)
        {
            return Unauthorized(new { error = GoogleAuthenticationFailedError });
        }

        var user = await _userService.FindOrCreateUserAsync(googleUserInfo);
        var session = await CreateUserSessionAsync(user.Id);

        SetSessionCookie(session.Id);
        CleanupPkceCookies();

        var returnUrl = GetAndCleanupReturnUrl();
        return Redirect(returnUrl);
    }

    /// <summary>
    /// Get current authenticated user info
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<SessionInfo>> GetCurrentUser()
    {
        if (!TryGetSessionIdFromCookie(out var sessionId))
        {
            return Unauthorized(new { error = NotAuthenticatedError });
        }

        var sessionInfo = await _sessionService.GetSessionAsync(sessionId);

        if (sessionInfo == null)
        {
            Response.Cookies.Delete(_sessionSettings.CookieName);
            return Unauthorized(new { error = SessionExpiredError });
        }

        return Ok(sessionInfo);
    }

    /// <summary>
    /// Logout (delete session)
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (TryGetSessionIdFromCookie(out var sessionId))
        {
            await _sessionService.DeleteSessionAsync(sessionId);
        }

        Response.Cookies.Delete(_sessionSettings.CookieName);

        return Ok(new { message = LogoutSuccessMessage });
    }

    private string BuildCallbackUri()
    {
        return $"{Request.Scheme}://{Request.Host}{CallbackPath}";
    }

    private void StorePkceDataInCookies(PkceData pkceData)
    {
        var cookieOptions = CreateTemporaryCookieOptions();

        Response.Cookies.Append(PkceVerifierCookieName, pkceData.CodeVerifier, cookieOptions);
        Response.Cookies.Append(PkceStateCookieName, pkceData.State, cookieOptions);
    }

    private void StoreReturnUrlIfProvided(string? returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            return;
        }

        var cookieOptions = CreateTemporaryCookieOptions();
        Response.Cookies.Append(ReturnUrlCookieName, returnUrl, cookieOptions);
    }

    private CookieOptions CreateTemporaryCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(PkceCookieExpirationMinutes)
        };
    }

    private CookieOptions CreateSessionCookieOptions()
    {
        // Allow insecure cookies when frontend is localhost (staging testing)
        var isLocalFrontend = _frontendSettings.BaseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase);

        return new CookieOptions
        {
            HttpOnly = true,
            // None required for cross-origin (localhost → staging)
            Secure = !isLocalFrontend,
            SameSite = isLocalFrontend ? SameSiteMode.None : SameSiteMode.Lax,
            MaxAge = TimeSpan.FromHours(_sessionSettings.ExpirationHours)
        };
    }

    private IActionResult? ValidatePkceData(string state)
    {
        if (!Request.Cookies.TryGetValue(PkceVerifierCookieName, out var codeVerifier) ||
            !Request.Cookies.TryGetValue(PkceStateCookieName, out var storedState))
        {
            return BadRequest(new { error = MissingPkceDataError });
        }

        if (state != storedState)
        {
            return BadRequest(new { error = InvalidStateError });
        }

        return null;
    }

    private async Task<Domain.Entities.UserSession> CreateUserSessionAsync(Guid userId)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        return await _sessionService.CreateSessionAsync(userId, ipAddress, userAgent);
    }

    private void SetSessionCookie(Guid sessionId)
    {
        var cookieOptions = CreateSessionCookieOptions();
        Response.Cookies.Append(_sessionSettings.CookieName, sessionId.ToString(), cookieOptions);
    }

    private void CleanupPkceCookies()
    {
        Response.Cookies.Delete(PkceVerifierCookieName);
        Response.Cookies.Delete(PkceStateCookieName);
    }

    private string GetAndCleanupReturnUrl()
    {
        var relativePath = Request.Cookies[ReturnUrlCookieName] ?? DefaultReturnUrl;
        Response.Cookies.Delete(ReturnUrlCookieName);
        return $"{_frontendSettings.BaseUrl}{relativePath}";
    }

    private bool TryGetSessionIdFromCookie(out Guid sessionId)
    {
        sessionId = Guid.Empty;

        if (!Request.Cookies.TryGetValue(_sessionSettings.CookieName, out var sessionIdStr))
        {
            return false;
        }

        return Guid.TryParse(sessionIdStr, out sessionId);
    }
}
