using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Options for signaling an unknown credential.
/// </summary>
public class SignalUnknownCredentialOptions
{
    [JsonPropertyName("rpId")]
    public string? RpId { get; set; }

    [JsonPropertyName("credentialId")]
    public string? CredentialId { get; set; }

    public SignalUnknownCredentialOptions()
    {
    }

    public SignalUnknownCredentialOptions(string? rpId, string? credentialId)
    {
        RpId = rpId;
        CredentialId = credentialId;
    }
}
