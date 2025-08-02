using FluentAssertions;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Tests.Unit.Domain;

public class ProductTests
{
    [Fact]
    public void Product_Should_Be_Created_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Pijama Feminino Floral";
        var slug = "pijama-feminino-floral";
        var categoryId = Guid.NewGuid();
        var gender = Gender.F;
        var basePrice = 89.90m;

        // Act
        var product = new Product
        {
            Id = id,
            Name = name,
            Slug = slug,
            CategoryId = categoryId,
            Gender = gender,
            BasePrice = basePrice,
            IsActive = true,
            IsFeatured = false
        };

        // Assert
        product.Id.Should().Be(id);
        product.Name.Should().Be(name);
        product.Slug.Should().Be(slug);
        product.CategoryId.Should().Be(categoryId);
        product.Gender.Should().Be(Gender.F);
        product.BasePrice.Should().Be(basePrice);
        product.IsActive.Should().BeTrue();
        product.IsFeatured.Should().BeFalse();
        product.Variants.Should().NotBeNull();
        product.Images.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Product_SetName_Should_Throw_When_Invalid(string invalidName)
    {
        var product = new Product();

        Assert.Throws<ArgumentException>(() => product.SetName(invalidName));
    }

    [Fact]
    public void Product_Should_Generate_Slug_From_Name()
    {
        // Arrange
        var name = "Pijama Feminino Floral";
        var expectedSlug = "pijama-feminino-floral";

        // Act
        var product = new Product();
        product.SetName(name); // Método que vamos criar

        // Assert
        product.Slug.Should().Be(expectedSlug);
    }
}