# Product Catalog API - Overview

## Introduction

The Product Catalog API is a modern REST API built with .NET 9 and C# 12, designed to manage product catalogs with support for multiple variants (color/size combinations). This backend system serves as the data layer for e-commerce applications, specifically tailored for clothing and fashion retail businesses.

## Project Purpose

This API was originally developed to support a small lingerie and pajamas store, providing a robust and scalable solution for managing product information, inventory, and categories. It serves as both a **portfolio project** demonstrating modern .NET development practices and a **real-world business solution** for small to medium-sized retailers.

## Key Features

### Core Functionality
- **Product Management**: Full CRUD operations for products with rich domain validation
- **Product Variants**: Support for multiple variants per product (color/size/stock combinations)
- **Category Management**: Organize products into hierarchical categories
- **Inventory Tracking**: Real-time stock management for each product variant
- **RESTful API**: Clean and consistent REST endpoints with proper HTTP semantics

### Technical Highlights
- **N-Layered Architecture**: Traditional layered architecture with clear separation of concerns
- **Repository Pattern**: Abstracted data access with Unit of Work implementation
- **Entity Framework Core**: Modern ORM with PostgreSQL database support
- **API Documentation**: Interactive Swagger/OpenAPI documentation
- **Testing**: Comprehensive unit and integration test suite with xUnit
- **Rich Domain Models**: Business logic encapsulation in domain entities

## Live Demo

**API Documentation**: [https://product-catalog-api-1t6o.onrender.com/swagger/index.html](https://product-catalog-api-1t6o.onrender.com/swagger/index.html)

**Note**: GET endpoints are public for demo purposes. POST/PUT/DELETE operations require admin authorization.

## Quick Start

```bash
# Clone the repository
git clone <repository-url>
cd product-catalog-api

# Set up environment variables
cp .env.example .env
# Edit .env with your configuration

# Restore packages
dotnet restore

# Run migrations
dotnet ef database update --project src/ProductCatalog.Data --startup-project src/ProductCatalog.API

# Start the API
dotnet run --project src/ProductCatalog.API
```

The API will be available at `https://localhost:7242`.

## Technology Stack

- **.NET 9**: Latest version of the .NET platform
- **C# 12**: Modern C# language features
- **ASP.NET Core**: Web framework for building APIs
- **Entity Framework Core**: Object-relational mapping
- **PostgreSQL**: Production-grade relational database
- **xUnit**: Testing framework
- **Swagger/OpenAPI**: API documentation and testing

## Architecture Overview

The application follows an N-Layered architecture pattern:

```
ProductCatalog.API       → API Layer (Controllers, DTOs, Swagger)
ProductCatalog.Services  → Services Layer (Business Logic, DTOs)
ProductCatalog.Domain    → Domain Layer (Entities, Interfaces)
ProductCatalog.Data      → Data Layer (EF Core, Repositories)
ProductCatalog.Migrator  → Database Migration Tool
```

For detailed architecture information, see [N-Layered Architecture](../01-Architecture/LayeredArchitecture.md).

## Related Documentation

- **Vision & Objectives**: [Vision.md](./Vision.md)
- **Technical Glossary**: [Glossary.md](./Glossary.md)
- **Architecture Decisions**: [ArchitectureDecisionLog.md](./ArchitectureDecisionLog.md)
- **Frontend Repository**: See [Frontend Documentation](../05-Links/FrontendDocs.md)

## Development Team

This project was developed with AI assistance (Claude) for rapid prototyping and architectural exploration, demonstrating modern development workflows and best practices.

## License

This project is open-sourced for portfolio and educational purposes.

---

**Next Steps**:
- Review the [System Architecture](../01-Architecture/SystemDiagram.md)
- Explore the [API Endpoints](../02-API/Endpoints.md)
- Set up your [Development Environment](../03-Development/EnvironmentSetup.md)
