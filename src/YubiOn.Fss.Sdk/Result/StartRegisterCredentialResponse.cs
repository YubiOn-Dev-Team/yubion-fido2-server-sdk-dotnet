using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the startRegisterCredential API call.
/// </summary>
public class StartRegisterCredentialResponse
{
    /// <summary>
    /// WebAuthn creation options to be passed to navigator.credentials.create().
    /// </summary>
    [JsonPropertyName("creationOptions")]
    public PublicKeyCredentialCreationOptionsJson? CreationOptions { get; set; }

    /// <summary>
    /// Target user for whom the credential is being registered.
    /// </summary>
    [JsonPropertyName("user")]
    public UserDataWithCredentialCount? User { get; set; }
}
