using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests.Unit.Builders;

/// <summary>
/// Test builder for creating Size instances using the Builder pattern
/// </summary>
public class SizeBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "M";
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private List<ProductVariant> _variants = new();

    public SizeBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public SizeBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SizeBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public SizeBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public SizeBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public SizeBuilder WithVariants(params ProductVariant[] variants)
    {
        _variants = variants.ToList();
        return this;
    }

    public SizeBuilder AddVariant(ProductVariant variant)
    {
        _variants.Add(variant);
        return this;
    }

    public Size Build()
    {
        return new Size
        {
            Id = _id,
            Name = _name,
            IsActive = _isActive,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            Variants = _variants
        };
    }
}
