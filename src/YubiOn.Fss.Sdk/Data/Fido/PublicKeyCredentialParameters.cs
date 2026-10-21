using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Parameters for public key credentials (e.g. COSE algorithm and type).
/// </summary>
public class PublicKeyCredentialParameters
{
    [JsonPropertyName("alg")]
    public int Alg { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    public PublicKeyCredentialParameters()
    {
    }

    public PublicKeyCredentialParameters(int alg, string type = "public-key")
    {
        Alg = alg;
        Type = type;
    }
}
