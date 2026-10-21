using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Rp;

/// <summary>
/// Represents Relying Party (RP) configuration data.
/// </summary>
public class RpData
{
    /// <summary>
    /// The Relying Party identifier (e.g. domain name).
    /// </summary>
    [JsonPropertyName("rpId")]
    public string? RpId { get; set; }

    /// <summary>
    /// The friendly name of the Relying Party.
    /// </summary>
    [JsonPropertyName("rpName")]
    public string? RpName { get; set; }
}
