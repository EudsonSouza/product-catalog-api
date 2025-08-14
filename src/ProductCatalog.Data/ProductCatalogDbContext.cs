using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Data.Configurations;
using ProductCatalog.Data.Extensions;

namespace ProductCatalog.Data;

public class ProductCatalogDbContext : DbContext
{
    public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Size> Sizes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ApplyEntityConfigurations(modelBuilder);
        ConfigureNamingConventions(modelBuilder);
    }

public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    NormalizeDateTimesToUtc();
    return await base.SaveChangesAsync(cancellationToken);
}

public override int SaveChanges()
{
    NormalizeDateTimesToUtc();
    return base.SaveChanges();
}

private void NormalizeDateTimesToUtc()
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.State is EntityState.Added or EntityState.Modified);

    foreach (var e in entries)
    {
        var props = e.Properties
            .Where(p => p.Metadata.ClrType == typeof(DateTime) && p.CurrentValue is DateTime);

        foreach (var p in props)
        {
            var dt = (DateTime)p.CurrentValue!;
            if (dt.Kind == DateTimeKind.Unspecified)
                p.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            else if (dt.Kind == DateTimeKind.Local)
                p.CurrentValue = dt.ToUniversalTime();
        }
    }
}


    private static void ApplyEntityConfigurations(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductVariantConfiguration());
        modelBuilder.ApplyConfiguration(new ProductImageConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ColorConfiguration());
        modelBuilder.ApplyConfiguration(new SizeConfiguration());
    }

    private static void ConfigureNamingConventions(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName().ToSnakeCase());

            foreach (var key in entity.GetKeys())
                key.SetName(key.GetName()?.ToSnakeCase());

            foreach (var foreignKey in entity.GetForeignKeys())
                foreignKey.SetConstraintName(foreignKey.GetConstraintName()?.ToSnakeCase());

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
        }
    }

   }
