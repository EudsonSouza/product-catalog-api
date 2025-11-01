using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.Services
{
    public class ProductService : IProductService
    {
        private const bool DefaultIsActive = true;
        private const bool DefaultIsFeatured = false;

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
            var filteredProducts = ApplyFilters(products, query);
            var paginatedProducts = ApplyPagination(filteredProducts, query);

            return paginatedProducts.Select(p => p.ToDto()).ToList();
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var product = BuildProductFromDto(dto);
            await AttachCategoriesToProduct(product, dto.CategoryIds);

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return product.ToDto();
        }

        public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            await ApplyProductUpdates(product, dto);
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

        private static IQueryable<Product> ApplyFilters(IEnumerable<Product> products, ProductQueryDto query)
        {
            var filteredProducts = products.AsQueryable();

            filteredProducts = ApplySearchFilter(filteredProducts, query.Search);
            filteredProducts = ApplyCategoryFilter(filteredProducts, query.CategoryId);
            filteredProducts = ApplyGenderFilter(filteredProducts, query.Gender);
            filteredProducts = ApplyActiveFilter(filteredProducts, query.IsActive);
            filteredProducts = ApplyFeaturedFilter(filteredProducts, query.IsFeatured);

            return filteredProducts;
        }

        private static IQueryable<Product> ApplySearchFilter(IQueryable<Product> products, string? searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return products;
            }

            return products.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }

        private static IQueryable<Product> ApplyCategoryFilter(IQueryable<Product> products, Guid? categoryId)
        {
            if (!categoryId.HasValue)
            {
                return products;
            }

            return products.Where(p => p.Categories.Any(c => c.Id == categoryId.Value));
        }

        private static IQueryable<Product> ApplyGenderFilter(IQueryable<Product> products, Domain.Enums.Gender? gender)
        {
            return gender.HasValue
                ? products.Where(p => p.Gender == gender.Value)
                : products;
        }

        private static IQueryable<Product> ApplyActiveFilter(IQueryable<Product> products, bool? isActive)
        {
            return isActive.HasValue
                ? products.Where(p => p.IsActive == isActive.Value)
                : products;
        }

        private static IQueryable<Product> ApplyFeaturedFilter(IQueryable<Product> products, bool? isFeatured)
        {
            return isFeatured.HasValue
                ? products.Where(p => p.IsFeatured == isFeatured.Value)
                : products;
        }

        private static IQueryable<Product> ApplyPagination(IQueryable<Product> products, ProductQueryDto query)
        {
            return products
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);
        }

        private static Product BuildProductFromDto(CreateProductDto dto)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Description = dto.Description,
                Gender = dto.Gender,
                BasePrice = dto.BasePrice,
                IsActive = DefaultIsActive,
                IsFeatured = DefaultIsFeatured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            product.SetName(dto.Name);
            return product;
        }

        private async Task AttachCategoriesToProduct(Product product, IEnumerable<Guid>? categoryIds)
        {
            if (categoryIds == null || !categoryIds.Any())
            {
                return;
            }

            foreach (var categoryId in categoryIds.Distinct())
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
                if (category != null)
                {
                    product.Categories.Add(category);
                }
            }
        }

        private async Task ApplyProductUpdates(Product product, UpdateProductDto dto)
        {
            UpdateProductName(product, dto.Name);
            UpdateProductDescription(product, dto.Description);
            await UpdateProductCategories(product, dto.CategoryIds);
            UpdateProductGender(product, dto.Gender);
            UpdateProductBasePrice(product, dto.BasePrice);
            UpdateProductActiveStatus(product, dto.IsActive);
            UpdateProductFeaturedStatus(product, dto.IsFeatured);
        }

        private static void UpdateProductName(Product product, string? name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                product.SetName(name);
            }
        }

        private static void UpdateProductDescription(Product product, string? description)
        {
            if (description != null)
            {
                product.Description = description;
            }
        }

        private async Task UpdateProductCategories(Product product, IEnumerable<Guid>? categoryIds)
        {
            if (categoryIds == null)
            {
                return;
            }

            product.Categories.Clear();
            await AttachCategoriesToProduct(product, categoryIds);
        }

        private static void UpdateProductGender(Product product, Domain.Enums.Gender? gender)
        {
            if (gender.HasValue)
            {
                product.Gender = gender.Value;
            }
        }

        private static void UpdateProductBasePrice(Product product, decimal? basePrice)
        {
            if (basePrice.HasValue)
            {
                product.SetBasePrice(basePrice.Value);
            }
        }

        private static void UpdateProductActiveStatus(Product product, bool? isActive)
        {
            if (!isActive.HasValue)
            {
                return;
            }

            if (isActive.Value)
            {
                product.Activate();
            }
            else
            {
                product.Deactivate();
            }
        }

        private static void UpdateProductFeaturedStatus(Product product, bool? isFeatured)
        {
            if (isFeatured.HasValue)
            {
                product.IsFeatured = isFeatured.Value;
            }
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
}
