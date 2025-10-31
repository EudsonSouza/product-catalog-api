using ProductCatalog.Services.DTOs;

namespace ProductCatalog.Services.Interfaces;

public interface IGoogleOAuthService
{
    /// <summary>
    /// Generate PKCE code verifier and challenge for OAuth flow
    /// </summary>
    PkceData GeneratePkceData();

    /// <summary>
    /// Build Google OAuth authorization URL
    /// </summary>
    string BuildAuthorizationUrl(string codeChallenge, string state, string redirectUri);

    /// <summary>
    /// Exchange authorization code for ID token and validate it
    /// </summary>
    Task<GoogleUserInfo?> ExchangeCodeForUserInfoAsync(string code, string codeVerifier, string redirectUri);
}
