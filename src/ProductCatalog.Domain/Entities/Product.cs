using System.Globalization;
using System.Text;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public decimal? BasePrice { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    // Business Logic
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty");
        if (name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters");

        Name = name.Trim();
        Slug = GenerateSlug(name);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDescription(string? description)
    {
        if (description?.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters");

        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBasePrice(decimal? price)
    {
        if (price.HasValue && price.Value < 0)
            throw new ArgumentException("Price cannot be negative");

        BasePrice = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (!CanBeActivated())
            throw new InvalidOperationException("Product cannot be activated without available variants in stock");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFeatured(bool featured)
    {
        IsFeatured = featured;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBeActivated()
    {
        return Variants.Any(v => v.IsAvailable && v.StockQuantity > 0);
    }

    public bool HasMainImage()
    {
        return Images.Any(i => i.IsMain);
    }

    public ProductImage? GetMainImage()
    {
        return Images.FirstOrDefault(i => i.IsMain);
    }

    public int GetTotalStock()
    {
        return Variants.Where(v => v.IsAvailable).Sum(v => v.StockQuantity);
    }

    public void AddCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        if (!Categories.Contains(category))
        {
            Categories.Add(category);
        }
        if (!category.Products.Contains(this))
        {
            category.Products.Add(this);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        Categories.Remove(category);
        if (category.Products.Contains(this))
        {
            category.Products.Remove(this);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal? GetMinPrice()
    {
        var availableVariants = Variants.Where(v => v.IsAvailable).ToList();
        if (availableVariants.Count == 0) return BasePrice;

        return availableVariants.Min(v => v.GetEffectivePrice());
    }

    public decimal? GetMaxPrice()
    {
        var availableVariants = Variants.Where(v => v.IsAvailable).ToList();
        if (availableVariants.Count == 0) return BasePrice;

        return availableVariants.Max(v => v.GetEffectivePrice());
    }

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var slug = stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');

        // Remove any remaining non-alphanumeric characters except hyphens
        return string.Concat(slug.Where(c => char.IsLetterOrDigit(c) || c == '-'));
    }
}
