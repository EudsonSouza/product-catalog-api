using ProductCatalog.Data.Repositories;
using ProductCatalog.Tests.Unit.Builders;
using ProductCatalog.Tests.Unit.Fixtures;

namespace ProductCatalog.Tests.Unit.Data;

/// <summary>
/// Tests for ColorRepository
/// </summary>
public class ColorRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ColorRepository _repository;

    public ColorRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new ColorRepository(_fixture.Context);

        // Clear caches and change tracker to avoid conflicts between tests
        ProductBuilder.ClearCache();
        _fixture.ClearChangeTracker();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsColor_WhenExists()
    {
        // Arrange
        var color = TestDataFactory.CreateColor("Red", "#FF0000");
        await _repository.AddAsync(color);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(color.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(color.Id, result.Id);
        Assert.Equal("Red", result.Name);
        Assert.Equal("#FF0000", result.HexCode);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllColors()
    {
        // Arrange
        var colors = TestDataFactory.CreateColors();
        foreach (var color in colors)
        {
            await _repository.AddAsync(color);
        }
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.True(result.Count() >= colors.Count);
    }

    [Fact]
    public async Task AddAsync_AddsColor()
    {
        // Arrange
        var color = new ColorBuilder()
            .WithName("Purple")
            .WithHexCode("#800080")
            .Build();

        // Act
        await _repository.AddAsync(color);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(color.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Purple", retrieved.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesColor()
    {
        // Arrange
        var color = TestDataFactory.CreateColor("Blue", "#0000FF");
        await _repository.AddAsync(color);
        await _fixture.Context.SaveChangesAsync();

        // Act
        color.Name = "Navy Blue";
        color.HexCode = "#000080";
        await _repository.UpdateAsync(color);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var updated = await _repository.GetByIdAsync(color.Id);
        Assert.Equal("Navy Blue", updated!.Name);
        Assert.Equal("#000080", updated.HexCode);
    }

    [Fact]
    public async Task DeleteAsync_RemovesColor()
    {
        // Arrange
        var color = TestDataFactory.CreateColor();
        await _repository.AddAsync(color);
        await _fixture.Context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(color.Id);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var deleted = await _repository.GetByIdAsync(color.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenColorExists()
    {
        // Arrange
        var color = TestDataFactory.CreateColor();
        await _repository.AddAsync(color);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(color.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenColorDoesNotExist()
    {
        // Act
        var exists = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveColors()
    {
        // Arrange
        var active1 = new ColorBuilder()
            .WithName("Active Red")
            .WithIsActive(true)
            .Build();
        var active2 = new ColorBuilder()
            .WithName("Active Blue")
            .WithIsActive(true)
            .Build();
        var inactive = new ColorBuilder()
            .WithName("Inactive Green")
            .WithIsActive(false)
            .Build();

        await _repository.AddAsync(active1);
        await _repository.AddAsync(active2);
        await _repository.AddAsync(inactive);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetActiveAsync();

        // Assert
        Assert.Contains(results, c => c.Id == active1.Id);
        Assert.Contains(results, c => c.Id == active2.Id);
        Assert.DoesNotContain(results, c => c.Id == inactive.Id);
        Assert.All(results, c => Assert.True(c.IsActive));
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsColor_WhenNameExists()
    {
        // Arrange
        var color = new ColorBuilder()
            .WithName("Crimson")
            .WithHexCode("#DC143C")
            .Build();
        await _repository.AddAsync(color);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Crimson");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(color.Id, result.Id);
        Assert.Equal("Crimson", result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_IsCaseInsensitive()
    {
        // Arrange
        var color = new ColorBuilder()
            .WithName("Turquoise")
            .Build();
        await _repository.AddAsync(color);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("TURQUOISE");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(color.Id, result.Id);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsNull_WhenNameDoesNotExist()
    {
        // Act
        var result = await _repository.GetByNameAsync("NonExistentColor");

        // Assert
        Assert.Null(result);
    }
}
