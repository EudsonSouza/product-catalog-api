using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public CategoriesController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Category>), 200)]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        var categories = await _uow.Categories.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<Category>), 200)]
    public async Task<ActionResult<IEnumerable<Category>>> GetActiveCategories()
    {
        var categories = await _uow.Categories.GetActiveAsync();
        return Ok(categories);
    }

    [HttpGet("gender/{gender}")]
    [ProducesResponseType(typeof(IEnumerable<Category>), 200)]
    public async Task<ActionResult<IEnumerable<Category>>> GetByGender(string gender)
    {
        var categories = await _uow.Categories.GetByGenderAsync(gender);
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Category), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Category>> GetCategory(Guid id)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(Category), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Category>> GetBySlug(string slug)
    {
        var category = await _uow.Categories.GetBySlugAsync(slug);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(Category), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Category>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ValidationProblem("Name is required");

        if (string.IsNullOrWhiteSpace(request.Slug))
            return ValidationProblem("Slug is required");

        var now = DateTime.UtcNow;
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim(),
            Gender = request.Gender,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        var existing = await _uow.Categories.GetBySlugAsync(category.Slug);
        if (existing != null)
            return ValidationProblem($"Slug '{category.Slug}' is already in use");

        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(Category), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Category>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Name))
            category.Name = request.Name.Trim();

        if (!string.IsNullOrWhiteSpace(request.Slug) && !string.Equals(request.Slug, category.Slug, StringComparison.Ordinal))
        {
            var existing = await _uow.Categories.GetBySlugAsync(request.Slug.Trim());
            if (existing != null && existing.Id != id)
                return ValidationProblem($"Slug '{request.Slug}' is already in use");
            category.Slug = request.Slug.Trim();
        }

        if (request.Gender.HasValue)
            category.Gender = request.Gender.Value;

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;

        await _uow.Categories.UpdateAsync(category);
        await _uow.SaveChangesAsync();

        return Ok(category);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteCategory(Guid id)
    {
        var exists = await _uow.Categories.GetByIdAsync(id);
        if (exists == null)
            return NotFound();

        await _uow.Categories.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return NoContent();
    }

    public record CreateCategoryRequest(string Name, string Slug, Gender Gender, bool IsActive = true);

    public record UpdateCategoryRequest
    {
        public string? Name { get; init; }
        public string? Slug { get; init; }
        public Gender? Gender { get; init; }
        public bool? IsActive { get; init; }
    }
}

