using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Tests.Unit.Builders;

namespace ProductCatalog.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Category entity
/// Tests business logic and validation rules
/// </summary>
public class CategoryTests
{
    [Fact]
    public void Category_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var category = new CategoryBuilder()
            .WithName("T-Shirts")
            .WithGender(Gender.Unisex)
            .Build();

        // Assert
        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal("T-Shirts", category.Name);
        Assert.Equal("t-shirts", category.Slug);
        Assert.Equal(Gender.Unisex, category.Gender);
        Assert.True(category.IsActive);
    }

    [Fact]
    public void Category_GeneratesSlugFromName_Automatically()
    {
        // Arrange & Act
        var category = new CategoryBuilder()
            .WithName("Men's Jackets")
            .Build();

        // Assert
        Assert.Equal("men's-jackets", category.Slug);
    }

    [Fact]
    public void Category_CanHaveProducts_AsNavigationProperty()
    {
        // Arrange
        var category = TestDataFactory.CreateCategory();
        var product1 = new ProductBuilder()
            .WithName("Product 1")
            .WithCategory(category)
            .Build();
        var product2 = new ProductBuilder()
            .WithName("Product 2")
            .WithCategory(category)
            .Build();

        // Act
        category.Products.Add(product1);
        category.Products.Add(product2);

        // Assert
        Assert.Equal(2, category.Products.Count);
        Assert.Contains(product1, category.Products);
        Assert.Contains(product2, category.Products);
    }

    [Fact]
    public void Category_CanBeDeactivated()
    {
        // Arrange
        var category = new CategoryBuilder()
            .WithIsActive(true)
            .Build();

        // Act
        category.IsActive = false;

        // Assert
        Assert.False(category.IsActive);
    }

    [Fact]
    public void Category_CanBeActivated()
    {
        // Arrange
        var category = new CategoryBuilder()
            .WithIsActive(false)
            .Build();

        // Act
        category.IsActive = true;

        // Assert
        Assert.True(category.IsActive);
    }

    [Fact]
    public void Category_HasCorrectGender_WhenSet()
    {
        // Arrange & Act
        var maleCategory = new CategoryBuilder()
            .WithGender(Gender.M)
            .Build();

        var femaleCategory = new CategoryBuilder()
            .WithGender(Gender.F)
            .Build();

        var unisexCategory = new CategoryBuilder()
            .WithGender(Gender.Unisex)
            .Build();

        // Assert
        Assert.Equal(Gender.M, maleCategory.Gender);
        Assert.Equal(Gender.F, femaleCategory.Gender);
        Assert.Equal(Gender.Unisex, unisexCategory.Gender);
    }

    [Fact]
    public void Category_TracksCreatedAt_Timestamp()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;

        // Act
        var category = new CategoryBuilder()
            .WithCreatedAt(DateTime.UtcNow)
            .Build();

        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.InRange(category.CreatedAt, beforeCreate.AddSeconds(-1), afterCreate.AddSeconds(1));
    }

    [Fact]
    public void Category_TracksUpdatedAt_Timestamp()
    {
        // Arrange
        var category = new CategoryBuilder()
            .WithUpdatedAt(DateTime.UtcNow.AddDays(-1))
            .Build();

        var beforeUpdate = DateTime.UtcNow;

        // Act
        category.UpdatedAt = DateTime.UtcNow;

        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.InRange(category.UpdatedAt, beforeUpdate.AddSeconds(-1), afterUpdate.AddSeconds(1));
    }
}
