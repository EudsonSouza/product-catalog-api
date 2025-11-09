using ProductCatalog.Domain.Entities;
using AwesomeAssertions;

namespace ProductCatalog.Tests.Unit.Domain;

public class UserSessionTests
{
    [Fact]
    public void UserSession_ShouldInitializeWithDefaultValues()
    {
        // Act
        var session = new UserSession();

        // Assert
        session.Id.Should().BeEmpty();
        session.UserId.Should().BeEmpty();
        session.ExpiresAt.Should().Be(default);
        session.IpAddress.Should().BeNull();
        session.UserAgent.Should().BeNull();
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UserSession_ShouldAcceptAllProperties()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(8);
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";
        var createdAt = DateTime.UtcNow;

        // Act
        var session = new UserSession
        {
            Id = sessionId,
            UserId = userId,
            ExpiresAt = expiresAt,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = createdAt
        };

        // Assert
        session.Id.Should().Be(sessionId);
        session.UserId.Should().Be(userId);
        session.ExpiresAt.Should().Be(expiresAt);
        session.IpAddress.Should().Be(ipAddress);
        session.UserAgent.Should().Be(userAgent);
        session.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void UserSession_UserNavigation_ShouldAllowSettingUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User"
        };

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id
        };

        // Act
        session.User = user;

        // Assert
        session.User.Should().NotBeNull();
        session.User.Id.Should().Be(user.Id);
        session.User.Email.Should().Be(user.Email);
    }

    [Fact]
    public void UserSession_ShouldAcceptNullableFields()
    {
        // Act
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            IpAddress = null,
            UserAgent = null
        };

        // Assert
        session.IpAddress.Should().BeNull();
        session.UserAgent.Should().BeNull();
    }
}
