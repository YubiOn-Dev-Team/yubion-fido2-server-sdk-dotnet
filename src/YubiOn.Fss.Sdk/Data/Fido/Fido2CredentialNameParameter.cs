using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Specifies credential name options when registering a credential.
/// </summary>
public class Fido2CredentialNameParameter
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nameIfModelNameExists")]
    public string? NameIfModelNameExists { get; set; }

    [JsonPropertyName("nameIfEnterpriseAttestationExists")]
    public string? NameIfEnterpriseAttestationExists { get; set; }

    public Fido2CredentialNameParameter()
    {
    }

    public Fido2CredentialNameParameter(string? name)
    {
        Name = name;
    }

    public Fido2CredentialNameParameter(string? name, string? nameIfModelNameExists, string? nameIfEnterpriseAttestationExists)
    {
        Name = name;
        NameIfModelNameExists = nameIfModelNameExists;
        NameIfEnterpriseAttestationExists = nameIfEnterpriseAttestationExists;
    }
}
