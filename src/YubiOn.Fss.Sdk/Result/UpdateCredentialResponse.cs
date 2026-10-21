using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.Credential;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the updateCredential API call.
/// </summary>
public class UpdateCredentialResponse
{
    /// <summary>
    /// The updated credential data.
    /// </summary>
    [JsonPropertyName("credential")]
    public CredentialData? Credential { get; set; }

    /// <summary>
    /// The user associated with the credential.
    /// </summary>
    [JsonPropertyName("user")]
    public UserDataWithCredentialCount? User { get; set; }
}
