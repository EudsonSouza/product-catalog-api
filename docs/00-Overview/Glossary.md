# Glossary

> _This document provides definitions of technical terms and concepts used throughout the Product Catalog API documentation._

---
last_updated: 2025-10-15
source: created
---

## Purpose

This glossary serves as a reference for technical terminology, domain-specific concepts, and acronyms used in the Product Catalog API project. It helps developers, stakeholders, and contributors understand the project's vocabulary.

## Domain Terms

### Product
A catalog item representing a physical item for sale. Products can have multiple variants based on size, color, and other attributes.

### Product Variant
A specific combination of a product's attributes (e.g., Red T-Shirt in size M). Each variant has its own stock quantity and pricing.

### Category
A classification used to organize products into groups (e.g., "Lingerie", "Pajamas", "Accessories").

### SKU (Stock Keeping Unit)
A unique identifier for each product variant used for inventory tracking.

### Inventory
The quantity of a specific product variant available for sale.

### Gender
Product classification by intended audience: Male (1), Female (2), or Unisex (0).

## Technical Terms

### REST API
Representational State Transfer - an architectural style for building web APIs using HTTP methods.

### DTO (Data Transfer Object)
An object that carries data between processes, typically used to transfer data between the API and client applications.

### Entity
A domain model class that represents a business object with identity and lifecycle (e.g., Product, Category).

### Repository Pattern
A design pattern that abstracts data access logic and provides a collection-like interface for accessing domain objects.

### Unit of Work
A pattern that maintains a list of objects affected by a business transaction and coordinates the writing out of changes.

### N-Layered Architecture
A traditional architectural pattern that organizes code into horizontal layers (API, Services, Domain, Data) with dependencies flowing downward.

### Domain-Driven Design (DDD)
An approach to software development that emphasizes collaboration between technical and domain experts to create a model of the business domain.

## Technology Terms

### .NET 9
The latest version of Microsoft's open-source, cross-platform framework for building applications.

### C# 12
The latest version of the C# programming language with modern features and syntax improvements.

### Entity Framework Core (EF Core)
An object-relational mapping (ORM) framework for .NET that enables developers to work with databases using .NET objects.

### PostgreSQL
An open-source relational database management system used for data persistence.

### Swagger/OpenAPI
A specification and toolset for documenting and testing REST APIs.

### xUnit
A testing framework for .NET applications.

## Acronyms

- **API**: Application Programming Interface
- **CRUD**: Create, Read, Update, Delete
- **HTTP**: Hypertext Transfer Protocol
- **JSON**: JavaScript Object Notation
- **ORM**: Object-Relational Mapping
- **REST**: Representational State Transfer
- **DTO**: Data Transfer Object
- **TDD**: Test-Driven Development
- **DI**: Dependency Injection
- **SOLID**: Single responsibility, Open-closed, Liskov substitution, Interface segregation, Dependency inversion

## Status Values

### Product.IsActive
- **true**: Product is visible and available for sale
- **false**: Product is hidden from public view

### Product.IsFeatured
- **true**: Product should be highlighted in featured sections
- **false**: Regular product display

## API Response Codes

- **200 OK**: Successful GET request
- **201 Created**: Successful POST request
- **204 No Content**: Successful DELETE request
- **400 Bad Request**: Invalid input data
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server-side error

## Related Documentation

- [Vision and Objectives](./Vision.md)
- [Architecture Overview](../01-Architecture/LayeredArchitecture.md)
- [API Endpoints](../02-API/Endpoints.md)

## Notes

This glossary is a living document. As new terms and concepts are introduced to the project, they should be added here with clear, concise definitions.
