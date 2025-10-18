using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Tests.Unit.Builders;

/// <summary>
/// Test builder for creating Category instances using the Builder pattern
/// </summary>
public class CategoryBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Category";
    private string _slug = "test-category";
    private Gender _gender = Gender.Unisex;
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private List<Product> _products = new();

    public CategoryBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CategoryBuilder WithName(string name)
    {
        _name = name;
        _slug = GenerateSlug(name);
        return this;
    }

    public CategoryBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public CategoryBuilder WithGender(Gender gender)
    {
        _gender = gender;
        return this;
    }

    public CategoryBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public CategoryBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public CategoryBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public CategoryBuilder WithProducts(params Product[] products)
    {
        _products = products.ToList();
        return this;
    }

    public CategoryBuilder AddProduct(Product product)
    {
        _products.Add(product);
        return this;
    }

    public Category Build()
    {
        return new Category
        {
            Id = _id,
            Name = _name,
            Slug = _slug,
            Gender = _gender,
            IsActive = _isActive,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            Products = _products
        };
    }

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }
}
