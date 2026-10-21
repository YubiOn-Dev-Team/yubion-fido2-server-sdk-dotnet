using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.User;

/// <summary>
/// Represents user data registered in the YubiOn FSS server.
/// </summary>
public class UserData
{
    /// <summary>
    /// The Relying Party ID the user belongs to.
    /// </summary>
    [JsonPropertyName("rpId")]
    public string? RpId { get; set; }

    /// <summary>
    /// The unique user identifier.
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    /// <summary>
    /// The unique login username (e.g. email address).
    /// </summary>
    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    /// <summary>
    /// The friendly display name of the user.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Custom user attributes dictionary.
    /// </summary>
    [JsonPropertyName("userAttributes")]
    public Dictionary<string, object>? UserAttributes { get; set; }

    /// <summary>
    /// Indicates whether the user account is disabled.
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Timestamp when the user was registered.
    /// </summary>
    [JsonPropertyName("registered")]
    public DateTimeOffset? Registered { get; set; }

    /// <summary>
    /// Timestamp when the user was last updated.
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTimeOffset? Updated { get; set; }
}
