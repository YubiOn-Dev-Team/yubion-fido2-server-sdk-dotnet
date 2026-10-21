using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.Credential;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the finishAuthenticate API call.
/// </summary>
public class FinishAuthenticateResponse
{
    /// <summary>
    /// The credential used during authentication.
    /// </summary>
    [JsonPropertyName("credential")]
    public CredentialData? Credential { get; set; }

    /// <summary>
    /// The authenticated user data.
    /// </summary>
    [JsonPropertyName("user")]
    public UserDataWithCredentialCount? User { get; set; }

    /// <summary>
    /// Options for signaling all accepted credentials.
    /// </summary>
    [JsonPropertyName("signalAllAcceptedCredentialsOptions")]
    public SignalAllAcceptedCredentialsOptions? SignalAllAcceptedCredentialsOptions { get; set; }

    /// <summary>
    /// Options for signaling current user details.
    /// </summary>
    [JsonPropertyName("signalCurrentUserDetailsOptions")]
    public SignalCurrentUserDetailsOptions? SignalCurrentUserDetailsOptions { get; set; }
}
