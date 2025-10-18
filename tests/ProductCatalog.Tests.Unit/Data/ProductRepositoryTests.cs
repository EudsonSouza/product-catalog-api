using ProductCatalog.Data.Repositories;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Tests.Unit.Builders;
using ProductCatalog.Tests.Unit.Fixtures;

namespace ProductCatalog.Tests.Unit.Data;

/// <summary>
/// Comprehensive tests for ProductRepository
/// Tests all CRUD operations and custom query methods
/// </summary>
public class ProductRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new ProductRepository(_fixture.Context);
    }

    #region Base Repository Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenProductExists()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct();
        await _repository.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList(5);
        foreach (var product in products)
        {
            await _repository.AddAsync(product);
        }
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.True(result.Count() >= 5);
    }

    [Fact]
    public async Task AddAsync_AddsProductToDatabase()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct("New Product");

        // Act
        var addedProduct = await _repository.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        Assert.NotNull(addedProduct);
        var retrieved = await _repository.GetByIdAsync(product.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(product.Name, retrieved.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProduct()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct("Original Name");
        await _repository.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        product.Name = "Updated Name";
        await _repository.UpdateAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var updated = await _repository.GetByIdAsync(product.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct();
        await _repository.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(product.Id);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var deleted = await _repository.GetByIdAsync(product.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenProductExists()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct();
        await _repository.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(product.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenProductDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var exists = await _repository.ExistsAsync(nonExistentId);

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region Custom Query Tests

    [Fact]
    public async Task GetBySlugAsync_ReturnsProduct_WhenSlugExists()
    {
        // Arrange
        var uniqueSlug = $"unique-test-product-{Guid.NewGuid()}";
        var product = new ProductBuilder()
            .WithName("Test Product")
            .WithSlug(uniqueSlug)
            .Build();
        await _repository.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySlugAsync(uniqueSlug);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(uniqueSlug, result.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsNull_WhenSlugDoesNotExist()
    {
        // Act
        var result = await _repository.GetBySlugAsync("non-existent-slug");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsActiveProductsInCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var activeProduct = new ProductBuilder()
            .WithCategoryId(categoryId)
            .WithIsActive(true)
            .WithName("Active Product")
            .Build();
        var inactiveProduct = new ProductBuilder()
            .WithCategoryId(categoryId)
            .WithIsActive(false)
            .WithName("Inactive Product")
            .Build();

        await _repository.AddAsync(activeProduct);
        await _repository.AddAsync(inactiveProduct);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByCategoryAsync(categoryId);

        // Assert
        Assert.Single(results);
        Assert.Equal(activeProduct.Id, results.First().Id);
    }

    [Fact]
    public async Task GetByGenderAsync_ReturnsActiveProductsByGender()
    {
        // Arrange
        var uniqueName = $"MaleProduct-{Guid.NewGuid()}";
        var maleProduct = new ProductBuilder()
            .WithName(uniqueName)
            .WithGender(Gender.M)
            .WithIsActive(true)
            .Build();
        var femaleProduct = new ProductBuilder()
            .WithGender(Gender.F)
            .WithIsActive(true)
            .Build();

        await _repository.AddAsync(maleProduct);
        await _repository.AddAsync(femaleProduct);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByGenderAsync("M");

        // Assert
        Assert.Contains(results, p => p.Id == maleProduct.Id);
        Assert.DoesNotContain(results, p => p.Id == femaleProduct.Id);
    }

    [Fact]
    public async Task GetFeaturedAsync_ReturnsFeaturedActiveProducts()
    {
        // Arrange
        var uniqueName = $"FeaturedProduct-{Guid.NewGuid()}";
        var featured = new ProductBuilder()
            .WithIsFeatured(true)
            .WithIsActive(true)
            .WithName(uniqueName)
            .Build();
        var notFeatured = new ProductBuilder()
            .WithIsFeatured(false)
            .WithIsActive(true)
            .WithName("Regular Product")
            .Build();

        await _repository.AddAsync(featured);
        await _repository.AddAsync(notFeatured);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetFeaturedAsync();

        // Assert
        Assert.Contains(results, p => p.Id == featured.Id);
        Assert.DoesNotContain(results, p => p.Id == notFeatured.Id);
    }

    [Fact]
    public async Task SearchByNameAsync_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = new ProductBuilder()
            .WithName("Blue T-Shirt")
            .WithIsActive(true)
            .Build();
        var product2 = new ProductBuilder()
            .WithName("Red Shirt")
            .WithIsActive(true)
            .Build();

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var results = await _repository.SearchByNameAsync("Blue");

        // Assert
        Assert.Single(results);
        Assert.Equal(product1.Id, results.First().Id);
    }

    [Fact]
    public async Task ExistsBySlugAsync_ReturnsTrue_WhenSlugExists()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithSlug("unique-slug")
            .Build();
        await _repository.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsBySlugAsync("unique-slug");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task GetWithVariantsAsync_LoadsVariantsWithColorAndSize()
    {
        // Arrange
        var color = TestDataFactory.CreateColor();
        var size = TestDataFactory.CreateSize();
        _fixture.Context.Colors.Add(color);
        _fixture.Context.Sizes.Add(size);

        var product = TestDataFactory.CreateProduct();
        var variant = new ProductVariantBuilder()
            .WithProduct(product)
            .WithColor(color)
            .WithSize(size)
            .Build();
        product.Variants.Add(variant);

        await _repository.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithVariantsAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Variants);
        Assert.NotNull(result.Variants.First().Color);
        Assert.NotNull(result.Variants.First().Size);
    }

    [Fact]
    public async Task GetWithImagesAsync_LoadsProductImages()
    {
        // Arrange
        var product = TestDataFactory.CreateProductWithImages(imageCount: 3);
        await _repository.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithImagesAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Images.Count);
    }

    [Fact]
    public async Task GetCompleteAsync_LoadsAllRelatedEntities()
    {
        // Arrange
        var category = TestDataFactory.CreateCategory();
        var color = TestDataFactory.CreateColor();
        var size = TestDataFactory.CreateSize();

        _fixture.Context.Categories.Add(category);
        _fixture.Context.Colors.Add(color);
        _fixture.Context.Sizes.Add(size);
        await _fixture.Context.SaveChangesAsync();

        var product = new ProductBuilder()
            .WithCategory(category)
            .Build();

        var variant = new ProductVariantBuilder()
            .WithProduct(product)
            .WithColor(color)
            .WithSize(size)
            .Build();
        product.Variants.Add(variant);

        var image = new ProductImageBuilder()
            .WithProduct(product)
            .Build();
        product.Images.Add(image);

        await _repository.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCompleteAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Category);
        Assert.NotEmpty(result.Variants);
        Assert.NotEmpty(result.Images);
        Assert.NotNull(result.Variants.First().Color);
        Assert.NotNull(result.Variants.First().Size);
    }

    #endregion
}
