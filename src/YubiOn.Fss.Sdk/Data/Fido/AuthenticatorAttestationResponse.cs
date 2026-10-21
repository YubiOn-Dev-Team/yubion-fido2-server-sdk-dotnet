using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Represents the response from an authenticator for a registration ceremony.
/// </summary>
public class AuthenticatorAttestationResponse : AuthenticatorResponse
{
    [JsonPropertyName("attestationObject")]
    public string? AttestationObject { get; set; }

    [JsonPropertyName("transports")]
    public List<AuthenticatorTransport>? Transports { get; set; }
}
