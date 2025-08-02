namespace ProductCatalog.Domain.Entities;

public class ProductImage
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }  // Opcional - imagem específica da variante
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsMain { get; set; }      // Imagem principal
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation Properties
    public Product Product { get; set; } = null!;
    public ProductVariant? Variant { get; set; }
}