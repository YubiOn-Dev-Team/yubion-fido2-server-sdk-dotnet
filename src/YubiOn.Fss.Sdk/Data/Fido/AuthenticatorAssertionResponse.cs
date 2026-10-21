using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Represents the response from an authenticator for an authentication ceremony.
/// </summary>
public class AuthenticatorAssertionResponse : AuthenticatorResponse
{
    [JsonPropertyName("authenticatorData")]
    public string? AuthenticatorData { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    [JsonPropertyName("userHandle")]
    public string? UserHandle { get; set; }
}
