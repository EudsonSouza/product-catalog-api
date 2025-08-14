using Bogus;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using Npgsql;

namespace ProductCatalog.Data.Seed;

public static class ProductCatalogSeeder
{
    private static volatile bool _isRunning;
    private const string SeedLockKey = "product_catalog.seed.v1";

    public static async Task SeedAsync(ProductCatalogDbContext context)
    {
        if (System.Threading.Interlocked.Exchange(ref _isRunning, true))
        {
            Console.WriteLine("🔒 Seed already running in this process. Skipping.");
            return;
        }

        await using var conn = (NpgsqlConnection)context.Database.GetDbConnection();
        var close = false;
        if (conn.State != ConnectionState.Open) { await conn.OpenAsync(); close = true; }

        await using var lockCmd = new NpgsqlCommand("SELECT pg_try_advisory_lock(hashtext(@k));", conn);
        lockCmd.Parameters.AddWithValue("k", SeedLockKey);
        var gotLock = (bool)(await lockCmd.ExecuteScalarAsync() ?? false);

        if (!gotLock)
        {
            Console.WriteLine("🔒 Another instance is seeding (advisory lock busy). Skipping.");
            if (close) await conn.CloseAsync();
            _isRunning = false;
            return;
        }

        try
        {
            Console.WriteLine("🌱 Starting database seed...");
            await SeedCategoriesAsync(context);
            await SeedColorsAsync(context);
            await SeedSizesAsync(context);
            if (!await context.Products.AnyAsync()) await SeedProductsAsync(context);
            Console.WriteLine("✅ Database seed completed successfully!");
        }
        finally
        {
            await using var unlock = new NpgsqlCommand("SELECT pg_advisory_unlock(hashtext(@k));", conn);
            unlock.Parameters.AddWithValue("k", SeedLockKey);
            await unlock.ExecuteNonQueryAsync();
            if (close) await conn.CloseAsync();
            _isRunning = false;
        }
    }


    private static async Task SeedCategoriesAsync(ProductCatalogDbContext context)
    {
        var now = DateTime.UtcNow;
        await context.Database.ExecuteSqlRawAsync(@"
INSERT INTO public.categories (id, name, slug, gender, is_active, created_at, updated_at)
VALUES
    ({0}, 'T-Shirts', 't-shirts', {1}, TRUE, {2}, {2}),
    ({3}, 'Jeans', 'jeans', {1}, TRUE, {2}, {2}),
    ({4}, 'Sneakers', 'sneakers', {1}, TRUE, {2}, {2}),
    ({5}, 'Hoodies', 'hoodies', {1}, TRUE, {2}, {2}),
    ({6}, 'Dresses', 'dresses', {7}, TRUE, {2}, {2})
ON CONFLICT (slug)
DO UPDATE SET
    name = EXCLUDED.name,
    gender = EXCLUDED.gender,
    is_active = EXCLUDED.is_active,
    updated_at = EXCLUDED.updated_at;
",
            Guid.NewGuid(), (int)Gender.Unisex, now,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(), (int)Gender.F
        );

        var count = await context.Categories.CountAsync();
        Console.WriteLine($"📁 Categories upserted. Total now: {count}");
    }

    private static async Task SeedColorsAsync(ProductCatalogDbContext context)
    {
        var now = DateTime.UtcNow;
        await context.Database.ExecuteSqlRawAsync(@"
INSERT INTO public.colors (id, name, hex_code, is_active, created_at, updated_at)
VALUES
    ({0}, 'Black', '#000000', TRUE, {6}, {6}),
    ({1}, 'White', '#FFFFFF', TRUE, {6}, {6}),
    ({2}, 'Navy Blue', '#000080', TRUE, {6}, {6}),
    ({3}, 'Red', '#FF0000', TRUE, {6}, {6}),
    ({4}, 'Gray', '#808080', TRUE, {6}, {6}),
    ({5}, 'Green', '#008000', TRUE, {6}, {6})
ON CONFLICT (name)
DO UPDATE SET
    name = EXCLUDED.name,
    is_active = EXCLUDED.is_active,
    updated_at = EXCLUDED.updated_at;
",
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), now);

