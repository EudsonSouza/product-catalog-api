using ProductCatalog.Data.Repositories;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Tests.Unit.Builders;
using ProductCatalog.Tests.Unit.Fixtures;

namespace ProductCatalog.Tests.Unit.Data;

/// <summary>
/// Comprehensive tests for CategoryRepository
/// Tests all CRUD operations and custom query methods
/// </summary>
public class CategoryRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new CategoryRepository(_fixture.Context);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategory_WhenExists()
    {
        // Arrange
        var category = TestDataFactory.CreateCategory();
        await _repository.AddAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        // Arrange
        var categories = TestDataFactory.CreateCategories();
        foreach (var category in categories)
        {
            await _repository.AddAsync(category);
        }
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.True(result.Count() >= categories.Count);
    }

    [Fact]
    public async Task AddAsync_AddsCategory()
    {
        // Arrange
        var category = TestDataFactory.CreateCategory("New Category");

        // Act
        await _repository.AddAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(category.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("New Category", retrieved.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCategory()
    {
        // Arrange
        var category = TestDataFactory.CreateCategory("Original");
        await _repository.AddAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        category.Name = "Updated";
        await _repository.UpdateAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var updated = await _repository.GetByIdAsync(category.Id);
        Assert.Equal("Updated", updated!.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        // Arrange
        var category = TestDataFactory.CreateCategory();
        await _repository.AddAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(category.Id);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var deleted = await _repository.GetByIdAsync(category.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsCategory_WhenSlugExists()
    {
        // Arrange
        var category = new CategoryBuilder()
            .WithName("Test Category")
            .WithSlug("test-category")
            .Build();
        await _repository.AddAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySlugAsync("test-category");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsNull_WhenSlugNotExists()
    {
        // Act
        var result = await _repository.GetBySlugAsync("non-existent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByGenderAsync_ReturnsActiveCategories_ForGender()
    {
        // Arrange
        var maleCategory = new CategoryBuilder()
            .WithGender(Gender.M)
            .WithIsActive(true)
            .WithName("Men's")
            .Build();
        var femaleCategory = new CategoryBuilder()
            .WithGender(Gender.F)
            .WithIsActive(true)
            .WithName("Women's")
            .Build();
        var inactiveMale = new CategoryBuilder()
            .WithGender(Gender.M)
            .WithIsActive(false)
            .WithName("Inactive")
            .Build();

        await _repository.AddAsync(maleCategory);
        await _repository.AddAsync(femaleCategory);
        await _repository.AddAsync(inactiveMale);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByGenderAsync("M");

        // Assert
        Assert.Single(results);
        Assert.Equal(maleCategory.Id, results.First().Id);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveCategories()
    {
        // Arrange
        var active1 = new CategoryBuilder()
            .WithIsActive(true)
            .WithName("Active 1")
            .Build();
        var active2 = new CategoryBuilder()
            .WithIsActive(true)
            .WithName("Active 2")
            .Build();
        var inactive = new CategoryBuilder()
            .WithIsActive(false)
            .WithName("Inactive")
            .Build();

        await _repository.AddAsync(active1);
        await _repository.AddAsync(active2);
        await _repository.AddAsync(inactive);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetActiveAsync();

        // Assert
        Assert.True(results.Count() >= 2);
        Assert.All(results, c => Assert.True(c.IsActive));
    }

    [Fact]
    public async Task GetWithProductsAsync_LoadsActiveProducts()
    {
        // Arrange
        var uniqueCategoryId = Guid.NewGuid();
        var category = new CategoryBuilder()
            .WithId(uniqueCategoryId)
            .WithName($"UniqueCategory-{Guid.NewGuid()}")
            .Build();
        await _repository.AddAsync(category);
        await _fixture.Context.SaveChangesAsync();

        var activeProduct = new ProductBuilder()
            .WithCategoryId(uniqueCategoryId)
            .WithIsActive(true)
            .WithName($"Active-{Guid.NewGuid()}")
            .Build();
        var inactiveProduct = new ProductBuilder()
            .WithCategoryId(uniqueCategoryId)
            .WithIsActive(false)
            .WithName($"Inactive-{Guid.NewGuid()}")
            .Build();

        _fixture.Context.Products.Add(activeProduct);
        _fixture.Context.Products.Add(inactiveProduct);
        await _fixture.Context.SaveChangesAsync();

        // Clear change tracker to force fresh query and allow filtered Include to work properly
        _fixture.Context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetWithProductsAsync(uniqueCategoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.Products, p => p.Id == activeProduct.Id);
        Assert.DoesNotContain(result.Products, p => p.Id == inactiveProduct.Id);
        Assert.All(result.Products, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenCategoryExists()
    {
        // Arrange
        var category = TestDataFactory.CreateCategory();
        await _repository.AddAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(category.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenCategoryDoesNotExist()
    {
        // Act
        var exists = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(exists);
    }
}
