namespace ProductCatalog.API.Configuration;

public class SessionSettings
{
    public required string CookieName { get; init; } = "product_catalog_session";
    public required int ExpirationHours { get; init; } = 8;
}
