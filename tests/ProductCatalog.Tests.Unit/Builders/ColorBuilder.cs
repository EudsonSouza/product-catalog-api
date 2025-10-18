using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests.Unit.Builders;

/// <summary>
/// Test builder for creating Color instances using the Builder pattern
/// </summary>
public class ColorBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Black";
    private string? _hexCode = "#000000";
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private List<ProductVariant> _variants = new();

    public ColorBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ColorBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ColorBuilder WithHexCode(string? hexCode)
    {
        _hexCode = hexCode;
        return this;
    }

    public ColorBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public ColorBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ColorBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public ColorBuilder WithVariants(params ProductVariant[] variants)
    {
        _variants = variants.ToList();
        return this;
    }

    public ColorBuilder AddVariant(ProductVariant variant)
    {
        _variants.Add(variant);
        return this;
    }

    public Color Build()
    {
        return new Color
        {
            Id = _id,
            Name = _name,
            HexCode = _hexCode,
            IsActive = _isActive,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            Variants = _variants
        };
    }
}
