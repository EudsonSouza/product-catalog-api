using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetCompleteAsync(id);
        return product?.ToDto();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(ProductQueryDto query)
    {
        var products = await _unitOfWork.Products.GetAllAsync();

        var filteredProducts = products.AsQueryable();

        if (!string.IsNullOrEmpty(query.Search))
        {
            filteredProducts = filteredProducts.Where(p =>
                p.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                (p.Description != null && p.Description.Contains(query.Search, StringComparison.OrdinalIgnoreCase)));
        }

        if (query.CategoryId.HasValue)
        {
            var catId = query.CategoryId.Value;
            filteredProducts = filteredProducts.Where(p => p.Categories.Any(c => c.Id == catId));
        }

        if (query.Gender.HasValue)
        {
            filteredProducts = filteredProducts.Where(p => p.Gender == query.Gender);
        }

        if (query.IsActive.HasValue)
        {
            filteredProducts = filteredProducts.Where(p => p.IsActive == query.IsActive);
        }

        if (query.IsFeatured.HasValue)
        {
            filteredProducts = filteredProducts.Where(p => p.IsFeatured == query.IsFeatured);
        }

        return filteredProducts
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => p.ToDto())
            .ToList();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Gender = dto.Gender,
            BasePrice = dto.BasePrice,
            IsActive = true,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        product.SetName(dto.Name);

        // Attach categories
        if (dto.CategoryIds?.Count > 0)
        {
            foreach (var cid in dto.CategoryIds.Distinct())
            {
                var cat = await _unitOfWork.Categories.GetByIdAsync(cid);
                if (cat != null)
                {
                    product.Categories.Add(cat);
                }
            }
        }

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return product.ToDto();
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
        {
            product.SetName(dto.Name);
        }

        if (dto.Description != null)
        {
            product.Description = dto.Description;
        }

        if (dto.CategoryIds != null)
        {
            product.Categories.Clear();
            foreach (var cid in dto.CategoryIds.Distinct())
            {
                var cat = await _unitOfWork.Categories.GetByIdAsync(cid);
                if (cat != null)
                {
                    product.Categories.Add(cat);
                }
            }
        }

        if (dto.Gender.HasValue)
        {
            product.Gender = dto.Gender.Value;
        }

        if (dto.BasePrice.HasValue)
        {
            product.SetBasePrice(dto.BasePrice.Value);
        }

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value)
                product.Activate();
            else
                product.Deactivate();
        }

        if (dto.IsFeatured.HasValue)
        {
            product.IsFeatured = dto.IsFeatured.Value;
        }

        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return product.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var exists = await _unitOfWork.Products.ExistsAsync(id);
        if (!exists)
            return false;

        await _unitOfWork.Products.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _unitOfWork.Products.ExistsAsync(id);
    }
}

public static class ProductExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Slug,
            product.Categories.Select(c => c.Id).ToList(),
            product.Gender,
            product.BasePrice,
            product.IsActive,
            product.IsFeatured,
            product.CreatedAt,
            product.UpdatedAt
        );
    }
}
