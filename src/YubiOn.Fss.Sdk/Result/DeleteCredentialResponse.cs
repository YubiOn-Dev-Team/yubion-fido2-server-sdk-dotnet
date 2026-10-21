using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.Credential;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the deleteCredential API call.
/// </summary>
public class DeleteCredentialResponse
{
    /// <summary>
    /// The deleted credential data.
    /// </summary>
    [JsonPropertyName("credential")]
    public CredentialData? Credential { get; set; }

    /// <summary>
    /// The user associated with the deleted credential, updated with remaining credential count.
    /// </summary>
    [JsonPropertyName("user")]
    public UserDataWithCredentialCount? User { get; set; }

    /// <summary>
    /// Signal options for notifying the browser/authenticator that the credential is no longer known.
    /// </summary>
    [JsonPropertyName("signalUnknownCredentialOptions")]
    public SignalUnknownCredentialOptions? SignalUnknownCredentialOptions { get; set; }
}
