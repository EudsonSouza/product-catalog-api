using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using ProductCatalog.API.Health;
using ProductCatalog.API.Middleware;
using ProductCatalog.Data;
using ProductCatalog.Data.Helpers;
using ProductCatalog.Data.Repositories;
using ProductCatalog.Domain.Entities;
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
        });
    appBuilder.Services.AddHttpContextAccessor();

    ConfigureJsonSerialization(appBuilder.Services);
    ConfigureForwardedHeaders(appBuilder.Services);
    ConfigureCors(appBuilder.Services, appBuilder.Configuration);
    ConfigureOpenApi(appBuilder.Services);
    ConfigureDatabase(appBuilder);
    RegisterRepositories(appBuilder.Services);
    RegisterServices(appBuilder.Services);
    ConfigureJwtAuthentication(appBuilder);
    ConfigureAuthorization(appBuilder.Services);
    ConfigureHealthChecks(appBuilder.Services);
}

void ConfigureJsonSerialization(IServiceCollection services)
{
    services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.SerializerOptions.MaxDepth = 128;
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

    services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
}

void ConfigureOpenApi(IServiceCollection services)
{
    services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Description = "Product Catalog API - GET endpoints are public, POST/PUT/DELETE require admin authorization";

            AddJwtSecuritySchemeToSwagger(document);
            ApplyGlobalSecurityRequirement(document);

            return Task.CompletedTask;
        });
    });
}

void AddJwtSecuritySchemeToSwagger(Microsoft.OpenApi.Models.OpenApiDocument document)
{
    document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
    document.Components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.Models.OpenApiSecurityScheme>();
    document.Components.SecuritySchemes.Add("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your JWT token in the text input below."
    });
}

void ApplyGlobalSecurityRequirement(Microsoft.OpenApi.Models.OpenApiDocument document)
{
    document.SecurityRequirements ??= new List<Microsoft.OpenApi.Models.OpenApiSecurityRequirement>();
    document.SecurityRequirements.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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
}

void ConfigureJwtAuthentication(WebApplicationBuilder appBuilder)
{
    var jwtSecret = appBuilder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("JWT Secret is not configured");
    var jwtIssuer = appBuilder.Configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("JWT Issuer is not configured");
    var jwtAudience = appBuilder.Configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("JWT Audience is not configured");

    appBuilder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
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

    // Session authentication middleware (before UseAuthentication)
    application.UseMiddleware<SessionAuthenticationMiddleware>();

    application.UseAuthentication();
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
    public static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };
}
#pragma warning restore CA1052
