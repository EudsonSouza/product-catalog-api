using ProductCatalog.Domain.Entities;
using ProductCatalog.Tests.Unit.Builders;

namespace ProductCatalog.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Color entity
/// Tests properties and navigation
/// </summary>
public class ColorTests
{
    [Fact]
    public void Color_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var color = new ColorBuilder()
            .WithName("Red")
            .WithHexCode("#FF0000")
            .Build();

        // Assert
        Assert.NotEqual(Guid.Empty, color.Id);
        Assert.Equal("Red", color.Name);
        Assert.Equal("#FF0000", color.HexCode);
        Assert.True(color.IsActive);
    }

    [Fact]
    public void Color_IsActive_DefaultsToTrue()
    {
        // Arrange & Act
        var color = new Color
        {
            Id = Guid.NewGuid(),
            Name = "Blue",
            HexCode = "#0000FF"
        };

        // Assert
        Assert.True(color.IsActive);
    }

    [Fact]
    public void Color_CanBeDeactivated()
    {
        // Arrange
        var color = new ColorBuilder()
            .WithIsActive(true)
            .Build();

        // Act
        color.IsActive = false;

        // Assert
        Assert.False(color.IsActive);
    }

    [Fact]
    public void Color_CanHaveVariants_AsNavigationProperty()
    {
        // Arrange
        var color = TestDataFactory.CreateColor();
        var product = TestDataFactory.CreateProduct();
        var variant1 = new ProductVariantBuilder()
            .WithProduct(product)
            .WithColor(color)
            .Build();
        var variant2 = new ProductVariantBuilder()
            .WithProduct(product)
            .WithColor(color)
            .Build();

        // Act
        color.Variants.Add(variant1);
        color.Variants.Add(variant2);

        // Assert
        Assert.Equal(2, color.Variants.Count);
        Assert.Contains(variant1, color.Variants);
        Assert.Contains(variant2, color.Variants);
    }

    [Fact]
    public void Color_TracksCreatedAt_Timestamp()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;

        // Act
        var color = new ColorBuilder()
            .WithCreatedAt(DateTime.UtcNow)
            .Build();

        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.InRange(color.CreatedAt, beforeCreate.AddSeconds(-1), afterCreate.AddSeconds(1));
    }

    [Fact]
    public void Color_TracksUpdatedAt_Timestamp()
    {
        // Arrange
        var color = new ColorBuilder()
            .WithUpdatedAt(DateTime.UtcNow.AddDays(-1))
            .Build();

        var beforeUpdate = DateTime.UtcNow;

        // Act
        color.UpdatedAt = DateTime.UtcNow;

        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.InRange(color.UpdatedAt, beforeUpdate.AddSeconds(-1), afterUpdate.AddSeconds(1));
    }

    [Fact]
    public void Color_HexCode_CanBeNull()
    {
        // Arrange & Act
        var color = new ColorBuilder()
            .WithName("NoHex")
            .WithHexCode(null)
            .Build();

        // Assert
        Assert.Null(color.HexCode);
    }

    [Fact]
    public void Color_Variants_InitializesToEmptyList()
    {
        // Arrange & Act
        var color = new Color
        {
            Id = Guid.NewGuid(),
            Name = "Green"
        };

        // Assert
        Assert.NotNull(color.Variants);
        Assert.Empty(color.Variants);
    }
}
