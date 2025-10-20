namespace ProductCatalog.Domain.Entities;

public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ColorId { get; set; }
    public Guid SizeId { get; set; }
    public string? SKU { get; set; }
    public decimal? Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public Product Product { get; set; } = null!;
    public Color Color { get; set; } = null!;
    public Size Size { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    // Business Logic
    public bool IsInStock() => IsAvailable && StockQuantity > 0;

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock cannot be negative");

        StockQuantity = quantity;
        IsAvailable = quantity > 0;
    }


    public decimal GetEffectivePrice()
    {
        return Price ?? Product.BasePrice ?? 0;
    }
}
