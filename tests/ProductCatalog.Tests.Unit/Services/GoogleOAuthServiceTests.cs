using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ProductCatalog.Services;
using ProductCatalog.Services.DTOs;
using AwesomeAssertions;

namespace ProductCatalog.Tests.Unit.Services;

public class GoogleOAuthServiceTests : IDisposable
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<GoogleOAuthService>> _loggerMock;
    private readonly GoogleOAuthService _service;

    public GoogleOAuthServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Google:ClientId"]).Returns("test-client-id.apps.googleusercontent.com");
        _configurationMock.Setup(c => c["Google:ClientSecret"]).Returns("test-client-secret");

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        _loggerMock = new Mock<ILogger<GoogleOAuthService>>();

        _service = new GoogleOAuthService(_configurationMock.Object, _httpClient, _loggerMock.Object);
    }

    [Fact]
    public void GeneratePkceData_ShouldReturnValidData()
    {
        // Act
        var result = _service.GeneratePkceData();

        // Assert
        result.Should().NotBeNull();
        result.CodeVerifier.Should().NotBeNullOrWhiteSpace();
        result.CodeVerifier.Length.Should().Be(64);
        result.CodeChallenge.Should().NotBeNullOrWhiteSpace();
        result.State.Should().NotBeNullOrWhiteSpace();
        result.State.Length.Should().Be(32);
    }

    [Fact]
    public void GeneratePkceData_ShouldGenerateUniqueValues()
    {
        // Act
        var result1 = _service.GeneratePkceData();
        var result2 = _service.GeneratePkceData();

        // Assert
        result1.CodeVerifier.Should().NotBe(result2.CodeVerifier);
        result1.CodeChallenge.Should().NotBe(result2.CodeChallenge);
        result1.State.Should().NotBe(result2.State);
    }

    [Fact]
    public void BuildAuthorizationUrl_ShouldContainRequiredParameters()
    {
        // Arrange
        var codeChallenge = "test-code-challenge";
        var state = "test-state";
        var redirectUri = "http://localhost:5182/api/auth/google/callback";

        // Act
        var result = _service.BuildAuthorizationUrl(codeChallenge, state, redirectUri);

        // Assert
        result.Should().Contain("https://accounts.google.com/o/oauth2/v2/auth");
        result.Should().Contain($"client_id=test-client-id.apps.googleusercontent.com");
        result.Should().Contain($"redirect_uri={Uri.EscapeDataString(redirectUri)}");
        result.Should().Contain("response_type=code");
        result.Should().Contain("scope=");
        result.Should().Contain($"code_challenge={Uri.EscapeDataString(codeChallenge)}");
        result.Should().Contain("code_challenge_method=S256");
        result.Should().Contain($"state={Uri.EscapeDataString(state)}");
        result.Should().Contain("access_type=offline");
        result.Should().Contain("prompt=consent");
    }

    [Fact]
    public async Task ExchangeCodeForUserInfoAsync_WithValidToken_ShouldReturnNull()
    {
        // Arrange
        var code = "test-code";
        var codeVerifier = "test-verifier";
        var redirectUri = "http://localhost:5182/api/auth/google/callback";

        var tokenResponse = new
        {
            access_token = "test-access-token",
            id_token = "invalid-token", // Token inválido para teste
            expires_in = 3600,
            token_type = "Bearer"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("oauth2.googleapis.com/token")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            });

        // Act
        var result = await _service.ExchangeCodeForUserInfoAsync(code, codeVerifier, redirectUri);

        // Assert - Esperamos null porque o token é inválido
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExchangeCodeForUserInfoAsync_WithFailedTokenExchange_ShouldReturnNull()
    {
        // Arrange
        var code = "invalid-code";
        var codeVerifier = "test-verifier";
        var redirectUri = "http://localhost:5182/api/auth/google/callback";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{\"error\":\"invalid_grant\"}")
            });

        // Act
        var result = await _service.ExchangeCodeForUserInfoAsync(code, codeVerifier, redirectUri);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMissingClientId_ShouldThrowException()
    {
        // Arrange
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Google:ClientId"]).Returns((string?)null);
        config.Setup(c => c["Google:ClientSecret"]).Returns("secret");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new GoogleOAuthService(config.Object, _httpClient, _loggerMock.Object));

        exception.Message.Should().Contain("Google:ClientId");
    }

    [Fact]
    public void Constructor_WithMissingClientSecret_ShouldThrowException()
    {
        // Arrange
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Google:ClientId"]).Returns("client-id");
        config.Setup(c => c["Google:ClientSecret"]).Returns((string?)null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new GoogleOAuthService(config.Object, _httpClient, _loggerMock.Object));

        exception.Message.Should().Contain("Google:ClientSecret");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
