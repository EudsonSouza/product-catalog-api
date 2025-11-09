using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProductCatalog.Data;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Services;
using ProductCatalog.Services.DTOs;
using AwesomeAssertions;

namespace ProductCatalog.Tests.Unit.Services;

public class UserServiceTests : IDisposable
{
    private readonly ProductCatalogDbContext _context;
    private readonly UserService _service;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductCatalogDbContext(options);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AdminEmails:0", "admin@example.com" },
                { "AdminEmails:1", "eudsonmateus@gmail.com" }
            })
            .Build();

        _service = new UserService(_context, configuration);
    }

    [Fact]
    public async Task FindOrCreateUserAsync_WithNewUser_ShouldCreateUser()
    {
        // Arrange
        var googleUserInfo = new GoogleUserInfo
        {
            Email = "newuser@example.com",
            Name = "New User",
            PictureUrl = "https://example.com/picture.jpg",
            GoogleId = "google123",
            EmailVerified = true
        };

        // Act
        var result = await _service.FindOrCreateUserAsync(googleUserInfo);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(googleUserInfo.Email);
        result.Name.Should().Be(googleUserInfo.Name);
        result.PictureUrl.Should().Be(googleUserInfo.PictureUrl);
        result.IsAdmin.Should().BeFalse(); // Not in admin whitelist
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify persisted to database
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == googleUserInfo.Email);
        savedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task FindOrCreateUserAsync_WithAdminEmail_ShouldSetIsAdminTrue()
    {
        // Arrange
        var googleUserInfo = new GoogleUserInfo
        {
            Email = "eudsonmateus@gmail.com", // In admin whitelist
            Name = "Admin User",
            PictureUrl = "https://example.com/admin.jpg",
            GoogleId = "google456",
            EmailVerified = true
        };

        // Act
        var result = await _service.FindOrCreateUserAsync(googleUserInfo);

        // Assert
        result.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task FindOrCreateUserAsync_WithExistingUser_ShouldUpdateUser()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            Name = "Old Name",
            PictureUrl = "https://example.com/old.jpg",
            IsAdmin = false,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var googleUserInfo = new GoogleUserInfo
        {
            Email = "existing@example.com",
            Name = "Updated Name",
            PictureUrl = "https://example.com/new.jpg",
            GoogleId = "google789",
            EmailVerified = true
        };

        // Act
        var result = await _service.FindOrCreateUserAsync(googleUserInfo);

        // Assert
        result.Id.Should().Be(existingUser.Id); // Same user
        result.Name.Should().Be("Updated Name");
        result.PictureUrl.Should().Be("https://example.com/new.jpg");
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            IsAdmin = false
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUserByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _service.GetUserByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("admin@example.com", true)]
    [InlineData("eudsonmateus@gmail.com", true)]
    [InlineData("ADMIN@EXAMPLE.COM", true)] // Case insensitive
    [InlineData("user@example.com", false)]
    [InlineData("notadmin@gmail.com", false)]
    public void IsAdminEmail_ShouldReturnCorrectValue(string email, bool expectedIsAdmin)
    {
        // Act
        var result = _service.IsAdminEmail(email);

        // Assert
        result.Should().Be(expectedIsAdmin);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
