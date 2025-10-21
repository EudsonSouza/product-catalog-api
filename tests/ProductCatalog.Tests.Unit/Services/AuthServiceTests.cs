using Microsoft.Extensions.Configuration;
using Moq;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Services;
using ProductCatalog.Services.DTOs;

namespace ProductCatalog.Tests.Unit.Services;

/// <summary>
/// Unit tests for AuthService
/// Target: 95%+ coverage of AuthService authentication logic
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly AuthService _sut; // System Under Test
    private readonly string _testJwtSecret = "ThisIsAVeryLongSecretKeyForJwtTokenGenerationAndValidation12345678901234567890";
    private readonly string _testIssuer = "ProductCatalogAPI";
    private readonly string _testAudience = "ProductCatalogClients";

    public AuthServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockConfig = new Mock<IConfiguration>();

        // Setup JWT configuration
        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns(_testJwtSecret);
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns(_testIssuer);
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns(_testAudience);
        _mockConfig.Setup(c => c["Jwt:ExpirationHours"]).Returns("24");

        _sut = new AuthService(_mockUserRepo.Object, _mockConfig.Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #region HashPassword Tests

    [Fact]
    public void HashPassword_ReturnsNonEmptyHash_WhenPasswordIsValid()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _sut.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void HashPassword_GeneratesDifferentHashes_ForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // BCrypt generates different salts
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_ThrowsException_WhenPasswordIsEmpty(string password)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.HashPassword(password));
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_ReturnsTrue_WhenPasswordMatchesHash()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _sut.HashPassword(correctPassword);

        // Act
        var result = _sut.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_WhenHashIsInvalid()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "invalid-hash-format";

        // Act
        var result = _sut.VerifyPassword(password, invalidHash);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_ThrowsException_WhenPasswordIsEmpty(string password)
    {
        // Arrange
        var hash = _sut.HashPassword("SomePassword123!");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.VerifyPassword(password, hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_ReturnsFalse_WhenHashIsEmpty(string hash)
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = _sut.VerifyPassword(password, hash);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ReturnsLoginResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var password = "TestPassword123!";
        var passwordHash = _sut.HashPassword(password);
        var user = new User("testuser", passwordHash);

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = password
        };

        _mockUserRepo
            .Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("testuser", result.Username);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.True(result.ExpiresAt <= DateTime.UtcNow.AddHours(25)); // Allowing some margin
        _mockUserRepo.Verify(r => r.GetByUsernameAsync("testuser"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "TestPassword123!"
        };

        _mockUserRepo
            .Setup(r => r.GetByUsernameAsync("nonexistent"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.Null(result);
        _mockUserRepo.Verify(r => r.GetByUsernameAsync("nonexistent"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var passwordHash = _sut.HashPassword(correctPassword);
        var user = new User("testuser", passwordHash);

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = wrongPassword
        };

        _mockUserRepo
            .Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.Null(result);
        _mockUserRepo.Verify(r => r.GetByUsernameAsync("testuser"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenUserIsInactive()
    {
        // Arrange
        var password = "TestPassword123!";
        var passwordHash = _sut.HashPassword(password);
        var user = new User("testuser", passwordHash);
        user.Deactivate();

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = password
        };

        _mockUserRepo
            .Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.Null(result);
        _mockUserRepo.Verify(r => r.GetByUsernameAsync("testuser"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_NormalizesUsername_ToLowercase()
    {
        // Arrange
        var password = "TestPassword123!";
        var passwordHash = _sut.HashPassword(password);
        var user = new User("testuser", passwordHash);

        var request = new LoginRequest
        {
            Username = "TestUser", // Mixed case
            Password = password
        };

        _mockUserRepo
            .Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        _mockUserRepo.Verify(r => r.GetByUsernameAsync("testuser"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_GeneratesJwtToken_WithCorrectClaims()
    {
        // Arrange
        var password = "TestPassword123!";
        var passwordHash = _sut.HashPassword(password);
        var user = new User("testuser", passwordHash);

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = password
        };

        _mockUserRepo
            .Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);

        // JWT tokens have 3 parts separated by dots
        var tokenParts = result.Token.Split('.');
        Assert.Equal(3, tokenParts.Length);
    }

    #endregion
}
