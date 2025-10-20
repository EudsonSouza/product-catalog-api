using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Data;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Services;
using ProductCatalog.Services.DTOs;

namespace ProductCatalog.Tests.Unit.Services;

public class ProductServiceCategoriesTests : IDisposable
{
    private readonly ProductCatalogDbContext _context;
    private readonly UnitOfWork _uow;
    private readonly ProductService _service;

    public ProductServiceCategoriesTests()
    {
        var options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ProductCatalogDbContext(options);
        _uow = new UnitOfWork(_context);
        _service = new ProductService(_uow);
    }

    [Fact]
    public async Task Create_Product_With_Multiple_Categories_Should_Persist_Associations()
    {
        var catA = new Category { Id = Guid.NewGuid(), Name = "Footwear", Slug = "footwear", Gender = Gender.Unisex };
        var catB = new Category { Id = Guid.NewGuid(), Name = "Running", Slug = "running", Gender = Gender.Unisex };
        _context.Categories.AddRange(catA, catB);
        await _context.SaveChangesAsync();

        var dto = new CreateProductDto(
            Name: "Runner Pro",
            Description: "Lightweight running shoe",
            CategoryIds: new List<Guid> { catA.Id, catB.Id },
            Gender: Gender.Unisex,
            BasePrice: 129.90m
        );

        var result = await _service.CreateAsync(dto);

        result.CategoryIds.Should().BeEquivalentTo(new[] { catA.Id, catB.Id });

        var persisted = await _context.Products
            .Include(p => p.Categories)
            .FirstAsync(p => p.Id == result.Id);

        persisted.Categories.Select(c => c.Id).Should().BeEquivalentTo(new[] { catA.Id, catB.Id });
    }

    public void Dispose()
    {
        _uow?.Dispose();
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}

