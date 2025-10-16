# N-Layered Architecture

> _This document explains the N-Layered Architecture implementation in the Product Catalog API._

---
last_updated: 2025-10-16
source: updated
---

## Purpose

This document describes how N-Layered Architecture principles are applied in the Product Catalog API, detailing the responsibilities of each layer and the dependency rules that govern them.

## N-Layered Architecture Principles

The Product Catalog API follows N-Layered Architecture (also known as Traditional Layered Architecture) with these core principles:

1. **Separation of Concerns**: Each layer has a specific responsibility
2. **Dependency Flow**: Upper layers depend on lower layers
3. **Testability**: Business logic can be tested with proper mocking
4. **Maintainability**: Clear structure makes code easy to understand and modify

## Dependency Rule

**Dependencies flow downward**: Upper layers can depend on lower layers.

```
┌─────────────────────────────────────────┐
│         API Layer                        │  ← Presentation
│  (Controllers, DTOs, Startup)           │
└──────────────┬──────────────────────────┘
               │ depends on
┌──────────────▼──────────────────────────┐
│      Services Layer                      │  ← Application Logic
│  (Services, Business Rules)              │
└──────────────┬──────────────────────────┘
               │ depends on
┌──────────────▼──────────────────────────┐
│         Domain Layer                     │  ← Entities
│  (Entities, Value Objects)               │
└──────────────▲──────────────────────────┘
               │ depends on
┌──────────────┴──────────────────────────┐
│         Data Layer                       │  ← Data Access
│  (EF Core, Repositories, DbContext)     │
└─────────────────────────────────────────┘
```

## Layer Descriptions

### 1. Domain Layer (Core)

**Location**: `src/ProductCatalog.Domain/`

**Responsibilities**:
- Define business entities (Product, Category, Variant)
- Encapsulate basic business rules and validation
- Define repository interfaces (no implementation)
- Contain value objects

