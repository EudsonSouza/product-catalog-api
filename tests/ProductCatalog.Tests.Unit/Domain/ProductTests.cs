using AwesomeAssertions;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests.Unit.Domain;

public class ProductTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetName_Should_Throw_When_NullOrEmpty(string invalidName)
    {
        var product = new Product();
        Assert.Throws<ArgumentException>(() => product.SetName(invalidName));
    }

    [Fact]
    public void SetName_Should_Throw_When_Too_Long()
    {
        var product = new Product();
        var longName = new string('A', 201);

        Assert.Throws<ArgumentException>(() => product.SetName(longName));
    }

    [Theory]
    [InlineData("Product Name", "product-name")]
    [InlineData("Tênis Esportivo", "tenis-esportivo")]
    [InlineData("Product@#$%Name", "productname")]
    [InlineData("Product  With  Multiple  Spaces", "product-with-multiple-spaces")]
    public void SetName_Should_Generate_Correct_Slug(string name, string expectedSlug)
    {
        var product = new Product();

        product.SetName(name);

        product.Slug.Should().Be(expectedSlug);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10.5)]
    public void SetBasePrice_Should_Throw_When_Negative(decimal negativePrice)
    {
        var product = new Product();

        Assert.Throws<ArgumentException>(() => product.SetBasePrice(negativePrice));
    }

    [Fact]
    public void CanBeActivated_Should_Return_True_When_Has_Available_Variants_In_Stock()
    {
        var product = new Product();
        product.Variants.Add(new ProductVariant { IsAvailable = true, StockQuantity = 5 });

        var canBeActivated = product.CanBeActivated();

        canBeActivated.Should().BeTrue();
    }

    [Fact]
    public void CanBeActivated_Should_Return_False_When_No_Variants_Available()
    {
        var product = new Product();
        product.Variants.Add(new ProductVariant { IsAvailable = false, StockQuantity = 5 });
        product.Variants.Add(new ProductVariant { IsAvailable = true, StockQuantity = 0 });

        var canBeActivated = product.CanBeActivated();

        canBeActivated.Should().BeFalse();
    }

    [Fact]
    public void Activate_Should_Throw_When_Cannot_Be_Activated()
    {
        var product = new Product();
        product.Variants.Add(new ProductVariant { IsAvailable = false, StockQuantity = 0 });

        Assert.Throws<InvalidOperationException>(() => product.Activate());
    }

    [Fact]
    public void GetTotalStock_Should_Return_Sum_Of_Available_Variants_Stock()
    {
        var product = new Product();
        product.Variants.Add(new ProductVariant { IsAvailable = true, StockQuantity = 10 });
        product.Variants.Add(new ProductVariant { IsAvailable = true, StockQuantity = 5 });
        product.Variants.Add(new ProductVariant { IsAvailable = false, StockQuantity = 3 }); // Ignored

        var totalStock = product.GetTotalStock();

        totalStock.Should().Be(15);
    }

    [Fact]
    public void GetMinPrice_Should_Return_Lowest_Available_Variant_Price()
    {
        var product = new Product { BasePrice = 100 };
        product.Variants.Add(new ProductVariant { IsAvailable = true, Price = 80, Product = product });
        product.Variants.Add(new ProductVariant { IsAvailable = true, Price = 120, Product = product });
        product.Variants.Add(new ProductVariant { IsAvailable = false, Price = 50, Product = product }); // Ignored

        var minPrice = product.GetMinPrice();

        minPrice.Should().Be(80);
    }

    [Fact]
    public void GetMinPrice_Should_Return_BasePrice_When_No_Available_Variants()
    {
        var product = new Product { BasePrice = 100 };
        product.Variants.Add(new ProductVariant { IsAvailable = false, Price = 80, Product = product });

        var minPrice = product.GetMinPrice();

        minPrice.Should().Be(100);
    }

    [Fact]
    public void GetMainImage_Should_Return_Main_Image_When_Exists()
    {
        var product = new Product();
        var mainImage = new ProductImage { IsMain = true, ImageUrl = "main.jpg" };
        product.Images.Add(new ProductImage { IsMain = false, ImageUrl = "other.jpg" });
        product.Images.Add(mainImage);

        var result = product.GetMainImage();

        result.Should().Be(mainImage);
    }
}
