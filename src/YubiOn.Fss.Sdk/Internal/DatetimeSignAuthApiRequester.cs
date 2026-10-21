using System.Text;
using YubiOn.Fss.Sdk.Config;

namespace YubiOn.Fss.Sdk.Internal;

/// <summary>
/// Handles timestamp-based signature authentication for YubiOn FSS API requests.
/// </summary>
public class DatetimeSignAuthApiRequester : SignAuthApiRequesterBase
{
    public DatetimeSignAuthApiRequester(FssSdkConfig config, bool denyUnknownResponseMember = false, bool allowBadSslForDebug = false, HttpClient? customHttpClient = null)
        : base(config, denyUnknownResponseMember, allowBadSslForDebug, customHttpClient)
    {
    }

    protected override Task<SignInfo> GetSignInfoAsync(byte[] body, CancellationToken cancellationToken)
    {
        return Task.FromResult(GetSignInfo(body));
    }

    protected override SignInfo GetSignInfo(byte[] body)
    {
        string now = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
        byte[] signTargetPrefix = Encoding.UTF8.GetBytes(now);
        var additionalHeaders = new Dictionary<string, string>
        {
            ["X-Fss-Auth-Request-Time"] = now
        };

        return new SignInfo(signTargetPrefix, additionalHeaders);
    }
}
