using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Represents a public key credential for authentication.
/// </summary>
public class PublicKeyCredentialForAuthentication
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("rawId")]
    public string? RawId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; } = "public-key";

    [JsonPropertyName("response")]
    public AuthenticatorAssertionResponse? Response { get; set; }
}
