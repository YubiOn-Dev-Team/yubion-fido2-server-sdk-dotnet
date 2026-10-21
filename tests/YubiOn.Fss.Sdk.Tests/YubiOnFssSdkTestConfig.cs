using YubiOn.Fss.Sdk.Config;

namespace YubiOn.Fss.Sdk.Tests;

public static class YubiOnFssSdkTestConfig
{
    public const bool AllowBadSsl = true;
    public const bool DenyUnknownResponseMember = true;

    public static string? RpId { get; set; }
    public static string? Endpoint { get; set; }

    // Nonce Auth Constants
    public static string? NonceAuthId { get; set; }
    public static string? NonceSecretKeyOk { get; set; }
    public static string? NonceSecretKeyNg { get; set; }

    // Datetime Auth Constants
    public static string? DatetimeAuthId { get; set; }
    public static string? DatetimeSecretKeyOk { get; set; }
    public static string? DatetimeSecretKeyNg { get; set; }

    // AccessKey Auth Constants
    public static string? AccessKeyAuthId { get; set; }
    public static string? AccessKeySecretKeyOk { get; set; }
    public static string? AccessKeySecretKeyNg { get; set; }

    // License Test Constants
    public static string? RpIdForLicenseTest { get; set; }
    public static string? ApiAuthIdForLicenseTest { get; set; }
    public static string? ApiSecretKeyForLicenseTest { get; set; }

    static YubiOnFssSdkTestConfig()
    {
        Init();
    }

    private static bool _hasPropertiesFile = false;

    public static void Init()
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 1. Try reading environment.properties from current dir or any parent directory
        var searchDirs = new List<string>
        {
            Directory.GetCurrentDirectory(),
            AppDomain.CurrentDomain.BaseDirectory
        };

        string? foundPath = null;
        foreach (var startDir in searchDirs)
        {
            var dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "environment.properties");
                if (File.Exists(candidate))
                {
                    foundPath = candidate;
                    break;
                }
                dir = dir.Parent;
            }
            if (foundPath != null) break;
        }

        if (foundPath != null && File.Exists(foundPath))
        {
            _hasPropertiesFile = true;
            foreach (var line in File.ReadAllLines(foundPath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#') || trimmed.StartsWith(';'))
                    continue;

                var eqIdx = trimmed.IndexOf('=');
                if (eqIdx > 0)
                {
                    var key = trimmed.Substring(0, eqIdx).Trim();
                    var val = trimmed.Substring(eqIdx + 1).Trim();
                    dict[key] = val;
                }
            }
        }

        // Helper to get from Environment or properties file
        string? GetVal(string key, string? fallback = null)
        {
            var env = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(env)) return env;
            if (dict.TryGetValue(key, out var propVal) && !string.IsNullOrEmpty(propVal)) return propVal;
            return fallback;
        }

        RpId = GetVal("FSS_TEST_RP_ID", "test.example.com");
        Endpoint = GetVal("FSS_TEST_ENDPOINT", FssSdkConfig.DefaultEndpoint);

        NonceAuthId = GetVal("FSS_TEST_NONCE_API_AUTH_ID", "test-nonce-auth-id");
        NonceSecretKeyOk = GetVal("FSS_TEST_NONCE_CORRECT_API_SECRET_KEY", "MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAChRANCAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==");
        NonceSecretKeyNg = GetVal("FSS_TEST_NONCE_INCORRECT_API_SECRET_KEY", "MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBChRANCAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==");

        DatetimeAuthId = GetVal("FSS_TEST_DATETIME_API_AUTH_ID", "test-datetime-auth-id");
        DatetimeSecretKeyOk = GetVal("FSS_TEST_DATETIME_CORRECT_API_SECRET_KEY", NonceSecretKeyOk);
        DatetimeSecretKeyNg = GetVal("FSS_TEST_DATETIME_INCORRECT_API_SECRET_KEY", NonceSecretKeyNg);

        AccessKeyAuthId = GetVal("FSS_TEST_ACCESSKEY_API_AUTH_ID", "test-accesskey-auth-id");
        AccessKeySecretKeyOk = GetVal("FSS_TEST_ACCESSKEY_CORRECT_API_SECRET_KEY", "test-correct-secret-key");
        AccessKeySecretKeyNg = GetVal("FSS_TEST_ACCESSKEY_INCORRECT_API_SECRET_KEY", "test-incorrect-secret-key");

        RpIdForLicenseTest = GetVal("FSS_TEST_RP_ID_FOR_LICENSE_TEST", RpId);
        ApiAuthIdForLicenseTest = GetVal("FSS_TEST_API_AUTH_ID_FOR_LICENSE_TEST", NonceAuthId);
        ApiSecretKeyForLicenseTest = GetVal("FSS_TEST_API_SECRET_KEY_FOR_LICENSE_TEST", NonceSecretKeyOk);
    }

    public static FssSdkConfig CreateNonceConfig(bool correctKey)
    {
        string? secretKey = correctKey ? NonceSecretKeyOk : NonceSecretKeyNg;
        var config = new FssSdkConfig(RpId!, NonceAuthId!, FssApiAuthType.NonceSignAuth, secretKey!);
        if (!string.IsNullOrEmpty(Endpoint))
        {
            config.Endpoint = Endpoint;
        }
        return config;
    }

    public static FssSdkConfig CreateDatetimeConfig(bool correctKey)
    {
        string? secretKey = correctKey ? DatetimeSecretKeyOk : DatetimeSecretKeyNg;
        var config = new FssSdkConfig(RpId!, DatetimeAuthId!, FssApiAuthType.DatetimeSignAuth, secretKey!);
        if (!string.IsNullOrEmpty(Endpoint))
        {
            config.Endpoint = Endpoint;
        }
        return config;
    }

    public static FssSdkConfig CreateAccessKeyConfig(bool correctKey)
    {
        string? secretKey = correctKey ? AccessKeySecretKeyOk : AccessKeySecretKeyNg;
        var config = new FssSdkConfig(RpId!, AccessKeyAuthId!, FssApiAuthType.AccessKeyAuth, secretKey!);
        if (!string.IsNullOrEmpty(Endpoint))
        {
            config.Endpoint = Endpoint;
        }
        return config;
    }

    public static bool HasLiveTestCredentials()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FSS_TEST_RP_ID")) ||
               _hasPropertiesFile ||
               File.Exists("environment.properties");
    }
}
