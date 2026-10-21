using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the registerUser API call.
/// </summary>
public class RegisterUserResponse
{
    /// <summary>
    /// The newly registered user data.
    /// </summary>
    [JsonPropertyName("user")]
    public UserDataWithCredentialCount? User { get; set; }
}
