using Moq;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Services;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Tests.Unit.Builders;

namespace ProductCatalog.Tests.Unit.Services;

/// <summary>
/// Comprehensive unit tests for ProductService
/// Target: 95%+ coverage of ProductService business logic
/// </summary>
public class ProductServiceTests : IDisposable
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly ProductService _sut; // System Under Test

    public ProductServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProductRepo = new Mock<IProductRepository>();

        _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepo.Object);

        _sut = new ProductService(_mockUnitOfWork.Object);
    }

    public void Dispose()
    {
        // Cleanup if needed
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsProductDto_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = TestDataFactory.CreateProduct();
        product.Id = productId;

        _mockProductRepo
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(product.Slug, result.Slug);
        _mockProductRepo.Verify(r => r.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockProductRepo
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.GetByIdAsync(productId);

        // Assert
        Assert.Null(result);
        _mockProductRepo.Verify(r => r.GetByIdAsync(productId), Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts_WhenNoFilterApplied()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList(5);
        var query = new ProductQueryDto(null, null, null, null, null, 1, 10);

        _mockProductRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
        _mockProductRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_FiltersProductsByName_WhenSearchQueryProvided()
    {
        // Arrange
        var products = new List<Product>
        {
            TestDataFactory.CreateProduct("Blue T-Shirt"),
            TestDataFactory.CreateProduct("Red Shirt"),
            TestDataFactory.CreateProduct("Blue Pants")
        };
        var query = new ProductQueryDto("Blue", null, null, null, null, 1, 10);

        _mockProductRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.Contains("Blue", p.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAllAsync_FiltersProductsByDescription_WhenSearchQueryProvided()
    {
        // Arrange
        var product1 = new ProductBuilder()
            .WithName("Product 1")
            .WithDescription("This is a cotton product")
            .Build();

        var product2 = new ProductBuilder()
            .WithName("Product 2")
            .WithDescription("This is a polyester product")
            .Build();

        var products = new List<Product> { product1, product2 };
        var query = new ProductQueryDto("cotton", null, null, null, null, 1, 10);

        _mockProductRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("cotton", result.First().Description!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAllAsync_FiltersProductsByCategory_WhenCategoryIdProvided()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product1 = new ProductBuilder()
            .WithCategoryId(categoryId)
            .WithName("Product 1")
            .Build();

        var product2 = new ProductBuilder()
            .WithCategoryId(Guid.NewGuid())
            .WithName("Product 2")
            .Build();

        var products = new List<Product> { product1, product2 };
        var query = new ProductQueryDto(null, categoryId, null, null, null, 1, 10);

        _mockProductRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(categoryId, result.First().CategoryId);
    }

    [Fact]
    public async Task GetAllAsync_FiltersProductsByGender_WhenGenderProvided()
    {
        // Arrange
        var product1 = new ProductBuilder()
            .WithGender(Gender.M)
            .WithName("Men's Product")
            .Build();

        var product2 = new ProductBuilder()
            .WithGender(Gender.F)
            .WithName("Women's Product")
            .Build();

        var products = new List<Product> { product1, product2 };
        var query = new ProductQueryDto(null, null, Gender.M, null, null, 1, 10);

        _mockProductRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(Gender.M, result.First().Gender);
    }

    [Fact]
    public async Task GetAllAsync_FiltersProductsByIsActive_WhenIsActiveProvided()
    {
        // Arrange
        var product1 = new ProductBuilder()
            .WithIsActive(true)
            .WithName("Active Product")
            .Build();

        var product2 = new ProductBuilder()
            .WithIsActive(false)
            .WithName("Inactive Product")
            .Build();

        var products = new List<Product> { product1, product2 };
        var query = new ProductQueryDto(null, null, null, true, null, 1, 10);

        _mockProductRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.First().IsActive);
    }

    [Fact]
    public async Task GetAllAsync_FiltersProductsByIsFeatured_WhenIsFeaturedProvided()
    {
        // Arrange
        var product1 = new ProductBuilder()
            .WithIsFeatured(true)
            .WithName("Featured Product")
            .Build();

        var product2 = new ProductBuilder()
            .WithIsFeatured(false)
            .WithName("Regular Product")
            .Build();

        var products = new List<Product> { product1, product2 };
        var query = new ProductQueryDto(null, null, null, null, true, 1, 10);

        _mockProductRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.First().IsFeatured);
    }

    [Fact]
    public async Task GetAllAsync_AppliesPaginationCorrectly_WhenPageAndPageSizeProvided()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList(25);
        var query = new ProductQueryDto(null, null, null, null, null, 2, 10); // Page 2, 10 items per page

        _mockProductRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Count()); // Should return 10 items
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoProductsMatchFilter()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList(5);
        var query = new ProductQueryDto("NonExistentProduct", null, null, null, null, 1, 10);

        _mockProductRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_CombinesMultipleFilters_WhenAllFiltersProvided()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product1 = new ProductBuilder()
            .WithName("Blue T-Shirt")
            .WithCategoryId(categoryId)
            .WithGender(Gender.M)
            .WithIsActive(true)
            .WithIsFeatured(true)
            .Build();

        var product2 = new ProductBuilder()
            .WithName("Blue Pants")
            .WithCategoryId(categoryId)
            .WithGender(Gender.F)
            .WithIsActive(true)
            .WithIsFeatured(false)
            .Build();

        var products = new List<Product> { product1, product2 };
        var query = new ProductQueryDto("Blue", categoryId, Gender.M, true, true, 1, 10);

        _mockProductRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(product1.Id, result.First().Id);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_CreatesProduct_WithValidData()
    {
        // Arrange
        var dto = new CreateProductDto(
            "New Product",
            "Product Description",
            Guid.NewGuid(),
            Gender.Unisex,
            49.99m
        );

        _mockProductRepo
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.CategoryId, result.CategoryId);
        Assert.Equal(dto.Gender, result.Gender);
        Assert.Equal(dto.BasePrice, result.BasePrice);
        Assert.True(result.IsActive);
        Assert.False(result.IsFeatured);
        Assert.NotEqual(Guid.Empty, result.Id);

        _mockProductRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_GeneratesSlugFromName_WhenCreatingProduct()
    {
        // Arrange
        var dto = new CreateProductDto(
            "Test Product Name",
            null,
            Guid.NewGuid(),
            Gender.Unisex,
            29.99m
        );

        _mockProductRepo
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Slug);
        Assert.Contains("test", result.Slug);
        Assert.Contains("product", result.Slug);
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedDateToCurrentDateTime()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        var dto = new CreateProductDto(
            "New Product",
            null,
            Guid.NewGuid(),
            Gender.Unisex,
            29.99m
        );

        _mockProductRepo
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(dto);
        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.InRange(result.CreatedAt, beforeCreate, afterCreate);
        Assert.InRange(result.UpdatedAt, beforeCreate, afterCreate);
    }

    [Fact]
    public async Task CreateAsync_SetsIsActiveToTrue_ByDefault()
    {
        // Arrange
        var dto = new CreateProductDto(
            "New Product",
            null,
            Guid.NewGuid(),
            Gender.Unisex,
            29.99m
        );

        _mockProductRepo
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.True(result.IsActive);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UpdatesExistingProduct_WithValidData()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = TestDataFactory.CreateProduct("Old Name");
        existingProduct.Id = productId;

        // Add a variant with stock so the product can be activated
        var variant = new ProductVariantBuilder()
            .WithProduct(existingProduct)
            .WithStockQuantity(10)
            .WithIsAvailable(true)
            .Build();
        existingProduct.Variants.Add(variant);

        var dto = new UpdateProductDto(
            "New Name",
            "New Description",
            Guid.NewGuid(),
            Gender.M,
            99.99m,
            true,
            true
        );

        _mockProductRepo
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _mockProductRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(productId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.CategoryId, result.CategoryId);
        Assert.Equal(dto.Gender, result.Gender);
        Assert.Equal(dto.BasePrice, result.BasePrice);
        Assert.Equal(dto.IsActive, result.IsActive);
        Assert.Equal(dto.IsFeatured, result.IsFeatured);

        _mockProductRepo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesName_WhenNewNameProvided()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = TestDataFactory.CreateProduct("Old Name");
        existingProduct.Id = productId;

        var dto = new UpdateProductDto("New Name", null, null, null, null, null, null);

        _mockProductRepo
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _mockProductRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(productId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesSlug_WhenNameChanges()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = TestDataFactory.CreateProduct("Old Product Name");
        existingProduct.Id = productId;
        var oldSlug = existingProduct.Slug;

        var dto = new UpdateProductDto("Completely New Product Name", null, null, null, null, null, null);

        _mockProductRepo
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _mockProductRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(productId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(oldSlug, result.Slug);
        Assert.Contains("completely", result.Slug);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var dto = new UpdateProductDto("New Name", null, null, null, null, null, null);

        _mockProductRepo
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.UpdateAsync(productId, dto);

        // Assert
        Assert.Null(result);
        _mockProductRepo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_RemovesProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockProductRepo
            .Setup(r => r.ExistsAsync(productId))
            .ReturnsAsync(true);

        _mockProductRepo
            .Setup(r => r.DeleteAsync(productId))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteAsync(productId);

        // Assert
        Assert.True(result);
        _mockProductRepo.Verify(r => r.ExistsAsync(productId), Times.Once);
        _mockProductRepo.Verify(r => r.DeleteAsync(productId), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockProductRepo
            .Setup(r => r.ExistsAsync(productId))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteAsync(productId);

        // Assert
        Assert.False(result);
        _mockProductRepo.Verify(r => r.ExistsAsync(productId), Times.Once);
        _mockProductRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockProductRepo
            .Setup(r => r.ExistsAsync(productId))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ExistsAsync(productId);

        // Assert
        Assert.True(result);
        _mockProductRepo.Verify(r => r.ExistsAsync(productId), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockProductRepo
            .Setup(r => r.ExistsAsync(productId))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ExistsAsync(productId);

        // Assert
        Assert.False(result);
        _mockProductRepo.Verify(r => r.ExistsAsync(productId), Times.Once);
    }

    #endregion
}
