namespace YubiOn.Fss.Sdk.Config;

/// <summary>
/// Configuration for the YubiOn FSS SDK client.
/// </summary>
public class FssSdkConfig
{
    /// <summary>
    /// Default API endpoint URL.
    /// </summary>
    public const string DefaultEndpoint = "https://fss-app.yubion.com/api/";

    /// <summary>
    /// Default user agent identifier.
    /// </summary>
    public const string DefaultAgent = "yubion-fido2-server-sdk-dotnet";

    /// <summary>
    /// Relying Party identifier (RP ID).
    /// </summary>
    public string? RpId { get; set; }

    /// <summary>
    /// API authentication identifier.
    /// </summary>
    public string? ApiAuthId { get; set; }

    /// <summary>
    /// Authentication type.
    /// </summary>
    public FssApiAuthType ApiAuthType { get; set; }

    /// <summary>
    /// Secret key (PKCS#8 EC private key in Base64URL format for signature auth, or access key secret).
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// API endpoint base URL.
    /// </summary>
    public string Endpoint { get; set; } = DefaultEndpoint;

    /// <summary>
    /// User agent string sent with API requests.
    /// </summary>
    public string Agent { get; set; } = DefaultAgent;

    public FssSdkConfig()
    {
    }

    public FssSdkConfig(string rpId, string apiAuthId, FssApiAuthType apiAuthType, string secretKey)
        : this(rpId, apiAuthId, apiAuthType, secretKey, DefaultEndpoint, DefaultAgent)
    {
    }

    public FssSdkConfig(string rpId, string apiAuthId, string apiAuthType, string secretKey)
        : this(rpId, apiAuthId, Enum.Parse<FssApiAuthType>(apiAuthType, true), secretKey, DefaultEndpoint, DefaultAgent)
    {
    }

    public FssSdkConfig(string rpId, string apiAuthId, FssApiAuthType apiAuthType, string secretKey, string endpoint)
        : this(rpId, apiAuthId, apiAuthType, secretKey, endpoint, DefaultAgent)
    {
    }

    public FssSdkConfig(string rpId, string apiAuthId, FssApiAuthType apiAuthType, string secretKey, string endpoint, string agent)
    {
        RpId = rpId;
        ApiAuthId = apiAuthId;
        ApiAuthType = apiAuthType;
        SecretKey = secretKey;
        Endpoint = endpoint ?? DefaultEndpoint;
        Agent = agent ?? DefaultAgent;
    }

    public FssSdkConfig(FssSdkConfig baseConfig)
    {
        if (baseConfig == null) throw new ArgumentNullException(nameof(baseConfig));
        RpId = baseConfig.RpId;
        ApiAuthId = baseConfig.ApiAuthId;
        ApiAuthType = baseConfig.ApiAuthType;
        SecretKey = baseConfig.SecretKey;
        Endpoint = baseConfig.Endpoint;
        Agent = baseConfig.Agent;
    }
}
