using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Tests.Unit.Builders;

/// <summary>
/// Factory class for creating common test data scenarios
/// </summary>
public static class TestDataFactory
{
    // ============ Categories ============

    public static Category CreateCategory(string name = "T-Shirts", Gender gender = Gender.Unisex)
    {
        return new CategoryBuilder()
            .WithName(name)
            .WithGender(gender)
            .Build();
    }

    public static List<Category> CreateCategories()
    {
        return new List<Category>
        {
            CreateCategory("T-Shirts", Gender.Unisex),
            CreateCategory("Pants", Gender.M),
            CreateCategory("Dresses", Gender.F),
            CreateCategory("Jackets", Gender.Unisex)
        };
    }

    // ============ Colors ============

    public static Color CreateColor(string name = "Black", string hexCode = "#000000")
    {
        return new ColorBuilder()
            .WithName(name)
            .WithHexCode(hexCode)
            .Build();
    }

    public static List<Color> CreateColors()
    {
        return new List<Color>
        {
            CreateColor("Black", "#000000"),
            CreateColor("White", "#FFFFFF"),
            CreateColor("Red", "#FF0000"),
            CreateColor("Blue", "#0000FF"),
            CreateColor("Green", "#00FF00")
        };
    }

    // ============ Sizes ============

    public static Size CreateSize(string name = "M")
    {
        return new SizeBuilder()
            .WithName(name)
            .Build();
    }

    public static List<Size> CreateSizes()
    {
        return new List<Size>
        {
            CreateSize("XS"),
            CreateSize("S"),
            CreateSize("M"),
            CreateSize("L"),
            CreateSize("XL")
        };
    }

    // ============ Products ============

    public static Product CreateProduct(
        string name = "Basic T-Shirt",
        decimal basePrice = 29.99m,
        Gender gender = Gender.Unisex)
    {
        var category = CreateCategory();

        return new ProductBuilder()
            .WithName(name)
            .WithBasePrice(basePrice)
            .WithGender(gender)
            .WithCategory(category)
            .Build();
    }

    public static Product CreateProductWithVariants(
        string name = "T-Shirt with Variants",
        int variantCount = 3)
    {
        var product = CreateProduct(name);
        var colors = CreateColors();
        var sizes = CreateSizes();

        for (int i = 0; i < variantCount; i++)
        {
            var variant = new ProductVariantBuilder()
                .WithProduct(product)
                .WithColor(colors[i % colors.Count])
                .WithSize(sizes[i % sizes.Count])
                .WithStockQuantity(10)
                .WithSKU($"SKU-{i + 1:D3}")
                .Build();

            product.Variants.Add(variant);
        }

        return product;
    }

    public static Product CreateProductWithImages(
        string name = "Product with Images",
        int imageCount = 3)
    {
        var product = CreateProduct(name);

        for (int i = 0; i < imageCount; i++)
        {
            var image = new ProductImageBuilder()
                .WithProduct(product)
                .WithImageUrl($"https://example.com/images/product-{i + 1}.jpg")
                .WithIsMain(i == 0) // First image is main
                .Build();

            product.Images.Add(image);
        }

        return product;
    }

    public static Product CreateCompleteProduct(string name = "Complete Product")
    {
        var category = CreateCategory();
        var product = new ProductBuilder()
            .WithName(name)
            .WithBasePrice(49.99m)
            .WithCategory(category)
            .WithGender(Gender.Unisex)
            .WithDescription("A complete test product with variants and images")
            .WithIsFeatured(true)
            .Build();

        // Add variants
        var colors = CreateColors().Take(3).ToList();
        var sizes = CreateSizes().Take(3).ToList();

        foreach (var color in colors)
        {
            foreach (var size in sizes)
            {
                var variant = new ProductVariantBuilder()
                    .WithProduct(product)
                    .WithColor(color)
                    .WithSize(size)
                    .WithStockQuantity(5)
                    .WithPrice(49.99m)
                    .WithSKU($"SKU-{color.Name}-{size.Name}")
                    .Build();

                product.Variants.Add(variant);
            }
        }

        // Add images
        for (int i = 0; i < 4; i++)
        {
            var image = new ProductImageBuilder()
                .WithProduct(product)
                .WithImageUrl($"https://example.com/images/{name.ToLower().Replace(" ", "-")}-{i + 1}.jpg")
                .WithIsMain(i == 0)
                .Build();

            product.Images.Add(image);
        }

        return product;
    }

    // ============ Product Variants ============

    public static ProductVariant CreateVariant(
        Product? product = null,
        Color? color = null,
        Size? size = null,
        int stockQuantity = 10)
    {
        var builder = new ProductVariantBuilder()
            .WithStockQuantity(stockQuantity)
            .WithPrice(29.99m)
            .WithSKU("TEST-SKU");

        if (product != null)
            builder.WithProduct(product);

        if (color != null)
            builder.WithColor(color);

        if (size != null)
            builder.WithSize(size);

        return builder.Build();
    }

    public static List<ProductVariant> CreateVariantsForProduct(Product product)
    {
        var colors = CreateColors().Take(2).ToList();
        var sizes = CreateSizes().Take(2).ToList();
        var variants = new List<ProductVariant>();

        foreach (var color in colors)
        {
            foreach (var size in sizes)
            {
                var variant = new ProductVariantBuilder()
                    .WithProduct(product)
                    .WithColor(color)
                    .WithSize(size)
                    .WithStockQuantity(5)
                    .WithSKU($"{product.Name}-{color.Name}-{size.Name}")
                    .Build();

                variants.Add(variant);
            }
        }

        return variants;
    }

    // ============ Product Images ============

    public static ProductImage CreateImage(
        Product? product = null,
        bool isMain = false)
    {
        var builder = new ProductImageBuilder()
            .WithImageUrl("https://example.com/test-image.jpg")
            .WithIsMain(isMain);

        if (product != null)
            builder.WithProduct(product);

        return builder.Build();
    }

    // ============ Lists for Testing ============

    public static List<Product> CreateProductList(int count = 10)
    {
        var products = new List<Product>();
        var category = CreateCategory();

        for (int i = 1; i <= count; i++)
        {
            var product = new ProductBuilder()
                .WithName($"Product {i}")
                .WithBasePrice(10m * i)
                .WithCategory(category)
                .WithGender(i % 2 == 0 ? Gender.M : Gender.F)
                .WithIsFeatured(i % 3 == 0)
                .Build();

            products.Add(product);
        }

        return products;
    }

    public static List<Product> CreateProductsWithVariants(int productCount = 5)
    {
        var products = new List<Product>();

        for (int i = 1; i <= productCount; i++)
        {
            var product = CreateProductWithVariants($"Product {i}", variantCount: 3);
            products.Add(product);
        }

        return products;
    }
}
