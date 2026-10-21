using System.Security.Cryptography;
using YubiOn.Fss.Sdk.Config;

namespace YubiOn.Fss.Sdk.Internal;

/// <summary>
/// Base class for signature-based API requesters (Nonce and Datetime).
/// Generates ECDSA P-256 (secp256r1) SHA-256 signatures in IEEE P1363 (R||S) format.
/// </summary>
public abstract class SignAuthApiRequesterBase : FssApiRequesterBase
{
    private readonly ECDsa _ecdsa;

    protected SignAuthApiRequesterBase(FssSdkConfig config, bool denyUnknownResponseMember = false, bool allowBadSslForDebug = false, HttpClient? customHttpClient = null)
        : base(config, denyUnknownResponseMember, allowBadSslForDebug, customHttpClient)
    {
        if (string.IsNullOrEmpty(config.SecretKey))
        {
            throw new ArgumentException("SecretKey must be provided for signature authentication.", nameof(config));
        }

        byte[] privateKeyBytes = Base64UrlDecode(config.SecretKey);
        _ecdsa = ECDsa.Create();
        try
        {
            _ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
        }
        catch (Exception ex)
        {
            _ecdsa.Dispose();
            throw new ArgumentException("Failed to load ECDSA private key from SecretKey (expected Base64URL-encoded PKCS#8 DER).", nameof(config), ex);
        }
    }

    protected override async Task<Dictionary<string, string>> GetAdditionalHeadersAsync(byte[] body, CancellationToken cancellationToken)
    {
        var signInfo = await GetSignInfoAsync(body, cancellationToken).ConfigureAwait(false);
        return BuildHeaders(signInfo, body);
    }

    protected override Dictionary<string, string> GetAdditionalHeaders(byte[] body)
    {
        var signInfo = GetSignInfo(body);
        return BuildHeaders(signInfo, body);
    }

    private Dictionary<string, string> BuildHeaders(SignInfo signInfo, byte[] body)
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

        foreach (var header in signInfo.AdditionalHeaders)
        {
            headers[header.Key] = header.Value;
        }

        byte[] bodyHash = SHA256.HashData(body);
        string bodyHashBase64Url = Base64UrlEncode(bodyHash);
        headers["X-Fss-Auth-Body-Hash"] = bodyHashBase64Url;

        byte[] signTargetPrefix = signInfo.SignTargetPrefix;
        byte[] signTarget = new byte[signTargetPrefix.Length + bodyHash.Length];
        Buffer.BlockCopy(signTargetPrefix, 0, signTarget, 0, signTargetPrefix.Length);
        Buffer.BlockCopy(bodyHash, 0, signTarget, signTargetPrefix.Length, bodyHash.Length);

        byte[] signatureBytes = _ecdsa.SignData(signTarget, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        string signatureBase64Url = Base64UrlEncode(signatureBytes);
        headers["X-Fss-Auth-Signature"] = signatureBase64Url;

        return headers;
    }

    protected abstract Task<SignInfo> GetSignInfoAsync(byte[] body, CancellationToken cancellationToken);
    protected abstract SignInfo GetSignInfo(byte[] body);

    public static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static byte[] Base64UrlDecode(string base64Url)
    {
        string base64 = base64Url.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    public override void Dispose()
    {
        _ecdsa.Dispose();
        base.Dispose();
    }

    protected class SignInfo
    {
        public byte[] SignTargetPrefix { get; }
        public Dictionary<string, string> AdditionalHeaders { get; }

        public SignInfo(byte[] signTargetPrefix, Dictionary<string, string> additionalHeaders)
        {
            SignTargetPrefix = signTargetPrefix;
            AdditionalHeaders = additionalHeaders;
        }
    }
}
