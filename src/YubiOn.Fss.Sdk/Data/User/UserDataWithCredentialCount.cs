using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.User;

/// <summary>
/// Represents user data along with the count of registered credentials.
/// </summary>
public class UserDataWithCredentialCount : UserData
{
    /// <summary>
    /// The number of enabled credentials associated with this user.
    /// </summary>
    [JsonPropertyName("enabledCredentialCount")]
    public int EnabledCredentialCount { get; set; }

    /// <summary>
    /// The total number of credentials associated with this user (including disabled).
    /// </summary>
    [JsonPropertyName("credentialCount")]
    public int CredentialCount { get; set; }
}
