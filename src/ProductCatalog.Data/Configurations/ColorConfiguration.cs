using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Data.Configurations;

public class ColorConfiguration : IEntityTypeConfiguration<Color>
{
    public void Configure(EntityTypeBuilder<Color> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<Color> builder)
    {
        builder.ToTable("colors");
        builder.HasKey(c => c.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Color> builder)
    {
        builder.Property(c => c.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.HexCode)
            .HasMaxLength(7);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Color> builder)
    {
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("ix_colors_name");

        builder.HasIndex(c => c.HexCode)
            .HasDatabaseName("ix_colors_hex_code")
            .HasFilter("hex_code IS NOT NULL");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("ix_colors_is_active");
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Color> builder)
    {
        builder.HasMany(c => c.Variants)
            .WithOne(v => v.Color)
            .HasForeignKey(v => v.ColorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}