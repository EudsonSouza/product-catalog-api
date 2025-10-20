using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(c => c.Gender)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.ParentId)
            .IsRequired(false);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Category> builder)
    {
        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasDatabaseName("ix_categories_slug");

        builder.HasIndex(c => c.Gender)
            .HasDatabaseName("ix_categories_gender");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("ix_categories_is_active");

        builder.HasIndex(c => new { c.Gender, c.IsActive })
            .HasDatabaseName("ix_categories_gender_active");

        builder.HasIndex(c => c.ParentId)
            .HasDatabaseName("ix_categories_parent_id");
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Category> builder)
    {
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
