using ProductCatalog.Domain.Entities;
using ProductCatalog.Tests.Unit.Builders;

namespace ProductCatalog.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Size entity
/// Tests properties and navigation
/// </summary>
public class SizeTests
{
    [Fact]
    public void Size_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var size = new SizeBuilder()
            .WithName("Medium")
            .Build();

        // Assert
        Assert.NotEqual(Guid.Empty, size.Id);
        Assert.Equal("Medium", size.Name);
        Assert.True(size.IsActive);
    }

    [Fact]
    public void Size_IsActive_DefaultsToTrue()
    {
        // Arrange & Act
        var size = new Size
        {
            Id = Guid.NewGuid(),
            Name = "Large"
        };

        // Assert
        Assert.True(size.IsActive);
    }

    [Fact]
    public void Size_CanBeDeactivated()
    {
        // Arrange
        var size = new SizeBuilder()
            .WithIsActive(true)
            .Build();

        // Act
        size.IsActive = false;

        // Assert
        Assert.False(size.IsActive);
    }

    [Fact]
    public void Size_CanHaveVariants_AsNavigationProperty()
    {
        // Arrange
        var size = TestDataFactory.CreateSize();
        var product = TestDataFactory.CreateProduct();
        var variant1 = new ProductVariantBuilder()
            .WithProduct(product)
            .WithSize(size)
            .Build();
        var variant2 = new ProductVariantBuilder()
            .WithProduct(product)
            .WithSize(size)
            .Build();

        // Act
        size.Variants.Add(variant1);
        size.Variants.Add(variant2);

        // Assert
        Assert.Equal(2, size.Variants.Count);
        Assert.Contains(variant1, size.Variants);
        Assert.Contains(variant2, size.Variants);
    }

    [Fact]
    public void Size_TracksCreatedAt_Timestamp()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;

        // Act
        var size = new SizeBuilder()
            .WithCreatedAt(DateTime.UtcNow)
            .Build();

        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.InRange(size.CreatedAt, beforeCreate.AddSeconds(-1), afterCreate.AddSeconds(1));
    }

    [Fact]
    public void Size_TracksUpdatedAt_Timestamp()
    {
        // Arrange
        var size = new SizeBuilder()
            .WithUpdatedAt(DateTime.UtcNow.AddDays(-1))
            .Build();

        var beforeUpdate = DateTime.UtcNow;

        // Act
        size.UpdatedAt = DateTime.UtcNow;

        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.InRange(size.UpdatedAt, beforeUpdate.AddSeconds(-1), afterUpdate.AddSeconds(1));
    }

    [Fact]
    public void Size_Variants_InitializesToEmptyList()
    {
        // Arrange & Act
        var size = new Size
        {
            Id = Guid.NewGuid(),
            Name = "Small"
        };

        // Assert
        Assert.NotNull(size.Variants);
        Assert.Empty(size.Variants);
    }

    [Fact]
    public void Size_NameProperty_CanBeSet()
    {
        // Arrange
        var size = new Size
        {
            Id = Guid.NewGuid(),
            Name = "XL"
        };

        // Act
        size.Name = "Extra Large";

        // Assert
        Assert.Equal("Extra Large", size.Name);
    }
}
