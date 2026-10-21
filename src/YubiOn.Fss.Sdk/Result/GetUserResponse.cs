using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.Credential;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the getUser API call.
/// </summary>
public class GetUserResponse
{
    /// <summary>
    /// The user details.
    /// </summary>
    [JsonPropertyName("user")]
    public UserDataWithCredentialCount? User { get; set; }

    /// <summary>
    /// List of credentials registered to this user.
    /// </summary>
    [JsonPropertyName("credentials")]
    public List<CredentialData>? Credentials { get; set; }

    /// <summary>
    /// Signal options for updating passkey details in the browser/authenticator.
    /// </summary>
    [JsonPropertyName("signalCurrentUserDetailsOptions")]
    public SignalCurrentUserDetailsOptions? SignalCurrentUserDetailsOptions { get; set; }
}
