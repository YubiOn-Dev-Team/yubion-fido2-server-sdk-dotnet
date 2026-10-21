using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.Credential;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the finishRegisterCredential API call.
/// </summary>
public class FinishRegisterCredentialResponse
{
    /// <summary>
    /// The newly registered credential data.
    /// </summary>
    [JsonPropertyName("credential")]
    public CredentialData? Credential { get; set; }

    /// <summary>
    /// The user associated with the new credential, with updated credential count.
    /// </summary>
    [JsonPropertyName("user")]
    public UserDataWithCredentialCount? User { get; set; }
}
