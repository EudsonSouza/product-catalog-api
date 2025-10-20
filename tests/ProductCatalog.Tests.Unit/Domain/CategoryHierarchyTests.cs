using AwesomeAssertions;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Tests.Unit.Domain;

public class CategoryHierarchyTests
{
    [Fact]
    public void Category_Can_Have_Parent_And_Children()
    {
        var parent = new Category { Id = Guid.NewGuid(), Name = "Clothing", Slug = "clothing", Gender = Gender.Unisex };
        var child1 = new Category { Id = Guid.NewGuid(), Name = "T-Shirts", Slug = "t-shirts", Gender = Gender.Unisex, Parent = parent };
        var child2 = new Category { Id = Guid.NewGuid(), Name = "Jeans", Slug = "jeans", Gender = Gender.Unisex };

        parent.Children.Add(child1);
        parent.Children.Add(child2);
        child2.Parent = parent;

        parent.Children.Should().Contain(child1);
        parent.Children.Should().Contain(child2);
        child1.Parent.Should().Be(parent);
        child2.Parent.Should().Be(parent);
    }
}

