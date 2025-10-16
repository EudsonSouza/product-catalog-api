# System Diagram

> _This document provides a visual overview of the Product Catalog API system architecture and its integration with the frontend._

---
last_updated: 2025-10-15
source: created
---

## Purpose

This document illustrates the high-level system architecture, showing how different components interact and how the backend integrates with the frontend application.

## System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Client Layer                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Product Catalog Frontend (Next.js)                  │   │
│  │  - Product listing and search                        │   │
│  │  - Category filtering                                 │   │
│  │  - Product details                                    │   │
│  │  - WhatsApp integration                              │   │
│  └─────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP/HTTPS (REST API)
                         │
┌────────────────────────▼────────────────────────────────────┐
│                    API Layer                                 │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  ASP.NET Core Web API (.NET 9)                       │   │
│  │  ┌─────────────────────────────────────────────┐    │   │
│  │  │  Controllers                                 │    │   │
│  │  │  - ProductsController                        │    │   │
│  │  │  - CategoriesController                      │    │   │
│  │  │  - VariantsController                        │    │   │
│  │  └─────────────────────────────────────────────┘    │   │
│  │  ┌─────────────────────────────────────────────┐    │   │
│  │  │  Middleware                                  │    │   │
│  │  │  - Error handling                            │    │   │
│  │  │  - CORS policy                               │    │   │
│  │  │  - Request logging                           │    │   │
│  │  └─────────────────────────────────────────────┘    │   │
│  └─────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                 Application Layer                            │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Services / Use Cases                                │   │
│  │  - ProductService                                    │   │
│  │  - CategoryService                                   │   │
│  │  - InventoryService                                  │   │
│  └─────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                   Domain Layer                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Domain Models                                       │   │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐   │   │
│  │  │  Product   │  │  Category  │  │  Variant   │   │   │
│  │  └────────────┘  └────────────┘  └────────────┘   │   │
│  │                                                      │   │
│  │  Business Logic & Validation Rules                  │   │
│  └─────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│              Infrastructure Layer                            │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Data Access (EF Core)                               │   │
│  │  ┌────────────────────────────────────────────┐     │   │
│  │  │  Repositories                               │     │   │
│  │  │  - ProductRepository                        │     │   │
│  │  │  - CategoryRepository                       │     │   │
│  │  │  - VariantRepository                        │     │   │
│  │  └────────────────────────────────────────────┘     │   │
│  │  ┌────────────────────────────────────────────┐     │   │
│  │  │  Unit of Work                               │     │   │
│  │  └────────────────────────────────────────────┘     │   │
│  └─────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                  Data Layer                                  │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  PostgreSQL Database                                 │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌──────────┐   │   │
│  │  │  Products   │  │  Categories │  │ Variants │   │   │
│  │  │   Table     │  │    Table    │  │  Table   │   │   │
│  │  └─────────────┘  └─────────────┘  └──────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Component Descriptions

### Client Layer (Frontend)
- **Technology**: Next.js 14+ with React 18 and TypeScript
- **Responsibilities**:
  - User interface rendering
  - Product browsing and search
  - Category filtering
  - WhatsApp integration for purchases
- **Documentation**: [Frontend Documentation](../05-Links/FrontendDocs.md)

### API Layer
- **Technology**: ASP.NET Core Web API (.NET 9)
- **Responsibilities**:
  - HTTP request handling
  - Input validation
  - Response formatting
  - CORS policy enforcement
  - Error handling middleware

### Application Layer
- **Responsibilities**:
  - Use case orchestration
  - Business workflow coordination
  - DTO mapping
  - Service-level validation

### Domain Layer
- **Responsibilities**:
  - Core business entities
  - Business logic encapsulation
  - Domain validation rules
  - Aggregate relationships

### Infrastructure Layer
- **Technology**: Entity Framework Core with PostgreSQL
- **Responsibilities**:
  - Data persistence
  - Database access abstraction
  - Transaction management
  - Query optimization