        var count = await context.Colors.CountAsync();
        Console.WriteLine($"🎨 Colors upserted. Total now: {count}");
    }

    private static async Task SeedSizesAsync(ProductCatalogDbContext context)
    {
        var now = DateTime.UtcNow;
        await context.Database.ExecuteSqlRawAsync(@"
INSERT INTO public.sizes (id, name, is_active, created_at, updated_at)
VALUES
    ({0}, 'XS', TRUE, {6}, {6}),
    ({1}, 'S', TRUE, {6}, {6}),
    ({2}, 'M', TRUE, {6}, {6}),
    ({3}, 'L', TRUE, {6}, {6}),
    ({4}, 'XL', TRUE, {6}, {6}),
    ({5}, 'XXL', TRUE, {6}, {6})
ON CONFLICT (name)
DO UPDATE SET
    is_active = EXCLUDED.is_active,
    updated_at = EXCLUDED.updated_at;
",
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), now);

        var count = await context.Sizes.CountAsync();
        Console.WriteLine($"📏 Sizes upserted. Total now: {count}");
    }

    private static async Task SeedProductsAsync(ProductCatalogDbContext context)
    {
        var categories = await context.Categories.AsNoTracking().ToListAsync();
        var colors = await context.Colors.AsNoTracking().ToListAsync();
        var sizes = await context.Sizes.AsNoTracking().ToListAsync();

        var existingSlugs = new HashSet<string>(
            await context.Products.AsNoTracking().Select(p => p.Slug).ToListAsync(),
            StringComparer.Ordinal
        );

        var faker = new Faker<Product>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id)
            .RuleFor(p => p.Gender, f => f.PickRandom<Gender>())
            .RuleFor(p => p.BasePrice, f => Math.Round(f.Random.Decimal(10, 200), 2))
            .RuleFor(p => p.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(p => p.IsFeatured, f => f.Random.Bool(0.3f))
     .RuleFor(p => p.CreatedAt, f =>
    {
        var dt = f.Date.Past(1, DateTime.UtcNow);
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    })
    .RuleFor(p => p.UpdatedAt, (f, p) =>
    {
        var dt = p.CreatedAt.AddDays(f.Random.Int(0, 30));
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    });
        var products = new List<Product>();
        for (int i = 0; i < 50; i++)
        {
            var p = faker.Generate();
            p.Slug = MakeUniqueSlug(ToSlug(p.Name), existingSlugs);
            products.Add(p);
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        await SeedProductVariantsAsync(context, products, colors, sizes);
        Console.WriteLine($"👕 Created {products.Count} products with variants");
    }

    private static async Task SeedProductVariantsAsync(ProductCatalogDbContext context, List<Product> products, List<Color> colors, List<Size> sizes)
    {
        var variants = new List<ProductVariant>();

        foreach (var product in products)
        {
            var productColors = colors.OrderBy(_ => Guid.NewGuid()).Take(Random.Shared.Next(1, 4)).ToList();
            var productSizes = sizes.OrderBy(_ => Guid.NewGuid()).Take(Random.Shared.Next(2, 5)).ToList();

            foreach (var color in productColors)
            foreach (var size in productSizes)
            {
                variants.Add(new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ColorId = color.Id,
                    SizeId = size.Id,
                    SKU = $"{product.Slug}-{ToSlug(color.Name)}-{ToSlug(size.Name)}",
                    Price = Random.Shared.Next(0, 3) == 0 ? product.BasePrice + Random.Shared.Next(-10, 20) : null,
                    StockQuantity = Random.Shared.Next(0, 100),
                    IsAvailable = Random.Shared.Next(0, 10) > 1,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                });
            }
        }

        context.ProductVariants.AddRange(variants);
        await context.SaveChangesAsync();
        Console.WriteLine($"🎯 Created {variants.Count} product variants");
    }


    private static string MakeUniqueSlug(string baseSlug, HashSet<string> used)
    {
        var s = baseSlug;
        var i = 2;
        while (used.Contains(s))
            s = $"{baseSlug}-{i++}";
        used.Add(s);
        return s;
    }

    private static string ToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        var noAccents = sb.ToString().Normalize(NormalizationForm.FormC);

        var slug = noAccents.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"&", " and ");
        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
        slug = Regex.Replace(slug, @"-+", "-").Trim('-');
        return slug;
    }
}
