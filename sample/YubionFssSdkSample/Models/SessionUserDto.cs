using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Sample.Models;

/// <summary>
/// Session user information.
/// </summary>
public class SessionUserDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
}
