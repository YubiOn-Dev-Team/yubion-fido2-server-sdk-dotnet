using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Sample.Models;

/// <summary>
/// Request payload for starting user registration.
/// </summary>
public class RegisterStartRequest
{
    /// <summary>
    /// User name / identifier.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Display name of the user.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}
