
namespace ProductCatalog.Domain.Entities;

public class Size
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
