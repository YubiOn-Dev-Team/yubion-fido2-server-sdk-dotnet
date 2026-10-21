using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Base options for the authentication request.
/// </summary>
public class Fido2RequestOptionsBase
{
    [JsonPropertyName("timeout")]
    public int? Timeout { get; set; }

    [JsonPropertyName("hints")]
    public List<string>? Hints { get; set; }

    [JsonPropertyName("userVerification")]
    public UserVerificationRequirement? UserVerification { get; set; }

    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; set; }
}
