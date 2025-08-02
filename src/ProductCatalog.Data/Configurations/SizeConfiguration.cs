using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Data.Configurations;

public class SizeConfiguration : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<Size> builder)
    {
        builder.ToTable("sizes");
        builder.HasKey(s => s.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Size> builder)
    {
        builder.Property(s => s.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Size> builder)
    {
        builder.HasIndex(s => s.Name)
            .IsUnique()
            .HasDatabaseName("ix_sizes_name");

        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("ix_sizes_is_active");
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Size> builder)
    {
        builder.HasMany(s => s.Variants)
            .WithOne(v => v.Size)
            .HasForeignKey(v => v.SizeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}