using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the updateUser API call.
/// </summary>
public class UpdateUserResponse
{
    /// <summary>
    /// The updated user data.
    /// </summary>
    [JsonPropertyName("user")]
    public UserDataWithCredentialCount? User { get; set; }

    /// <summary>
    /// Signal options for updating current user details in the browser/authenticator.
    /// </summary>
    [JsonPropertyName("signalCurrentUserDetailsOptions")]
    public SignalCurrentUserDetailsOptions? SignalCurrentUserDetailsOptions { get; set; }
}
