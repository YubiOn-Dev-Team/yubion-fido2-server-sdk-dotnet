using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Represents PublicKeyCredentialCreationOptions in JSON format.
/// </summary>
public class PublicKeyCredentialCreationOptionsJson
{
    [JsonPropertyName("attestation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Attestation { get; set; }

    [JsonPropertyName("authenticatorSelection")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AuthenticatorSelectionCriteria? AuthenticatorSelection { get; set; }

    [JsonPropertyName("challenge")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Challenge { get; set; }

    [JsonPropertyName("excludeCredentials")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PublicKeyCredentialDescriptorJson[]? ExcludeCredentials { get; set; }

    [JsonPropertyName("extensions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Extensions { get; set; }

    [JsonPropertyName("hints")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Hints { get; set; }

    [JsonPropertyName("pubKeyCredParams")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PublicKeyCredentialParameters[]? PubKeyCredParams { get; set; }

    [JsonPropertyName("rp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PublicKeyCredentialRpEntity? Rp { get; set; }

    [JsonPropertyName("timeout")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Timeout { get; set; }

    [JsonPropertyName("user")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PublicKeyCredentialUserEntityJson? User { get; set; }
}
