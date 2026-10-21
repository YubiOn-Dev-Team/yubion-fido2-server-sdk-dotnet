using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Internal;

/// <summary>
/// Response payload for the getNonce API call.
/// </summary>
public class GetNonceResponse
{
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }
}
