using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests.Unit.Builders;

/// <summary>
/// Test builder for creating ProductVariant instances using the Builder pattern
/// </summary>
public class ProductVariantBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _productId = Guid.NewGuid();
    private Guid _colorId = Guid.NewGuid();
    private Guid _sizeId = Guid.NewGuid();
    private string? _sku = "TEST-SKU-001";
    private decimal? _price = 99.99m;
    private int _stockQuantity = 10;
    private bool _isAvailable = true;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private Product? _product;
    private Color? _color;
    private Size? _size;
    private List<ProductImage> _images = new();

    public ProductVariantBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProductVariantBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public ProductVariantBuilder WithColorId(Guid colorId)
    {
        _colorId = colorId;
        return this;
    }

    public ProductVariantBuilder WithSizeId(Guid sizeId)
    {
        _sizeId = sizeId;
        return this;
    }

    public ProductVariantBuilder WithSKU(string? sku)
    {
        _sku = sku;
        return this;
    }

    public ProductVariantBuilder WithPrice(decimal? price)
    {
        _price = price;
        return this;
    }

    public ProductVariantBuilder WithStockQuantity(int stockQuantity)
    {
        _stockQuantity = stockQuantity;
        return this;
    }

    public ProductVariantBuilder WithIsAvailable(bool isAvailable)
    {
        _isAvailable = isAvailable;
        return this;
    }

    public ProductVariantBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ProductVariantBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public ProductVariantBuilder WithProduct(Product product)
    {
        _product = product;
        _productId = product.Id;
        return this;
    }

    public ProductVariantBuilder WithColor(Color color)
    {
        _color = color;
        _colorId = color.Id;
        return this;
    }

    public ProductVariantBuilder WithSize(Size size)
    {
        _size = size;
        _sizeId = size.Id;
        return this;
    }

    public ProductVariantBuilder WithImages(params ProductImage[] images)
    {
        _images = images.ToList();
        return this;
    }

    public ProductVariantBuilder AddImage(ProductImage image)
    {
        _images.Add(image);
        return this;
    }

    public ProductVariant Build()
    {
        var variant = new ProductVariant
        {
            Id = _id,
            ProductId = _productId,
            ColorId = _colorId,
            SizeId = _sizeId,
            SKU = _sku,
            Price = _price,
            StockQuantity = _stockQuantity,
            IsAvailable = _isAvailable,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            Images = _images
        };

        if (_product != null)
            variant.Product = _product;

        if (_color != null)
            variant.Color = _color;

        if (_size != null)
            variant.Size = _size;

        return variant;
    }
}
