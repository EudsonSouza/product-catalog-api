using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");
        builder.HasKey(i => i.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<ProductImage> builder)
    {
        builder.Property(i => i.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.VariantId);

        builder.Property(i => i.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.IsMain)
            .HasDefaultValue(false);

        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(i => i.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    private static void ConfigureIndexes(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasIndex(i => i.ProductId)
            .HasDatabaseName("ix_product_images_product_id");

        builder.HasIndex(i => i.VariantId)
            .HasDatabaseName("ix_product_images_variant_id");

        builder.HasIndex(i => i.IsMain)
            .HasDatabaseName("ix_product_images_is_main");

        builder.HasIndex(i => new { i.ProductId, i.IsMain })
            .HasDatabaseName("ix_product_images_product_main");
    }

    private static void ConfigureRelationships(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Variant)
            .WithMany(v => v.Images)
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}