# Clean Architecture

> _This document explains the Clean Architecture implementation in the Product Catalog API._

---
last_updated: 2025-10-15
source: created
---

## Purpose

This document describes how Clean Architecture principles are applied in the Product Catalog API, detailing the responsibilities of each layer and the dependency rules that govern them.

## Clean Architecture Principles

The Product Catalog API follows Clean Architecture (also known as Onion Architecture or Hexagonal Architecture) with these core principles:

1. **Independence of Frameworks**: Business logic doesn't depend on external frameworks
2. **Testability**: Business logic can be tested without UI, database, or external services
3. **Independence of UI**: The UI can change without affecting business logic
4. **Independence of Database**: Business logic is not bound to a specific database
5. **Independence of External Agencies**: Business logic doesn't know about external services

## Dependency Rule

**Dependencies point inward**: Outer layers can depend on inner layers, but inner layers cannot depend on outer layers.

```
┌─────────────────────────────────────────┐
│         API Layer                        │  ← Presentation
│  (Controllers, DTOs, Startup)           │
└──────────────┬──────────────────────────┘
               │ depends on
┌──────────────▼──────────────────────────┐
│      Application Layer                   │  ← Use Cases
│  (Services, Interfaces)                  │
└──────────────┬──────────────────────────┘
               │ depends on
┌──────────────▼──────────────────────────┐
│         Domain Layer                     │  ← Entities
│  (Entities, Value Objects, Rules)       │  ← (No dependencies)
└──────────────▲──────────────────────────┘
               │ depends on
┌──────────────┴──────────────────────────┐
│      Infrastructure Layer                │  ← External
│  (EF Core, Repositories, Services)      │
└─────────────────────────────────────────┘
```

## Layer Descriptions

### 1. Domain Layer (Core)

**Location**: `src/ProductCatalog.Domain/`

**Responsibilities**:
- Define business entities (Product, Category, Variant)
- Encapsulate business rules and validation
- Define repository interfaces (no implementation)
- Contain value objects and domain events

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

### 2. Application Layer

**Location**: `src/ProductCatalog.Application/`

**Responsibilities**:
- Define application services and use cases
- Orchestrate domain objects to perform tasks
- Define DTOs for data transfer
- Handle application-level validation
- Map between domain entities and DTOs

**Dependencies**: Domain Layer only

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

### 3. Infrastructure Layer

**Location**: `src/ProductCatalog.Data/`

**Responsibilities**:
- Implement repository interfaces from Domain
- Configure Entity Framework Core
- Implement database context
- Handle data persistence concerns
- Implement external service integrations

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

**Dependencies**: Application Layer and Infrastructure Layer

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
│   ├── ProductCatalog.Domain/           # Core business logic
│   │   ├── Entities/
│   │   │   ├── Product.cs
│   │   │   ├── Category.cs
│   │   │   └── ProductVariant.cs
│   │   ├── Interfaces/
│   │   │   ├── IProductRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   └── Exceptions/
│   │
│   ├── ProductCatalog.Application/      # Use cases & services
│   │   ├── Services/
│   │   │   └── ProductService.cs
│   │   ├── DTOs/
│   │   │   ├── ProductDto.cs
│   │   │   └── CreateProductDto.cs
│   │   └── Mapping/
│   │
│   ├── ProductCatalog.Data/             # Data access
│   │   ├── Context/
│   │   │   └── ApplicationDbContext.cs
│   │   ├── Repositories/
│   │   │   ├── ProductRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── Configurations/
│   │   └── Migrations/
│   │
│   └── ProductCatalog.API/              # Web API
│       ├── Controllers/
│       │   └── ProductsController.cs
│       ├── Middleware/
│       ├── Extensions/
│       └── Program.cs
│
└── tests/
    ├── ProductCatalog.Domain.Tests/
    ├── ProductCatalog.Application.Tests/
    └── ProductCatalog.API.Tests/
```

## Benefits of This Architecture

### 1. Testability
- Domain logic can be tested without database
- Application services can be tested with mock repositories
- API controllers can be tested with mock services

### 2. Maintainability
- Clear separation of concerns
- Easy to locate and modify code
- Changes in one layer don't affect others

### 3. Flexibility
- Can swap database (e.g., PostgreSQL → SQL Server)
- Can change API framework (e.g., ASP.NET → NancyFx)
- Can add new use cases without modifying existing code

### 4. Scalability
- Layers can be deployed independently
- Easy to add new features
- Support for microservices migration

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
- **Application.Tests**: Test services with mock repositories
- **API.Tests**: Integration tests for endpoints

## Anti-Patterns to Avoid

1. **Domain depending on Infrastructure**: Never reference EF Core in domain entities
2. **Anemic Domain Model**: Domain entities should contain business logic, not just properties
3. **Service Locator**: Use dependency injection, not service locator pattern
4. **Leaky Abstractions**: Repository interfaces should not expose EF Core types

## Related Documentation

- [System Diagram](./SystemDiagram.md)
- [Data Model](./DataModel.md)
- [Development Setup](../03-Development/EnvironmentSetup.md)
- [Testing Guide](../03-Development/Testing.md)

## Further Reading

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Onion Architecture](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/)
