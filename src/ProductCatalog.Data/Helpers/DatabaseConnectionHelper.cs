using System;

namespace ProductCatalog.Data.Helpers;

public static class DatabaseConnectionHelper
{
    private const int DefaultPostgreSqlPort = 5432;
    private const string DefaultSslMode = "Require";
    
    public static string ConvertToNpgsqlConnectionString(string? rawConnectionString)
    {
        ValidateConnectionString(rawConnectionString);
        
        if (IsAlreadyNpgsqlFormat(rawConnectionString))
            return rawConnectionString;
            
        if (IsPostgreSqlUrl(rawConnectionString))
            return ConvertUrlToConnectionString(rawConnectionString);
            
        throw new ArgumentException("Connection string must be either Npgsql format or PostgreSQL URL format.");
    }
    
    private static void ValidateConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string cannot be null or empty.");
    }
    
    private static bool IsAlreadyNpgsqlFormat(string connectionString)
    {
        return connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase);
    }
    
    private static bool IsPostgreSqlUrl(string connectionString)
    {
        return connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
               connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);
    }
    
    private static string ConvertUrlToConnectionString(string postgresUrl)
    {
        var normalizedUrl = NormalizePostgreSqlUrl(postgresUrl);
        var uri = new Uri(normalizedUrl);
        
        var credentials = ParseCredentials(uri.UserInfo);
        var connectionDetails = ParseConnectionDetails(uri);
        var sslMode = ParseSslModeFromQuery(uri.Query);
        
        return BuildNpgsqlConnectionString(
            host: connectionDetails.Host,
            port: connectionDetails.Port,
            database: connectionDetails.Database,
            username: credentials.Username,
            password: credentials.Password,
            sslMode: sslMode
        );
    }
    
    private static string NormalizePostgreSqlUrl(string url)
    {
        return url.Replace("postgresql://", "postgres://", StringComparison.OrdinalIgnoreCase);
    }
    
    private static (string Username, string Password) ParseCredentials(string userInfo)
    {
        var parts = userInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(parts[0]);
        var password = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
        
        return (username, password);
    }
    
    private static (string Host, int Port, string Database) ParseConnectionDetails(Uri uri)
    {
        var host = uri.Host;
        var port = uri.IsDefaultPort ? DefaultPostgreSqlPort : uri.Port;
        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        
        return (host, port, database);
    }
    
    private static string ParseSslModeFromQuery(string? query)
    {
        if (string.IsNullOrEmpty(query))
            return DefaultSslMode;
            
        var queryParams = query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var param in queryParams)
        {
            var keyValue = param.Split('=', 2);
            if (keyValue.Length == 2 && 
                keyValue[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(keyValue[1]);
            }
        }
        
        return DefaultSslMode;
    }
    
    private static string BuildNpgsqlConnectionString(
        string host, 
        int port, 
        string database, 
        string username, 
        string password, 
        string sslMode)
    {
        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode};Trust Server Certificate=true";
    }
}