using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ProductCatalog.Data;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Services;
using AwesomeAssertions;

namespace ProductCatalog.Tests.Unit.Services;

public class SessionServiceTests : IDisposable
{
    private readonly ProductCatalogDbContext _context;
    private readonly SessionService _service;
    private readonly User _testUser;

    public SessionServiceTests()
    {
        var options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductCatalogDbContext(options);

        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Session:ExpirationHours"]).Returns("8");

        _service = new SessionService(_context, config.Object);

        // Seed test user
        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            IsAdmin = false
        };
        _context.Users.Add(_testUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldCreateValidSession()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act
        var result = await _service.CreateSessionAsync(_testUser.Id, ipAddress, userAgent);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.UserId.Should().Be(_testUser.Id);
        result.IpAddress.Should().Be(ipAddress);
        result.UserAgent.Should().Be(userAgent);
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(8), TimeSpan.FromSeconds(5));
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldPersistToDatabase()
    {
        // Act
        var session = await _service.CreateSessionAsync(_testUser.Id, "127.0.0.1", "test-agent");

        // Assert
        var savedSession = await _context.UserSessions.FindAsync(session.Id);
        savedSession.Should().NotBeNull();
        savedSession!.UserId.Should().Be(_testUser.Id);
    }

    [Fact]
    public async Task GetSessionAsync_WithValidSession_ShouldReturnSessionInfo()
    {
        // Arrange
        var session = await _service.CreateSessionAsync(_testUser.Id, "127.0.0.1", "agent");

        // Act
        var result = await _service.GetSessionAsync(session.Id);

        // Assert
        result.Should().NotBeNull();
        result!.SessionId.Should().Be(session.Id);
        result.UserId.Should().Be(_testUser.Id);
        result.Email.Should().Be(_testUser.Email);
        result.Name.Should().Be(_testUser.Name);
        result.IsAdmin.Should().Be(_testUser.IsAdmin);
    }

    [Fact]
    public async Task GetSessionAsync_WithNonExistentSession_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _service.GetSessionAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSessionAsync_WithExpiredSession_ShouldReturnNullAndDeleteSession()
    {
        // Arrange
        var expiredSession = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = _testUser.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired 1 hour ago
            CreatedAt = DateTime.UtcNow.AddHours(-9)
        };
        _context.UserSessions.Add(expiredSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetSessionAsync(expiredSession.Id);

        // Assert
        result.Should().BeNull();

        // Verify session was deleted
        var deletedSession = await _context.UserSessions.FindAsync(expiredSession.Id);
        deletedSession.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSessionAsync_WithExistingSession_ShouldReturnTrueAndDelete()
    {
        // Arrange
        var session = await _service.CreateSessionAsync(_testUser.Id, "127.0.0.1", "agent");

        // Act
        var result = await _service.DeleteSessionAsync(session.Id);

        // Assert
        result.Should().BeTrue();

        var deletedSession = await _context.UserSessions.FindAsync(session.Id);
        deletedSession.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSessionAsync_WithNonExistentSession_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _service.DeleteSessionAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteExpiredSessionsAsync_ShouldDeleteOnlyExpiredSessions()
    {
        // Arrange
        var validSession = await _service.CreateSessionAsync(_testUser.Id, "127.0.0.1", "agent1");

        var expiredSession1 = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = _testUser.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow.AddHours(-9)
        };
        var expiredSession2 = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = _testUser.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(-2),
            CreatedAt = DateTime.UtcNow.AddHours(-10)
        };
        _context.UserSessions.AddRange(expiredSession1, expiredSession2);
        await _context.SaveChangesAsync();

        // Act
        var deletedCount = await _service.DeleteExpiredSessionsAsync();

        // Assert
        deletedCount.Should().Be(2);

        var remainingSessions = await _context.UserSessions.ToListAsync();
        remainingSessions.Should().HaveCount(1);
        remainingSessions[0].Id.Should().Be(validSession.Id);
    }

    [Fact]
    public async Task DeleteExpiredSessionsAsync_WithNoExpiredSessions_ShouldReturnZero()
    {
        // Arrange
        await _service.CreateSessionAsync(_testUser.Id, "127.0.0.1", "agent");

        // Act
        var deletedCount = await _service.DeleteExpiredSessionsAsync();

        // Assert
        deletedCount.Should().Be(0);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
