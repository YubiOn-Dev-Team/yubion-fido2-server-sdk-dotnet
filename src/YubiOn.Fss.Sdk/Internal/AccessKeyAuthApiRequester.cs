using YubiOn.Fss.Sdk.Config;

namespace YubiOn.Fss.Sdk.Internal;

/// <summary>
/// Handles access key-based authentication for YubiOn FSS API requests.
/// </summary>
public class AccessKeyAuthApiRequester : FssApiRequesterBase
{
    public AccessKeyAuthApiRequester(FssSdkConfig config, bool denyUnknownResponseMember = false, bool allowBadSslForDebug = false, HttpClient? customHttpClient = null)
        : base(config, denyUnknownResponseMember, allowBadSslForDebug, customHttpClient)
    {
    }

    protected override Task<Dictionary<string, string>> GetAdditionalHeadersAsync(byte[] body, CancellationToken cancellationToken)
    {
        return Task.FromResult(GetAdditionalHeaders(body));
    }

    protected override Dictionary<string, string> GetAdditionalHeaders(byte[] body)
    {
        var headers = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(Config.RpId))
        {
            headers["X-Fss-Rp-Id"] = Config.RpId;
        }
        if (!string.IsNullOrEmpty(Config.ApiAuthId))
        {
            headers["X-Fss-Api-Auth-Id"] = Config.ApiAuthId;
        }
        if (!string.IsNullOrEmpty(Config.SecretKey))
        {
            headers["X-Fss-Auth-Access-Key"] = Config.SecretKey;
        }
        return headers;
    }
}
