using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Base creation options for the registration process.
/// </summary>
public class Fido2CreationOptionsBase
{
    [JsonPropertyName("authenticatorSelection")]
    public AuthenticatorSelectionCriteria? AuthenticatorSelection { get; set; }

    [JsonPropertyName("timeout")]
    public int? Timeout { get; set; }

    [JsonPropertyName("hints")]
    public List<string>? Hints { get; set; }

    [JsonPropertyName("attestation")]
    public AttestationConveyancePreference? Attestation { get; set; }

    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; set; }
}
