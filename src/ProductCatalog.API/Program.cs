using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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


// ---- Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: HealthStatics.LiveTags)
    .AddCheck<DbHealthCheck>("database", tags: HealthStatics.ReadyTags);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

static Task WriteJsonResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var payload = new
    {
        status = report.Status.ToString(),
        timestamp = DateTime.UtcNow,
        version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
        totalDuration = $"{report.TotalDuration.TotalMilliseconds}ms",
        checks = report.Entries.Select(kvp => new
        {
            name = kvp.Key,
            status = kvp.Value.Status.ToString(),
            duration = $"{kvp.Value.Duration.TotalMilliseconds}ms",
            description = kvp.Value.Description,
            data = kvp.Value.Data
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
