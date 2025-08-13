# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY ProductCatalog.sln ./
COPY src/ProductCatalog.API/ProductCatalog.API.csproj src/ProductCatalog.API/
COPY src/ProductCatalog.Data/ProductCatalog.Data.csproj src/ProductCatalog.Data/
COPY src/ProductCatalog.Domain/ProductCatalog.Domain.csproj src/ProductCatalog.Domain/
COPY src/ProductCatalog.Services/ProductCatalog.Services.csproj src/ProductCatalog.Services/
COPY src/ProductCatalog.Migrator/ProductCatalog.Migrator.csproj src/ProductCatalog.Migrator/

RUN dotnet restore src/ProductCatalog.API/ProductCatalog.API.csproj
RUN dotnet restore src/ProductCatalog.Migrator/ProductCatalog.Migrator.csproj

COPY src/ src/

RUN dotnet publish src/ProductCatalog.API/ProductCatalog.API.csproj -c Release -o /out/api --no-restore
RUN dotnet publish src/ProductCatalog.Migrator/ProductCatalog.Migrator.csproj -c Release -o /out/migrator --no-restore

# ===== Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN groupadd -r appgroup && useradd -r -g appgroup appuser

COPY --from=build /out/api/ ./api/
COPY --from=build /out/migrator/ ./migrator/

COPY deploy/entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh && chown -R appuser:appgroup /app /entrypoint.sh

USER appuser

EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0


CMD ["/entrypoint.sh"]
