using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(p => p.Gender)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.BasePrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.IsFeatured)
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(p => p.Slug)
            .IsUnique()
            .HasDatabaseName("ix_products_slug");

        builder.HasIndex(p => p.Gender)
            .HasDatabaseName("ix_products_gender");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("ix_products_is_active");

        builder.HasIndex(p => p.IsFeatured)
            .HasDatabaseName("ix_products_is_featured");

        builder.HasIndex(p => new { p.Name, p.IsActive })
            .HasDatabaseName("ix_products_name_active");
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Product> builder)
    {
        builder
            .HasMany(p => p.Categories)
            .WithMany(c => c.Products)
            .UsingEntity<Dictionary<string, object>>(
                "product_categories",
                j => j
                    .HasOne<Category>()
                    .WithMany()
                    .HasForeignKey("category_id")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Product>()
                    .WithMany()
                    .HasForeignKey("product_id")
                    .OnDelete(DeleteBehavior.Cascade))
            .ToTable("product_categories");

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
