using ProductCatalog.Domain.Entities;
using ProductCatalog.Tests.Unit.Builders;

namespace ProductCatalog.Tests.Unit.Domain;

/// <summary>
/// Unit tests for ProductImage entity
/// Tests image URL validation, main image logic, and relationships
/// </summary>
public class ProductImageTests
{
    [Fact]
    public void ProductImage_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var image = new ProductImageBuilder()
            .WithImageUrl("https://example.com/product-image.jpg")
            .WithIsMain(true)
            .Build();

        // Assert
        Assert.NotEqual(Guid.Empty, image.Id);
        Assert.Equal("https://example.com/product-image.jpg", image.ImageUrl);
        Assert.True(image.IsMain);
    }

    [Fact]
    public void ProductImage_CanBeSetAsMainImage()
    {
        // Arrange
        var image = new ProductImageBuilder()
            .WithIsMain(false)
            .Build();

        // Act
        image.IsMain = true;

        // Assert
        Assert.True(image.IsMain);
    }

    [Fact]
    public void ProductImage_CanBeSetAsNonMainImage()
    {
        // Arrange
        var image = new ProductImageBuilder()
            .WithIsMain(true)
            .Build();

        // Act
        image.IsMain = false;

        // Assert
        Assert.False(image.IsMain);
    }

    [Fact]
    public void ProductImage_BelongsToProduct()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct();

        // Act
        var image = new ProductImageBuilder()
            .WithProduct(product)
            .Build();

        // Assert
        Assert.Equal(product.Id, image.ProductId);
        Assert.NotNull(image.Product);
        Assert.Equal(product.Name, image.Product.Name);
    }

    [Fact]
    public void ProductImage_CanBelongToVariant()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct();
        var variant = new ProductVariantBuilder()
            .WithProduct(product)
            .Build();

        // Act
        var image = new ProductImageBuilder()
            .WithProduct(product)
            .WithVariant(variant)
            .Build();

        // Assert
        Assert.Equal(variant.Id, image.VariantId);
        Assert.NotNull(image.Variant);
    }

    [Fact]
    public void ProductImage_VariantIdCanBeNull()
    {
        // Arrange & Act
        var image = new ProductImageBuilder()
            .WithVariantId(null)
            .Build();

        // Assert
        Assert.Null(image.VariantId);
        Assert.Null(image.Variant);
    }

    [Fact]
    public void ProductImage_SupportsVariousImageFormats()
    {
        // Arrange
        var jpgImage = new ProductImageBuilder()
            .WithImageUrl("https://example.com/image.jpg")
            .Build();

        var pngImage = new ProductImageBuilder()
            .WithImageUrl("https://example.com/image.png")
            .Build();

        var webpImage = new ProductImageBuilder()
            .WithImageUrl("https://example.com/image.webp")
            .Build();

        // Assert
        Assert.Contains(".jpg", jpgImage.ImageUrl);
        Assert.Contains(".png", pngImage.ImageUrl);
        Assert.Contains(".webp", webpImage.ImageUrl);
    }

    [Fact]
    public void ProductImage_CanHaveHttpsUrl()
    {
        // Arrange & Act
        var image = new ProductImageBuilder()
            .WithImageUrl("https://cdn.example.com/secure/product-123.jpg")
            .Build();

        // Assert
        Assert.StartsWith("https://", image.ImageUrl);
    }

    [Fact]
    public void ProductImage_TracksCreatedAndUpdatedTimestamps()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var updatedAt = DateTime.UtcNow;

        // Act
        var image = new ProductImageBuilder()
            .WithCreatedAt(createdAt)
            .WithUpdatedAt(updatedAt)
            .Build();

        // Assert
        Assert.Equal(createdAt, image.CreatedAt);
        Assert.Equal(updatedAt, image.UpdatedAt);
    }

    [Fact]
    public void ProductImage_CanBeAssociatedWithProductOnly()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct();

        // Act
        var image = new ProductImageBuilder()
            .WithProduct(product)
            .WithVariantId(null)
            .Build();

        // Assert
        Assert.Equal(product.Id, image.ProductId);
        Assert.Null(image.VariantId);
    }

    [Fact]
    public void ProductImage_DefaultsToNotMain()
    {
        // Arrange & Act
        var image = new ProductImageBuilder().Build();

        // Assert
        Assert.False(image.IsMain);
    }

    [Fact]
    public void ProductImage_AllowsEmptyImageUrl()
    {
        // Arrange & Act
        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ImageUrl = string.Empty,
            IsMain = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(string.Empty, image.ImageUrl);
    }
}
