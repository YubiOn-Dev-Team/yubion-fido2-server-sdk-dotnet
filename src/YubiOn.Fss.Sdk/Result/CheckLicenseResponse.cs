using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Response returned from the checkLicense API.
/// </summary>
public class CheckLicenseResponse
{
    /// <summary>
    /// License type or status string.
    /// </summary>
    [JsonPropertyName("license")]
    public string? License { get; set; }

    /// <summary>
    /// Expiration timestamp of the license.
    /// </summary>
    [JsonPropertyName("expire")]
    public DateTimeOffset? Expire { get; set; }
}
