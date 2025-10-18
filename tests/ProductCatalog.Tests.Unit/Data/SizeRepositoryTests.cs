using ProductCatalog.Data.Repositories;
using ProductCatalog.Tests.Unit.Builders;
using ProductCatalog.Tests.Unit.Fixtures;

namespace ProductCatalog.Tests.Unit.Data;

/// <summary>
/// Tests for SizeRepository
/// </summary>
public class SizeRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly SizeRepository _repository;

    public SizeRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new SizeRepository(_fixture.Context);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsSize_WhenExists()
    {
        // Arrange
        var size = TestDataFactory.CreateSize("XL");
        await _repository.AddAsync(size);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(size.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(size.Id, result.Id);
        Assert.Equal("XL", result.Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllSizes()
    {
        // Arrange
        var sizes = TestDataFactory.CreateSizes();
        foreach (var size in sizes)
        {
            await _repository.AddAsync(size);
        }
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.True(result.Count() >= sizes.Count);
    }

    [Fact]
    public async Task AddAsync_AddsSize()
    {
        // Arrange
        var size = new SizeBuilder()
            .WithName("XXL")
            .Build();

        // Act
        await _repository.AddAsync(size);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(size.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("XXL", retrieved.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesSize()
    {
        // Arrange
        var size = TestDataFactory.CreateSize("M");
        await _repository.AddAsync(size);
        await _fixture.Context.SaveChangesAsync();

        // Act
        size.Name = "Medium";
        await _repository.UpdateAsync(size);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var updated = await _repository.GetByIdAsync(size.Id);
        Assert.Equal("Medium", updated!.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesSize()
    {
        // Arrange
        var size = TestDataFactory.CreateSize();
        await _repository.AddAsync(size);
        await _fixture.Context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(size.Id);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var deleted = await _repository.GetByIdAsync(size.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenSizeExists()
    {
        // Arrange
        var size = TestDataFactory.CreateSize();
        await _repository.AddAsync(size);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(size.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenSizeDoesNotExist()
    {
        // Act
        var exists = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveSizes()
    {
        // Arrange
        var active1 = new SizeBuilder()
            .WithName("Small")
            .WithIsActive(true)
            .Build();
        var active2 = new SizeBuilder()
            .WithName("Medium")
            .WithIsActive(true)
            .Build();
        var inactive = new SizeBuilder()
            .WithName("Discontinued")
            .WithIsActive(false)
            .Build();

        await _repository.AddAsync(active1);
        await _repository.AddAsync(active2);
        await _repository.AddAsync(inactive);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetActiveAsync();

        // Assert
        Assert.Contains(results, s => s.Id == active1.Id);
        Assert.Contains(results, s => s.Id == active2.Id);
        Assert.DoesNotContain(results, s => s.Id == inactive.Id);
        Assert.All(results, s => Assert.True(s.IsActive));
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsSize_WhenNameExists()
    {
        // Arrange
        var size = new SizeBuilder()
            .WithName("Large")
            .Build();
        await _repository.AddAsync(size);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Large");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(size.Id, result.Id);
        Assert.Equal("Large", result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_IsCaseInsensitive()
    {
        // Arrange
        var size = new SizeBuilder()
            .WithName("ExtraLarge")
            .Build();
        await _repository.AddAsync(size);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("EXTRALARGE");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(size.Id, result.Id);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsNull_WhenNameDoesNotExist()
    {
        // Act
        var result = await _repository.GetByNameAsync("NonExistentSize");

        // Assert
        Assert.Null(result);
    }
}
