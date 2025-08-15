# Product Catalog API

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)

REST API for managing product catalogs with variants.

## 🚀 Live Demo

📖 **API Documentation**: https://product-catalog-api-1t6o.onrender.com/swagger/index.html

*Note: GET endpoints are public for demo purposes. POST/PUT/DELETE operations require admin authorization.*

## What it does

Product catalog system for clothing e-commerce. Manages products with multiple variants (color/size combinations), categories, and inventory tracking.

## Tech Stack

- .NET 9 / C# 12
- Entity Framework Core
- PostgreSQL
- Repository Pattern + Unit of Work
- xUnit for testing

## Features Implemented

✅ **Core Domain**
- Product entities with business logic
- Product variants (color/size/stock)
- Categories and basic attributes
- Rich domain validation

✅ **Data Layer**
- EF Core with PostgreSQL
- Repository pattern
- Database migrations
- Entity configurations

✅ **API Layer**
- RESTful controllers
- CRUD operations for Products
- Swagger documentation
- DTO mappings

✅ **Architecture**
- Layered architecture structure
- Dependency injection
- Separated concerns
- Unit and integration test setup

## Next Features

🔜 **Authentication & Authorization**
🔜 **Image upload handling**
🔜 **Advanced search and filtering**
🔜 **Inventory alerts**
🔜 **Performance optimizations**

## How to Run

### Prerequisites
- .NET 9 SDK
- PostgreSQL database

### Setup
```bash
# Clone repository
git clone <repo-url>
cd product-catalog-api

# Create environment file from template
cp .env.example .env

# Edit .env file with your configuration
# Update database credentials and other settings

# Restore packages
dotnet restore

# Run migrations
dotnet ef database update --project src/ProductCatalog.Data --startup-project src/ProductCatalog.API

# Run the API
dotnet run --project src/ProductCatalog.API
```

### Environment Configuration

The application uses environment variables for configuration. Create a `.env` file from the template:

```bash
cp .env.example .env
```

Required environment variables:
- `DB_HOST` - Database host (localhost for development)
- `DB_PORT` - Database port (5432 for PostgreSQL)
- `DB_NAME` - Database name
- `DB_USERNAME` - Database username
- `DB_PASSWORD` - Database password

API will be available at `https://localhost:7242`.

## Project Origin

Built for my mother's pajama store as a learning project to explore modern .NET development practices applied to a real business domain.

Developed with AI assistance (Claude) for rapid prototyping and architectural exploration.

---
*🚧 Work in progress*
