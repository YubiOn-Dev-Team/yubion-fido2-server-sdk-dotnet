using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Options for signaling current user details.
/// </summary>
public class SignalCurrentUserDetailsOptions
{
    /// <summary>
    /// The Relying Party identifier.
    /// </summary>
    [JsonPropertyName("rpId")]
    public string? RpId { get; set; }

    /// <summary>
    /// The user ID.
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    /// <summary>
    /// The username.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The display name.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="SignalCurrentUserDetailsOptions"/>.
    /// </summary>
    public SignalCurrentUserDetailsOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignalCurrentUserDetailsOptions"/> with parameters.
    /// </summary>
    public SignalCurrentUserDetailsOptions(string? rpId, string? userId, string? name)
    {
        RpId = rpId;
        UserId = userId;
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignalCurrentUserDetailsOptions"/> with all parameters.
    /// </summary>
    public SignalCurrentUserDetailsOptions(string? rpId, string? userId, string? name, string? displayName)
    {
        RpId = rpId;
        UserId = userId;
        Name = name;
        DisplayName = displayName;
    }
}
