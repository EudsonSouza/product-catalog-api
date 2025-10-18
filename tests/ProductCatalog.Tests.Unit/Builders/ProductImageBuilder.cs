using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests.Unit.Builders;

/// <summary>
/// Test builder for creating ProductImage instances using the Builder pattern
/// </summary>
public class ProductImageBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _productId = Guid.NewGuid();
    private Guid? _variantId;
    private string _imageUrl = "https://example.com/image.jpg";
    private bool _isMain;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private Product? _product;
    private ProductVariant? _variant;

    public ProductImageBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProductImageBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public ProductImageBuilder WithVariantId(Guid? variantId)
    {
        _variantId = variantId;
        return this;
    }

    public ProductImageBuilder WithImageUrl(string imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public ProductImageBuilder WithIsMain(bool isMain)
    {
        _isMain = isMain;
        return this;
    }

    public ProductImageBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ProductImageBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public ProductImageBuilder WithProduct(Product product)
    {
        _product = product;
        _productId = product.Id;
        return this;
    }

    public ProductImageBuilder WithVariant(ProductVariant variant)
    {
        _variant = variant;
        _variantId = variant.Id;
        return this;
    }

    public ProductImage Build()
    {
        var image = new ProductImage
        {
            Id = _id,
            ProductId = _productId,
            VariantId = _variantId,
            ImageUrl = _imageUrl,
            IsMain = _isMain,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt
        };

        if (_product != null)
            image.Product = _product;

        if (_variant != null)
            image.Variant = _variant;

        return image;
    }
}
