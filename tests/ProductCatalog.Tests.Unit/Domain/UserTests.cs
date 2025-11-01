using ProductCatalog.Domain.Entities;
using AwesomeAssertions;

namespace ProductCatalog.Tests.Unit.Domain;

public class UserTests
{
    [Fact]
    public void User_ShouldInitializeWithDefaultValues()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().BeEmpty();
        user.Email.Should().BeEmpty();
        user.Name.Should().BeEmpty();
        user.PictureUrl.Should().BeNull();
        user.IsAdmin.Should().BeFalse();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.UpdatedAt.Should().BeNull();
        user.Sessions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void User_ShouldAcceptAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "admin@example.com";
        var name = "Admin User";
        var pictureUrl = "https://example.com/picture.jpg";
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var updatedAt = DateTime.UtcNow;

        // Act
        var user = new User
        {
            Id = userId,
            Email = email,
            Name = name,
            PictureUrl = pictureUrl,
            IsAdmin = true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        user.Id.Should().Be(userId);
        user.Email.Should().Be(email);
        user.Name.Should().Be(name);
        user.PictureUrl.Should().Be(pictureUrl);
        user.IsAdmin.Should().BeTrue();
        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void User_SessionsNavigation_ShouldAllowMultipleSessions()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var session1 = new UserSession { Id = Guid.NewGuid(), UserId = user.Id };
        var session2 = new UserSession { Id = Guid.NewGuid(), UserId = user.Id };

        // Act
        user.Sessions.Add(session1);
        user.Sessions.Add(session2);

        // Assert
        user.Sessions.Should().HaveCount(2);
        user.Sessions.Should().Contain(session1);
        user.Sessions.Should().Contain(session2);
    }
}
