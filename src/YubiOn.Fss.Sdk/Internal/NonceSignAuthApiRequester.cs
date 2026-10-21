using System.Text;
using YubiOn.Fss.Sdk.Config;
using YubiOn.Fss.Sdk.Result;

namespace YubiOn.Fss.Sdk.Internal;

/// <summary>
/// Handles nonce-based signature authentication for YubiOn FSS API requests.
/// </summary>
public class NonceSignAuthApiRequester : SignAuthApiRequesterBase
{
    private const string GetNoncePath = "getNonce";

    public NonceSignAuthApiRequester(FssSdkConfig config, bool denyUnknownResponseMember = false, bool allowBadSslForDebug = false, HttpClient? customHttpClient = null)
        : base(config, denyUnknownResponseMember, allowBadSslForDebug, customHttpClient)
    {
    }

    protected override async Task<SignInfo> GetSignInfoAsync(byte[] body, CancellationToken cancellationToken)
    {
        var cookieResponse = await RequestWithCookieResponseAsync<GetNonceResponse>(
            GetNoncePath,
            new Dictionary<string, object>(),
            withAuth: false,
            cookie: null,
            cancellationToken).ConfigureAwait(false);

        string? nonce = cookieResponse.Response?.Nonce;
        if (string.IsNullOrEmpty(nonce))
        {
            throw new FssApiException("Failed to retrieve nonce from the FSS server.");
        }

        byte[] signTargetPrefix = Encoding.UTF8.GetBytes(nonce);
        var additionalHeaders = new Dictionary<string, string>
        {
            ["X-Fss-Auth-Nonce"] = nonce
        };

        return new SignInfo(signTargetPrefix, additionalHeaders);
    }

    protected override SignInfo GetSignInfo(byte[] body)
    {
        var cookieResponse = RequestWithCookieResponse<GetNonceResponse>(
            GetNoncePath,
            new Dictionary<string, object>(),
            withAuth: false,
            cookie: null);

        string? nonce = cookieResponse.Response?.Nonce;
        if (string.IsNullOrEmpty(nonce))
        {
            throw new FssApiException("Failed to retrieve nonce from the FSS server.");
        }

        byte[] signTargetPrefix = Encoding.UTF8.GetBytes(nonce);
        var additionalHeaders = new Dictionary<string, string>
        {
            ["X-Fss-Auth-Nonce"] = nonce
        };

        return new SignInfo(signTargetPrefix, additionalHeaders);
    }
}
