using AwesomeAssertions;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Tests.Unit.Domain;

public class ProductCategoriesTests
{
    [Fact]
    public void Product_Can_Have_Multiple_Categories()
    {
        var product = new Product { Id = Guid.NewGuid(), Gender = Gender.Unisex };
        var c1 = new Category { Id = Guid.NewGuid(), Name = "Sneakers", Slug = "sneakers", Gender = Gender.Unisex };
        var c2 = new Category { Id = Guid.NewGuid(), Name = "Sports", Slug = "sports", Gender = Gender.Unisex };

        product.AddCategory(c1);
        product.AddCategory(c2);

        product.Categories.Should().HaveCount(2);
        product.Categories.Should().Contain(c1);
        product.Categories.Should().Contain(c2);
        c1.Products.Should().Contain(product);
        c2.Products.Should().Contain(product);
    }
}
