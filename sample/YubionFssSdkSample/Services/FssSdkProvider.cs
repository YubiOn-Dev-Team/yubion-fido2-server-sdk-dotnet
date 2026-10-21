using YubiOn.Fss.Sdk.Config;

namespace YubiOn.Fss.Sdk.Sample.Services;

/// <summary>
/// Provides configuration loading and on-demand initialization for the YubiOn FSS SDK client.
/// </summary>
public class FssSdkProvider
{
    private readonly ILogger<FssSdkProvider> _logger;
    private readonly Lazy<YubiOnFssSdk> _sdkLazy;
    private Exception? _initException;

    public FssSdkConfig? Config { get; }
    public bool IsConfigured => _initException == null && Config != null;

    public FssSdkProvider(IConfiguration configuration, ILogger<FssSdkProvider> logger)
    {
        _logger = logger;

        Config = LoadConfiguration(configuration);

        _sdkLazy = new Lazy<YubiOnFssSdk>(() =>
        {
            if (Config == null)
            {
                throw new InvalidOperationException("FSS SDK configuration is missing.");
            }

            if (string.IsNullOrWhiteSpace(Config.RpId) ||
                string.IsNullOrWhiteSpace(Config.ApiAuthId) ||
                string.IsNullOrWhiteSpace(Config.SecretKey) ||
                Config.ApiAuthId.Contains("your_api_key_id") ||
                Config.SecretKey.Contains("your_api_secret"))
            {
                throw new InvalidOperationException(
                    "FSS SDK credentials are not configured or still have placeholder values. " +
                    "Please set your RP ID, API Auth ID, and Secret Key in appsettings.json, environment variables, or environment.properties.");
            }

            try
            {
                // In local development, if connecting to test server with self-signed SSL or mock, allowBadSsl may be useful
                return new YubiOnFssSdk(Config, denyUnknownResponseMember: true, allowBadSslForDebug: true);
            }
            catch (Exception ex)
            {
                _initException = ex;
                _logger.LogError(ex, "Failed to initialize YubiOnFssSdk: {Message}", ex.Message);
                throw new InvalidOperationException(
                    $"Failed to initialize FSS SDK: {ex.Message}. Please check if the SecretKey format matches the ApiAuthType.", ex);
            }
        });
    }

    public YubiOnFssSdk GetSdk()
    {
        try
        {
            return _sdkLazy.Value;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"FSS SDK initialization error: {ex.Message}", ex);
        }
    }

    private FssSdkConfig LoadConfiguration(IConfiguration configuration)
    {
        var fssSection = configuration.GetSection("Fss");

        // 1. Try environment.properties from current or parent directories (common in local SDK dev)
        var props = LoadPropertiesFile();

        // 2. Resolve values with priority: Environment Variables > appsettings.json > environment.properties
        var rpId = Environment.GetEnvironmentVariable("FSS_RP_ID")
                   ?? (fssSection["RpId"] != "your-app.localhost" && !string.IsNullOrEmpty(fssSection["RpId"]) ? fssSection["RpId"] : null)
                   ?? (props.TryGetValue("fss.rp_id", out var pRpId) ? pRpId : null)
                   ?? fssSection["RpId"]
                   ?? string.Empty;

        var authTypeStr = Environment.GetEnvironmentVariable("FSS_API_AUTH_TYPE")
                          ?? fssSection["ApiAuthType"]
                          ?? "NonceSignAuth";

        if (!Enum.TryParse<FssApiAuthType>(authTypeStr, ignoreCase: true, out var authType))
        {
            authType = FssApiAuthType.NonceSignAuth;
        }

        string? defaultAuthIdFromProp = null;
        string? defaultSecretKeyFromProp = null;

        if (authType == FssApiAuthType.NonceSignAuth)
        {
            props.TryGetValue("fss.nonce_auth_id", out defaultAuthIdFromProp);
            props.TryGetValue("fss.nonce_secret_key_ok", out defaultSecretKeyFromProp);
        }
        else if (authType == FssApiAuthType.DatetimeSignAuth)
        {
            props.TryGetValue("fss.datetime_auth_id", out defaultAuthIdFromProp);
            props.TryGetValue("fss.datetime_secret_key_ok", out defaultSecretKeyFromProp);
        }
        else if (authType == FssApiAuthType.AccessKeyAuth)
        {
            props.TryGetValue("fss.access_key_auth_id", out defaultAuthIdFromProp);
            props.TryGetValue("fss.access_key_secret_key_ok", out defaultSecretKeyFromProp);
        }

        var apiKeyId = Environment.GetEnvironmentVariable("FSS_API_KEY_ID")
                      ?? (fssSection["ApiAuthId"] != "your_api_key_id" && !string.IsNullOrEmpty(fssSection["ApiAuthId"]) ? fssSection["ApiAuthId"] : null)
                      ?? defaultAuthIdFromProp
                      ?? fssSection["ApiAuthId"]
                      ?? string.Empty;

        var apiSecret = Environment.GetEnvironmentVariable("FSS_API_SECRET")
                        ?? (fssSection["SecretKey"] != "your_api_secret" && !string.IsNullOrEmpty(fssSection["SecretKey"]) ? fssSection["SecretKey"] : null)
                        ?? defaultSecretKeyFromProp
                        ?? fssSection["SecretKey"]
                        ?? string.Empty;

        var endpoint = Environment.GetEnvironmentVariable("FSS_API_BASE_URL")
                       ?? (props.TryGetValue("fss.endpoint", out var pEndpoint) ? pEndpoint : null)
                       ?? fssSection["Endpoint"];

        var config = new FssSdkConfig(rpId, apiKeyId, authType, apiSecret);
        if (!string.IsNullOrEmpty(endpoint))
        {
            config.Endpoint = endpoint;
        }

        return config;
    }

    private static Dictionary<string, string> LoadPropertiesFile()
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var searchDirs = new List<string>
        {
            Directory.GetCurrentDirectory(),
            AppDomain.CurrentDomain.BaseDirectory
        };

        foreach (var startDir in searchDirs)
        {
            var dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "environment.properties");
                if (File.Exists(candidate))
                {
                    try
                    {
                        foreach (var line in File.ReadAllLines(candidate))
                        {
                            var trimmed = line.Trim();
                            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                            var eq = trimmed.IndexOf('=');
                            if (eq > 0)
                            {
                                var key = trimmed.Substring(0, eq).Trim();
                                var val = trimmed.Substring(eq + 1).Trim();
                                dict[key] = val;
                            }
                        }
                        return dict;
                    }
                    catch
                    {
                        // Ignore file read error
                    }
                }
                dir = dir.Parent;
            }
        }

        return dict;
    }
}
