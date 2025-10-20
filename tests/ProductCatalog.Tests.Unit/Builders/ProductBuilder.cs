using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Tests.Unit.Builders;

/// <summary>
/// Test builder for creating Product instances using the Builder pattern
/// </summary>
public class ProductBuilder
{
    // Static cache to ensure same category instances are reused across products
    private static readonly Dictionary<Guid, Category> _categoryCache = new();

    /// <summary>
    /// Clears the category cache. Call this between tests to avoid entity tracking conflicts.
    /// </summary>
    public static void ClearCache()
    {
        _categoryCache.Clear();
    }

    private Guid _id = Guid.NewGuid();
    private string _name = "Test Product";
    private string? _description = "Test product description";
    private string _slug = "test-product";
    private readonly List<Guid> _categoryIds = new();
    private Gender _gender = Gender.Unisex;
    private decimal? _basePrice = 99.99m;
    private bool _isActive = true;
    private bool _isFeatured;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private readonly List<Category> _categories = new();
    private List<ProductVariant> _variants = new();
    private List<ProductImage> _images = new();

    public ProductBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _name = name;
        _slug = GenerateSlug(name);
        return this;
    }

    public ProductBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ProductBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public ProductBuilder WithCategoryId(Guid categoryId)
    {
        _categoryIds.Add(categoryId);
        return this;
    }

    public ProductBuilder WithGender(Gender gender)
    {
        _gender = gender;
        return this;
    }

    public ProductBuilder WithBasePrice(decimal? basePrice)
    {
        _basePrice = basePrice;
        return this;
    }

    public ProductBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public ProductBuilder WithIsFeatured(bool isFeatured)
    {
        _isFeatured = isFeatured;
        return this;
    }

    public ProductBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ProductBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public ProductBuilder WithCategory(Category category)
    {
        _categories.Add(category);
        return this;
    }

    public ProductBuilder WithVariants(params ProductVariant[] variants)
    {
        _variants = variants.ToList();
        return this;
    }

    public ProductBuilder WithImages(params ProductImage[] images)
    {
        _images = images.ToList();
        return this;
    }

    public ProductBuilder AddVariant(ProductVariant variant)
    {
        _variants.Add(variant);
        return this;
    }

    public ProductBuilder AddImage(ProductImage image)
    {
        _images.Add(image);
        return this;
    }

    public Product Build()
    {
        var product = new Product
        {
            Id = _id,
            Name = _name,
            Description = _description,
            Slug = _slug,
            Gender = _gender,
            BasePrice = _basePrice,
            IsActive = _isActive,
            IsFeatured = _isFeatured,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            Variants = _variants,
            Images = _images
        };

        // Compose categories from provided ids and category objects
        var cats = new List<Category>();
        cats.AddRange(_categories);
        foreach (var id in _categoryIds)
        {
            if (!cats.Any(c => c.Id == id))
            {
                // Use cached category instance to avoid duplicate key errors in EF
                if (!_categoryCache.TryGetValue(id, out Category? value))
                {
                    value = new Category
                    {
                        Id = id,
                        Name = $"Category-{id.ToString()[..8]}",
                        Slug = $"category-{id.ToString()[..8]}",
                        Gender = Gender.Unisex
                    };
                    _categoryCache[id] = value;
                }
                cats.Add(value);
            }
        }
        foreach (var c in cats)
        {
            if (!product.Categories.Contains(c))
            {
                product.Categories.Add(c);
            }
            if (!c.Products.Contains(product))
            {
                c.Products.Add(product);
            }
        }

        // Set ProductId on variants
        foreach (var variant in _variants)
        {
            variant.ProductId = product.Id;
            variant.Product = product;
        }

        // Set ProductId on images
        foreach (var image in _images)
        {
            image.ProductId = product.Id;
            image.Product = product;
        }

        return product;
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
