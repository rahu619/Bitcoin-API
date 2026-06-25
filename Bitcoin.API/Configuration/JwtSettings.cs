namespace BitCoin.API.Configuration;

/// <summary>
/// The JWT authentication configuration entity.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// The symmetric signing key used to validate JWT signatures.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The expected token issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The expected token audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;
}
