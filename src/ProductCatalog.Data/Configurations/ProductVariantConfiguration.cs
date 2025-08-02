using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");
        builder.HasKey(v => v.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.Property(v => v.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(v => v.ProductId)
            .IsRequired();

        builder.Property(v => v.ColorId)
            .IsRequired();

        builder.Property(v => v.SizeId)
            .IsRequired();

        builder.Property(v => v.SKU)
            .HasMaxLength(50);

        builder.Property(v => v.Price)
            .HasColumnType("decimal(10,2)");

        builder.Property(v => v.StockQuantity)
            .HasDefaultValue(0);

        builder.Property(v => v.IsAvailable)
            .HasDefaultValue(true);

        builder.Property(v => v.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(v => v.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    private static void ConfigureIndexes(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasIndex(v => v.ProductId)
            .HasDatabaseName("ix_product_variants_product_id");

        builder.HasIndex(v => v.ColorId)
            .HasDatabaseName("ix_product_variants_color_id");

        builder.HasIndex(v => v.SizeId)
            .HasDatabaseName("ix_product_variants_size_id");

        builder.HasIndex(v => v.SKU)
            .IsUnique()
            .HasDatabaseName("ix_product_variants_sku")
            .HasFilter("sku IS NOT NULL");

        builder.HasIndex(v => v.IsAvailable)
            .HasDatabaseName("ix_product_variants_is_available");

        builder.HasIndex(v => new { v.ProductId, v.ColorId, v.SizeId })
            .IsUnique()
            .HasDatabaseName("ix_product_variants_unique_combination");

        builder.HasIndex(v => new { v.ProductId, v.IsAvailable, v.StockQuantity })
            .HasDatabaseName("ix_product_variants_product_availability_stock");
    }

    private static void ConfigureRelationships(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Color)
            .WithMany(c => c.Variants)
            .HasForeignKey(v => v.ColorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Size)
            .WithMany(s => s.Variants)
            .HasForeignKey(v => v.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.Images)
            .WithOne(i => i.Variant)
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}