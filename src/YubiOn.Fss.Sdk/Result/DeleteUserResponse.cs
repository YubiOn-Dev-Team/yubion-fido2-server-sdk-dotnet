using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the deleteUser API call.
/// </summary>
public class DeleteUserResponse
{
    /// <summary>
    /// The deleted user data.
    /// </summary>
    [JsonPropertyName("user")]
    public UserDataWithCredentialCount? User { get; set; }

    /// <summary>
    /// The list of credentials that were deleted along with the user.
    /// </summary>
    [JsonPropertyName("credentials")]
    public List<YubiOn.Fss.Sdk.Data.Credential.CredentialData>? Credentials { get; set; }

    /// <summary>
    /// Signal options for notifying the browser/authenticator that credentials for this user are no longer accepted.
    /// </summary>
    [JsonPropertyName("signalAllAcceptedCredentialsOptions")]
    public SignalAllAcceptedCredentialsOptions? SignalAllAcceptedCredentialsOptions { get; set; }
}
