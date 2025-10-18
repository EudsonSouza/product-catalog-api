using ProductCatalog.Domain.Entities;
using ProductCatalog.Tests.Unit.Builders;

namespace ProductCatalog.Tests.Unit.Domain;

/// <summary>
/// Unit tests for ProductVariant entity
/// Tests business logic, stock management, and pricing calculations
/// </summary>
public class ProductVariantTests
{
    [Fact]
    public void ProductVariant_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var variant = new ProductVariantBuilder()
            .WithSKU("TEST-SKU-001")
            .WithPrice(49.99m)
            .WithStockQuantity(10)
            .Build();

        // Assert
        Assert.NotEqual(Guid.Empty, variant.Id);
        Assert.Equal("TEST-SKU-001", variant.SKU);
        Assert.Equal(49.99m, variant.Price);
        Assert.Equal(10, variant.StockQuantity);
        Assert.True(variant.IsAvailable);
    }

    [Fact]
    public void IsInStock_ReturnsTrue_WhenAvailableAndHasStock()
    {
        // Arrange
        var variant = new ProductVariantBuilder()
            .WithStockQuantity(5)
            .WithIsAvailable(true)
            .Build();

        // Act
        var result = variant.IsInStock();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInStock_ReturnsFalse_WhenNotAvailable()
    {
        // Arrange
        var variant = new ProductVariantBuilder()
            .WithStockQuantity(5)
            .WithIsAvailable(false)
            .Build();

        // Act
        var result = variant.IsInStock();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInStock_ReturnsFalse_WhenStockIsZero()
    {
        // Arrange
        var variant = new ProductVariantBuilder()
            .WithStockQuantity(0)
            .WithIsAvailable(true)
            .Build();

        // Act
        var result = variant.IsInStock();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateStock_UpdatesStockQuantity_WithValidQuantity()
    {
        // Arrange
        var variant = new ProductVariantBuilder()
            .WithStockQuantity(10)
            .Build();

        // Act
        variant.UpdateStock(20);

        // Assert
        Assert.Equal(20, variant.StockQuantity);
        Assert.True(variant.IsAvailable);
    }

    [Fact]
    public void UpdateStock_SetsIsAvailableToTrue_WhenQuantityGreaterThanZero()
    {
        // Arrange
        var variant = new ProductVariantBuilder()
            .WithStockQuantity(0)
            .WithIsAvailable(false)
            .Build();

        // Act
        variant.UpdateStock(5);

        // Assert
        Assert.Equal(5, variant.StockQuantity);
        Assert.True(variant.IsAvailable);
    }

    [Fact]
    public void UpdateStock_SetsIsAvailableToFalse_WhenQuantityIsZero()
    {
        // Arrange
        var variant = new ProductVariantBuilder()
            .WithStockQuantity(10)
            .WithIsAvailable(true)
            .Build();

        // Act
        variant.UpdateStock(0);

        // Assert
        Assert.Equal(0, variant.StockQuantity);
        Assert.False(variant.IsAvailable);
    }

    [Fact]
    public void UpdateStock_ThrowsException_WhenQuantityIsNegative()
    {
        // Arrange
        var variant = new ProductVariantBuilder().Build();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => variant.UpdateStock(-5));
        Assert.Equal("Stock cannot be negative", exception.Message);
    }

    [Fact]
    public void GetEffectivePrice_ReturnsVariantPrice_WhenVariantPriceIsSet()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithBasePrice(29.99m)
            .Build();

        var variant = new ProductVariantBuilder()
            .WithProduct(product)
            .WithPrice(39.99m)
            .Build();

        // Act
        var effectivePrice = variant.GetEffectivePrice();

        // Assert
        Assert.Equal(39.99m, effectivePrice);
    }

    [Fact]
    public void GetEffectivePrice_ReturnsProductBasePrice_WhenVariantPriceIsNull()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithBasePrice(29.99m)
            .Build();

        var variant = new ProductVariantBuilder()
            .WithProduct(product)
            .WithPrice(null)
            .Build();

        // Act
        var effectivePrice = variant.GetEffectivePrice();

        // Assert
        Assert.Equal(29.99m, effectivePrice);
    }

    [Fact]
    public void GetEffectivePrice_ReturnsZero_WhenBothPricesAreNull()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithBasePrice(null)
            .Build();

        var variant = new ProductVariantBuilder()
            .WithProduct(product)
            .WithPrice(null)
            .Build();

        // Act
        var effectivePrice = variant.GetEffectivePrice();

        // Assert
        Assert.Equal(0m, effectivePrice);
    }

    [Fact]
    public void ProductVariant_HasColorAndSize_NavigationProperties()
    {
        // Arrange
        var color = TestDataFactory.CreateColor("Blue", "#0000FF");
        var size = TestDataFactory.CreateSize("L");

        // Act
        var variant = new ProductVariantBuilder()
            .WithColor(color)
            .WithSize(size)
            .Build();

        // Assert
        Assert.NotNull(variant.Color);
        Assert.Equal("Blue", variant.Color.Name);
        Assert.NotNull(variant.Size);
        Assert.Equal("L", variant.Size.Name);
    }

    [Fact]
    public void ProductVariant_CanHaveImages()
    {
        // Arrange
        var variant = new ProductVariantBuilder().Build();
        var image1 = new ProductImageBuilder()
            .WithVariant(variant)
            .WithImageUrl("https://example.com/variant-1.jpg")
            .Build();
        var image2 = new ProductImageBuilder()
            .WithVariant(variant)
            .WithImageUrl("https://example.com/variant-2.jpg")
            .Build();

        // Act
        variant.Images.Add(image1);
        variant.Images.Add(image2);

        // Assert
        Assert.Equal(2, variant.Images.Count);
    }

    [Fact]
    public void ProductVariant_TracksCreatedAndUpdatedTimestamps()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var updatedAt = DateTime.UtcNow;

        // Act
        var variant = new ProductVariantBuilder()
            .WithCreatedAt(createdAt)
            .WithUpdatedAt(updatedAt)
            .Build();

        // Assert
        Assert.Equal(createdAt, variant.CreatedAt);
        Assert.Equal(updatedAt, variant.UpdatedAt);
    }
}
