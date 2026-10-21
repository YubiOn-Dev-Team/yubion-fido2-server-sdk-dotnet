using YubiOn.Fss.Sdk.Config;

namespace YubiOn.Fss.Sdk.Internal;

/// <summary>
/// Factory for creating IFssApiRequester instances based on authentication type.
/// </summary>
public static class FssApiRequesterFactory
{
    public static IFssApiRequester Create(FssSdkConfig config)
    {
        return Create(config, false, false, null);
    }

    public static IFssApiRequester Create(FssSdkConfig config, bool denyUnknownResponseMember, bool allowBadSslForDebug)
    {
        return Create(config, denyUnknownResponseMember, allowBadSslForDebug, null);
    }

    public static IFssApiRequester Create(FssSdkConfig config, bool denyUnknownResponseMember, bool allowBadSslForDebug, HttpClient? customHttpClient)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        return config.ApiAuthType switch
        {
            FssApiAuthType.NonceSignAuth => new NonceSignAuthApiRequester(config, denyUnknownResponseMember, allowBadSslForDebug, customHttpClient),
            FssApiAuthType.DatetimeSignAuth => new DatetimeSignAuthApiRequester(config, denyUnknownResponseMember, allowBadSslForDebug, customHttpClient),
            FssApiAuthType.AccessKeyAuth => new AccessKeyAuthApiRequester(config, denyUnknownResponseMember, allowBadSslForDebug, customHttpClient),
            _ => throw new ArgumentException($"Unsupported API authentication type: {config.ApiAuthType}", nameof(config))
        };
    }
}
