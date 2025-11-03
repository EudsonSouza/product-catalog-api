using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.Services;

public class GoogleOAuthService : IGoogleOAuthService
{
    private const string GoogleAuthorizationBaseUrl = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string GoogleTokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string GoogleScopes = "openid profile email";
    private const string CodeChallengeMethod = "S256";
    private const string ResponseType = "code";
    private const string GrantType = "authorization_code";
    private const string AccessType = "offline";
    private const string Prompt = "consent";

    private const int CodeVerifierLength = 64;
    private const int StateLength = 32;
    private const string UrlSafeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";

    private const string GoogleClientIdConfigKey = "Google:ClientId";
    private const string GoogleClientSecretConfigKey = "Google:ClientSecret";
    private const string ClientIdNotConfiguredMessage = "Google:ClientId is not configured";
    private const string ClientSecretNotConfiguredMessage = "Google:ClientSecret is not configured";
    private const string TokenExchangeNullMessage = "Token exchange returned null or empty ID token";
    private const string InvalidTokenMessage = "Invalid Google token provided";
    private const string CriticalTokenValidationMessage = "Critical error validating Google token";
    private const string TokenExchangeFailedMessage = "Token exchange failed with status code: {StatusCode}";

    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleOAuthService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GoogleOAuthService(
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<GoogleOAuthService> logger)
    {
        _clientId = configuration[GoogleClientIdConfigKey]
            ?? throw new InvalidOperationException(ClientIdNotConfiguredMessage);
        _clientSecret = configuration[GoogleClientSecretConfigKey]
            ?? throw new InvalidOperationException(ClientSecretNotConfiguredMessage);
        _httpClient = httpClient;
        _logger = logger;
    }

    public PkceData GeneratePkceData()
    {
        var codeVerifier = GenerateRandomString(CodeVerifierLength);
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = GenerateRandomString(StateLength);

        return new PkceData
        {
            CodeVerifier = codeVerifier,
            CodeChallenge = codeChallenge,
            State = state
        };
    }

    public string BuildAuthorizationUrl(string codeChallenge, string state, string redirectUri)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "redirect_uri", redirectUri },
            { "response_type", ResponseType },
            { "scope", GoogleScopes },
            { "code_challenge", codeChallenge },
            { "code_challenge_method", CodeChallengeMethod },
            { "state", state },
            { "access_type", AccessType },
            { "prompt", Prompt }
        };

        var queryString = BuildQueryString(queryParams);
        return $"{GoogleAuthorizationBaseUrl}?{queryString}";
    }

    private static string BuildQueryString(Dictionary<string, string> parameters)
    {
        var escapedParams = parameters.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");

        return string.Join("&", escapedParams);
    }

    public async Task<GoogleUserInfo?> ExchangeCodeForUserInfoAsync(string code, string codeVerifier, string redirectUri)
    {
        try
        {
            var tokenResponse = await ExchangeCodeForTokensAsync(code, codeVerifier, redirectUri);
            if (!IsValidTokenResponse(tokenResponse))
            {
                _logger.LogWarning(TokenExchangeNullMessage);
                return null;
            }

            var payload = await ValidateGoogleTokenAsync(tokenResponse!.IdToken!);
            return ConvertPayloadToUserInfo(payload);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, InvalidTokenMessage);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CriticalTokenValidationMessage);
            throw;
        }
    }

    private static bool IsValidTokenResponse(TokenResponse? tokenResponse)
    {
        return tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.IdToken);
    }

    private static GoogleUserInfo ConvertPayloadToUserInfo(GoogleJsonWebSignature.Payload payload)
    {
        return new GoogleUserInfo
        {
            GoogleId = payload.Subject,
            Email = payload.Email,
            Name = payload.Name ?? payload.Email,
            PictureUrl = payload.Picture,
            EmailVerified = payload.EmailVerified
        };
    }

    private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
    {
        return await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _clientId }
        });
    }

    private async Task<TokenResponse?> ExchangeCodeForTokensAsync(string code, string codeVerifier, string redirectUri)
    {
        var requestData = BuildTokenRequestData(code, codeVerifier, redirectUri);
        var response = await _httpClient.PostAsync(GoogleTokenEndpoint, new FormUrlEncodedContent(requestData));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(TokenExchangeFailedMessage, response.StatusCode);
            return null;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Google token response: {Response}", jsonResponse);
        return DeserializeTokenResponse(jsonResponse);
    }

    private static TokenResponse? DeserializeTokenResponse(string jsonResponse)
    {
        return JsonSerializer.Deserialize<TokenResponse>(jsonResponse, JsonOptions);
    }

    private Dictionary<string, string> BuildTokenRequestData(string code, string codeVerifier, string redirectUri)
    {
        return new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", _clientId },
            { "client_secret", _clientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", GrantType },
            { "code_verifier", codeVerifier }
        };
    }

    private static string GenerateRandomString(int length)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(length);
        var result = new StringBuilder(length);

        foreach (var randomByte in randomBytes)
        {
            result.Append(UrlSafeCharacters[randomByte % UrlSafeCharacters.Length]);
        }

        return result.ToString();
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }
}
