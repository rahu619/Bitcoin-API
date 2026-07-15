namespace BitCoin.API.Configuration;

/// <summary>
/// CORS configuration for the API's default policy.
/// </summary>
public sealed class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = [];
}
