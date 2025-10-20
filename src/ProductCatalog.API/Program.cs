using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProductCatalog.API.Health;
using ProductCatalog.Data;
using ProductCatalog.Data.Helpers;
using ProductCatalog.Data.Repositories;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Services;
using ProductCatalog.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Description = "Product Catalog API - GET endpoints are public, POST/PUT/DELETE require admin authorization";
        return Task.CompletedTask;
    });
});

var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                        ?? builder.Configuration["ConnectionStrings__DefaultConnection"]
                        ?? builder.Configuration["DATABASE_URL"];

var connectionString = DatabaseConnectionHelper.ConvertToNpgsqlConnectionString(rawConnectionString);

builder.Services.AddDbContext<ProductCatalogDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IColorRepository, ColorRepository>();
builder.Services.AddScoped<ISizeRepository, SizeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdmin", policy =>
        policy.RequireClaim("role", "admin"));

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: HealthStatics.LiveTags)
    .AddCheck<DbHealthCheck>("database", tags: HealthStatics.ReadyTags);

var app = builder.Build();

var enableSwagger = builder.Configuration.GetValue<bool>("ENABLE_SWAGGER", app.Environment.IsDevelopment());

if (enableSwagger)
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Product Catalog API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Product Catalog API - Read Only Demo";
    });
}

// IMPORTANT: apply forwarded headers BEFORE https redirection/auth/etc.
app.UseForwardedHeaders();

app.UseHttpsRedirection();

static Task WriteJsonResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var payload = new
    {
        status = report.Status.ToString(),
        timestamp = DateTime.UtcNow,
        totalDuration = $"{report.TotalDuration.TotalMilliseconds}ms",
        checks = report.Entries.Select(kvp => new
        {
            name = kvp.Key,
            status = kvp.Value.Status.ToString(),
            duration = $"{kvp.Value.Duration.TotalMilliseconds}ms"
        })
    };

    var json = JsonSerializer.Serialize(payload, HealthStatics.JsonOpts);
    return context.Response.WriteAsync(json);
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Name == "self",
    ResponseWriter = WriteJsonResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
}).WithOpenApi(operation =>
{
    operation.Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
    {
        new() { Name = "Health" }
    };
    operation.Summary = "Liveness health check";
    operation.Description = "Returns the application liveness status";
    return operation;
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = WriteJsonResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
}).WithOpenApi(operation =>
{
    operation.Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
    {
        new() { Name = "Health" }
    };
    operation.Summary = "Readiness health check";
    operation.Description = "Returns the application readiness status including database connectivity";
    return operation;
});

app.MapControllers();

app.Run();

#pragma warning disable CA1052
internal static class HealthStatics
{
    public static readonly string[] LiveTags = new[] { "live" };
    public static readonly string[] ReadyTags = new[] { "ready" };
    public static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };
}
#pragma warning restore CA1052
