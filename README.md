# Product Catalog API

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/EudsonSouza/8cdc218770b5707458a6c377a1612355/raw/coverage.json)](./docs/03-Development/Coverage.md)
[![Tests](https://github.com/EudsonSouza/product-catalog-api/actions/workflows/test-coverage.yml/badge.svg)](https://github.com/EudsonSouza/product-catalog-api/actions/workflows/test-coverage.yml)

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
- N-Layered architecture (API → Services → Domain → Data)
- Dependency injection
- Repository pattern with Unit of Work
- Unit and integration test setup

## Next Features

🔜 **Authentication & Authorization**
🔜 **Image upload handling**
🔜 **Advanced search and filtering**
🔜 **Inventory alerts**
🔜 **Performance optimizations**

## How to Run

### Option 1: Docker (Recomendado)

```bash
# Clone repository
git clone <repo-url>
cd product-catalog-api

# Configure environment
cp .env.example .env
# Edit .env with your database and JWT settings

# Start with Docker Compose
docker compose up --build
```

API: `http://localhost:5080`
Swagger: `http://localhost:5080/swagger`

### Option 2: Local Development

**Prerequisites:** .NET 9 SDK + PostgreSQL

```bash
# Clone and configure
git clone <repo-url>
cd product-catalog-api
cp .env.example .env

# Restore and run migrations
dotnet restore
dotnet ef database update --project src/ProductCatalog.Data --startup-project src/ProductCatalog.API

# Run the API
dotnet run --project src/ProductCatalog.API
```

API: `https://localhost:7242`

### Environment Variables

Required in `.env`:
- **Database**: `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USERNAME`, `DB_PASSWORD`
- **JWT**: `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationHours`

## Testing with Postman

### Login
```http
POST http://localhost:5080/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "your_password"
}
```

**Response:**
```json
{
  "token": "eyJhbGci...",
  "username": "admin",
  "expiresAt": "2025-10-21T10:30:00Z"
}
```

### Using Token in Protected Endpoints
Add header to POST/PUT/DELETE requests:
```
Authorization: Bearer YOUR_TOKEN_HERE
```

## Documentation

📚 **Complete docs**: [/docs](./docs/00-Overview/README.md)
📖 **API Endpoints**: [API_ENDPOINTS.md](./API_ENDPOINTS.md)

## Project Origin

Built for my mother's pajama store as a learning project to explore modern .NET development practices applied to a real business domain.

Developed with AI assistance (Claude) for rapid prototyping and architectural exploration.

---
*🚧 Work in progress*
