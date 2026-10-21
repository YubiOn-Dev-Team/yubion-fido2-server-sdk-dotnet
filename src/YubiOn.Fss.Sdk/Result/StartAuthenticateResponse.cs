using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the startAuthenticate API call.
/// </summary>
public class StartAuthenticateResponse
{
    /// <summary>
    /// WebAuthn authentication request options to be passed to navigator.credentials.get().
    /// </summary>
    [JsonPropertyName("requestOptions")]
    public PublicKeyCredentialRequestOptionsJson? RequestOptions { get; set; }

    /// <summary>
    /// Target user being authenticated, if known.
    /// </summary>
    [JsonPropertyName("user")]
    public UserDataWithCredentialCount? User { get; set; }
}