**Dependencies**: None (pure C# classes)

**Key Components**:
```csharp
// Entities
- Product.cs
- Category.cs
- ProductVariant.cs
- BaseEntity.cs

// Repository Interfaces
- IProductRepository.cs
- ICategoryRepository.cs
- IVariantRepository.cs
- IUnitOfWork.cs

// Business Logic
- Domain validation rules
- Business constraints
```

**Example**:
```csharp
public class Product : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal BasePrice { get; private set; }
    public bool IsActive { get; private set; }

    // Business logic
    public void Activate()
    {
        if (string.IsNullOrEmpty(Name))
            throw new InvalidOperationException("Cannot activate product without name");
        IsActive = true;
    }
}
```

### 2. Services Layer

**Location**: `src/ProductCatalog.Services/`

**Responsibilities**:
- Implement application services and business logic
- Orchestrate domain objects to perform tasks
- Define DTOs for data transfer
- Handle application-level validation
- Map between domain entities and DTOs

**Dependencies**: Domain Layer and Data Layer

**Key Components**:
```csharp
// Services
- ProductService.cs
- CategoryService.cs
- InventoryService.cs

// DTOs
- ProductDto.cs
- CreateProductDto.cs
- UpdateProductDto.cs

// Mapping
- AutoMapper profiles
```

**Example**:
```csharp
public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<ProductDto> GetProductByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException("Product not found");
        return MapToDto(product);
    }
}
```

### 3. Data Layer

**Location**: `src/ProductCatalog.Data/`

**Responsibilities**:
- Implement repository interfaces from Domain
- Configure Entity Framework Core
- Implement database context
- Handle data persistence concerns
- Manage database migrations

**Dependencies**: Domain Layer (implements interfaces)

**Key Components**:
```csharp
// Repositories (Implementations)
- ProductRepository.cs
- CategoryRepository.cs
- VariantRepository.cs
- UnitOfWork.cs

// Database Context
- ApplicationDbContext.cs

// Configurations
- ProductConfiguration.cs
- CategoryConfiguration.cs

// Migrations
- Database migration files
```

**Example**:
```csharp
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<Product> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

### 4. API Layer (Presentation)

**Location**: `src/ProductCatalog.API/`

**Responsibilities**:
- Handle HTTP requests and responses
- Input validation
- Authentication and authorization
- Error handling and logging
- API documentation (Swagger)
- Dependency injection configuration

**Dependencies**: Services Layer and Data Layer

**Key Components**:
```csharp
// Controllers
- ProductsController.cs
- CategoriesController.cs

// Middleware
- ErrorHandlingMiddleware.cs
- LoggingMiddleware.cs

// Configuration
- Program.cs
- appsettings.json

// Extensions
- ServiceCollectionExtensions.cs
```

**Example**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return Ok(product);
    }
}
```

## Project Structure

```
ProductCatalog.sln
│
├── src/
│   ├── ProductCatalog.Domain/           # Core business entities
│   │   ├── Entities/
│   │   │   ├── Product.cs
│   │   │   ├── Category.cs
│   │   │   └── ProductVariant.cs
│   │   ├── Interfaces/
│   │   │   ├── IProductRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   └── Exceptions/
│   │
│   ├── ProductCatalog.Services/        # Application services
│   │   ├── Services/
│   │   │   └── ProductService.cs
│   │   ├── DTOs/
│   │   │   ├── ProductDto.cs
│   │   │   └── CreateProductDto.cs
│   │   └── Mapping/
│   │
│   ├── ProductCatalog.Data/            # Data access
│   │   ├── Context/
│   │   │   └── ApplicationDbContext.cs
│   │   ├── Repositories/
│   │   │   ├── ProductRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── Configurations/
│   │   └── Migrations/
│   │
│   ├── ProductCatalog.API/             # Web API
│   │   ├── Controllers/
│   │   │   └── ProductsController.cs
│   │   ├── Middleware/
│   │   ├── Extensions/
│   │   └── Program.cs
│   │
│   └── ProductCatalog.Migrator/        # Database migration utility
│
└── tests/
    ├── ProductCatalog.Domain.Tests/
    ├── ProductCatalog.Services.Tests/
    └── ProductCatalog.API.Tests/
```

## Benefits of This Architecture

### 1. Simplicity
- Straightforward layer hierarchy
- Easy to understand for developers
- Clear dependency flow

### 2. Testability
- Domain logic can be tested without database
- Services can be tested with mock repositories
- API controllers can be tested with mock services

### 3. Maintainability
- Clear separation of concerns
- Easy to locate and modify code
- Well-organized project structure

### 4. Flexibility
- Can swap database implementations
- Can modify business logic without affecting presentation
- Easy to add new features

## Dependency Injection

The API layer configures dependency injection to wire everything together:

```csharp
// Program.cs
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
```

## Testing Strategy

Each layer has its own test project:

- **Domain.Tests**: Test business logic and domain rules
- **Services.Tests**: Test services with mock repositories
- **API.Tests**: Integration tests for endpoints

## N-Layered vs Clean Architecture

**Why N-Layered instead of Clean Architecture?**

This project uses N-Layered Architecture, which differs from Clean Architecture in these key ways:

| Aspect | N-Layered | Clean Architecture |
|--------|-----------|-------------------|
| **Layers** | API → Services → Domain → Data | API → Application → Domain ← Infrastructure |
| **Dependencies** | Downward flow | Inward flow (Dependency Inversion) |
| **Services Layer** | Can depend on Data Layer | Application depends only on Domain |
| **Infrastructure** | Data layer is at bottom | Infrastructure depends on Domain |
| **Complexity** | Simpler, more direct | More complex, stricter rules |

**When to use N-Layered:**
- Small to medium projects
- Teams familiar with traditional layering
- When simplicity is preferred over strict separation

**When to use Clean Architecture:**
- Large, complex systems
- Need for strict independence from frameworks
- Multiple data sources or external services
- Microservices architecture

## Anti-Patterns to Avoid

1. **Domain depending on Data**: Never reference EF Core in domain entities
2. **Anemic Domain Model**: Domain entities should contain business logic, not just properties
3. **Fat Services**: Don't put all logic in services; use domain entities
4. **Circular Dependencies**: Avoid layers depending on upper layers

## Related Documentation

- [System Diagram](./SystemDiagram.md)
- [Data Model](./DataModel.md)
- [Development Setup](../03-Development/Setup.md)
- [Testing Guide](../03-Development/Tests.md)

## Further Reading

- [N-Layered Architecture Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
