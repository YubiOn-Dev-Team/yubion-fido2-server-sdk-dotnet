using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the getUsers API call.
/// </summary>
public class GetUsersResponse
{
    /// <summary>
    /// List of retrieved users.
    /// </summary>
    [JsonPropertyName("users")]
    public List<UserDataWithCredentialCount>? Users { get; set; }
}
