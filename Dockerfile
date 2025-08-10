# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy solution file and project files
COPY ProductCatalog.sln ./
COPY src/ProductCatalog.API/ProductCatalog.API.csproj src/ProductCatalog.API/
COPY src/ProductCatalog.Data/ProductCatalog.Data.csproj src/ProductCatalog.Data/
COPY src/ProductCatalog.Domain/ProductCatalog.Domain.csproj src/ProductCatalog.Domain/
COPY src/ProductCatalog.Services/ProductCatalog.Services.csproj src/ProductCatalog.Services/
COPY tests/ProductCatalog.Tests.Unit/ProductCatalog.Tests.Unit.csproj tests/ProductCatalog.Tests.Unit/
COPY tests/ProductCatalog.Tests.Integration/ProductCatalog.Tests.Integration.csproj tests/ProductCatalog.Tests.Integration/

# Restore dependencies
RUN dotnet restore src/ProductCatalog.API/ProductCatalog.API.csproj

# Copy source code
COPY src/ src/
COPY tests/ tests/

# Build and publish the application
RUN dotnet publish src/ProductCatalog.API/ProductCatalog.API.csproj -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published application
COPY --from=build /app ./

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "ProductCatalog.API.dll"]