### Data Layer
- **Technology**: PostgreSQL 15+
- **Responsibilities**:
  - Data storage
  - Data integrity enforcement
  - Query execution
  - Index management

## Data Flow

### Read Operation (GET /api/products)

```
1. Client (Frontend) → HTTP GET Request
2. API Controller → Receives request
3. Application Service → Processes business logic
4. Repository → Queries database via EF Core
5. PostgreSQL → Returns data
6. EF Core → Maps to domain entities
7. Application Service → Maps to DTOs
8. API Controller → Returns JSON response
9. Client → Renders products
```

### Write Operation (POST /api/products)

```
1. Client → HTTP POST Request with product data
2. API Controller → Validates input
3. Application Service → Creates domain entity
4. Domain Entity → Validates business rules
5. Unit of Work → Begins transaction
6. Repository → Adds entity to context
7. Unit of Work → Commits transaction
8. EF Core → Persists to PostgreSQL
9. API Controller → Returns 201 Created
10. Client → Updates UI
```

## Integration Points

### Frontend → Backend
- **Protocol**: HTTP/HTTPS
- **Format**: JSON
- **Authentication**: [To be implemented]
- **CORS**: Configured for frontend domain

### Backend → Database
- **Protocol**: PostgreSQL native protocol
- **ORM**: Entity Framework Core
- **Connection**: Connection string from environment variables
- **Pooling**: Enabled for performance

## Deployment Architecture

```
┌─────────────────────────────────────────────┐
│           Production Environment            │
│                                             │
│  ┌───────────────────────────────────┐    │
│  │  Frontend (Vercel/Render)         │    │
│  │  - Next.js SSR/Static             │    │
│  │  - Edge CDN                        │    │
│  └─────────────┬─────────────────────┘    │
│                │                            │
│                │ HTTPS                      │
│                │                            │
│  ┌─────────────▼─────────────────────┐    │
│  │  Backend API (Render/Railway)     │    │
│  │  - .NET 9 Runtime                 │    │
│  │  - Auto-scaling                   │    │
│  └─────────────┬─────────────────────┘    │
│                │                            │
│                │ PostgreSQL Protocol        │
│                │                            │
│  ┌─────────────▼─────────────────────┐    │
│  │  PostgreSQL Database              │    │
│  │  - Managed instance               │    │
│  │  - Automated backups              │    │
│  └───────────────────────────────────┘    │
└─────────────────────────────────────────────┘
```

## Security Architecture

```
┌──────────────────────────────────────┐
│  Security Layers                     │
│                                      │
│  1. HTTPS/TLS Encryption            │
│  2. CORS Policy                     │
│  3. Input Validation                │
│  4. SQL Injection Prevention (EF)   │
│  5. [Future] Authentication         │
│  6. [Future] Authorization          │
└──────────────────────────────────────┘
```

## Performance Considerations

- **Caching**: [To be implemented] Response caching for product listings
- **Connection Pooling**: Enabled for database connections
- **Async/Await**: All I/O operations are asynchronous
- **Pagination**: Implemented for large result sets
- **Indexing**: Database indexes on frequently queried columns

## Scalability Strategy

- **Horizontal Scaling**: Stateless API design allows multiple instances
- **Database Scaling**: PostgreSQL read replicas for read-heavy workloads
- **Caching Layer**: [Future] Redis for frequently accessed data
- **CDN**: Frontend assets served via CDN

## Related Documentation

- [Clean Architecture](./CleanArchitecture.md)
- [Data Model](./DataModel.md)
- [API Endpoints](../02-API/Endpoints.md)
- [Deployment Guide](./Deployment.md)
- [Frontend Integration](../05-Links/FrontendDocs.md)

## Notes

This diagram represents the current state of the system architecture. As the project evolves, this document should be updated to reflect architectural changes and new components.

## Future Enhancements

- **Authentication Service**: JWT-based authentication
- **Image Service**: Cloud storage integration for product images
- **Cache Layer**: Redis for performance optimization
- **Message Queue**: For asynchronous operations
- **API Gateway**: For advanced routing and load balancing
