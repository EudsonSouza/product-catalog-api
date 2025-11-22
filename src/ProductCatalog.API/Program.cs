using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProductCatalog.API.Health;
using ProductCatalog.API.Middleware;
using ProductCatalog.Data;
using ProductCatalog.Data.Helpers;
using ProductCatalog.Data.Repositories;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Services;
using ProductCatalog.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);

var app = builder.Build();

ConfigureMiddleware(app);
ConfigureEndpoints(app);

app.Run();

void ConfigureServices(WebApplicationBuilder appBuilder)
{
    appBuilder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.MaxDepth = 128;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    appBuilder.Services.AddHttpContextAccessor();

    ConfigureJsonSerialization(appBuilder.Services);
    ConfigureForwardedHeaders(appBuilder.Services);
    ConfigureCors(appBuilder.Services, appBuilder.Configuration);
    ConfigureOpenApi(appBuilder.Services);
    ConfigureDatabase(appBuilder);
    RegisterRepositories(appBuilder.Services);
    RegisterServices(appBuilder.Services);
    ConfigureAuthorization(appBuilder.Services);
    ConfigureHealthChecks(appBuilder.Services);
}

void ConfigureJsonSerialization(IServiceCollection services)
{
    services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.SerializerOptions.MaxDepth = 128;
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
}

void ConfigureForwardedHeaders(IServiceCollection services)
{
    services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

void ConfigureCors(IServiceCollection services, IConfiguration configuration)
{
    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000", "http://localhost:3001" };

    var allowVercelPreviews = configuration.GetValue<bool>("Cors:AllowVercelPreviews", false);

    services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();

            if (allowVercelPreviews)
            {
                policy.SetIsOriginAllowed(origin =>
                {
                    if (string.IsNullOrEmpty(origin))
                        return false;

                    var uri = new Uri(origin);
                    return uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
                });
            }
        });
    });
}

void ConfigureOpenApi(IServiceCollection services)
{
    services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Description = "Product Catalog API - GET endpoints are public, POST/PUT/DELETE require admin authorization (Session-based via Google OAuth)";
            return Task.CompletedTask;
        });
    });
}

void ConfigureDatabase(WebApplicationBuilder appBuilder)
{
    var rawConnectionString = appBuilder.Configuration.GetConnectionString("DefaultConnection")
                            ?? appBuilder.Configuration["ConnectionStrings__DefaultConnection"]
                            ?? appBuilder.Configuration["DATABASE_URL"];

    var connectionString = DatabaseConnectionHelper.ConvertToNpgsqlConnectionString(rawConnectionString);

    appBuilder.Services.AddDbContext<ProductCatalogDbContext>(options =>
        options.UseNpgsql(connectionString));
}

void RegisterRepositories(IServiceCollection services)
{
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<ICategoryRepository, CategoryRepository>();
    services.AddScoped<IColorRepository, ColorRepository>();
    services.AddScoped<ISizeRepository, SizeRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
}

void RegisterServices(IServiceCollection services)
{
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<IColorService, ColorService>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ISessionService, SessionService>();
    services.AddHttpClient<IGoogleOAuthService, GoogleOAuthService>();

    // Configure settings
    services.AddSingleton(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        return new ProductCatalog.API.Configuration.SessionSettings
        {
            CookieName = config["Session:CookieName"] ?? "product_catalog_session",
            ExpirationHours = int.Parse(config["Session:ExpirationHours"] ?? "8")
        };
    });

    services.AddSingleton(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        return new ProductCatalog.API.Configuration.FrontendSettings
        {
            BaseUrl = config["Frontend:BaseUrl"] ?? "http://localhost:3000"
        };
    });
}

void ConfigureAuthorization(IServiceCollection services)
{
    services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdmin", policy =>
            policy.RequireClaim("role", "admin"));
}

void ConfigureHealthChecks(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: HealthStatics.LiveTags)
        .AddCheck<DbHealthCheck>("database", tags: HealthStatics.ReadyTags);
}

void ConfigureMiddleware(WebApplication application)
{
    EnableSwaggerIfConfigured(application);

    application.UseForwardedHeaders();
    application.UseHttpsRedirection();
    application.UseCors("AllowFrontend");

    // Session authentication middleware
    application.UseMiddleware<SessionAuthenticationMiddleware>();

    application.UseAuthorization();
}

void EnableSwaggerIfConfigured(WebApplication application)
{
    var enableSwagger = builder.Configuration.GetValue<bool>("ENABLE_SWAGGER", application.Environment.IsDevelopment());

    if (enableSwagger)
    {
        application.MapOpenApi();
        application.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Product Catalog API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Product Catalog API - Read Only Demo";
        });
    }
}

void ConfigureEndpoints(WebApplication application)
{
    MapLivenessHealthCheck(application);
    MapReadinessHealthCheck(application);
    application.MapControllers();
}

void MapLivenessHealthCheck(WebApplication application)
{
    application.MapHealthChecks("/health/live", new HealthCheckOptions
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
}

void MapReadinessHealthCheck(WebApplication application)
{
    application.MapHealthChecks("/health/ready", new HealthCheckOptions
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
}

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

#pragma warning disable CA1052
internal static class HealthStatics
{
    public static readonly string[] LiveTags = new[] { "live" };
    public static readonly string[] ReadyTags = new[] { "ready" };
    public static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
#pragma warning restore CA1052
