using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Pijama Feminino Floral",
                Description = "Pijama confortável com estampa floral",
                Slug = "pijama-feminino-floral",
                CategoryId = Guid.NewGuid(),
                Gender = Gender.F,
                BasePrice = 89.90m,
                IsActive = true,
                IsFeatured = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Camiseta Masculina Básica",
                Description = "Camiseta básica 100% algodão",
                Slug = "camiseta-masculina-basica",
                CategoryId = Guid.NewGuid(),
                Gender = Gender.M,
                BasePrice = 49.90m,
                IsActive = true,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<Product> GetProduct(Guid id)
    {
        var product = new Product
        {
            Id = id,
            Name = "Produto Exemplo",
            Description = "Descrição do produto exemplo",
            Slug = "produto-exemplo",
            CategoryId = Guid.NewGuid(),
            Gender = Gender.Unisex,
            BasePrice = 99.99m,
            IsActive = true,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(product);
    }

    [HttpPost]
    public ActionResult<Product> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Gender = request.Gender,
            BasePrice = request.BasePrice,
            IsActive = true,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        product.SetName(request.Name);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
}

public record CreateProductRequest(
    string Name,
    string? Description,
    Guid CategoryId,
    Gender Gender,
    decimal? BasePrice
);