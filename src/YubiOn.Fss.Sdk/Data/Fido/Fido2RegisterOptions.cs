using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Provides additional options for the registration process.
/// </summary>
public class Fido2RegisterOptions
{
    [JsonPropertyName("createUserIfNotExists")]
    public bool? CreateUserIfNotExists { get; set; }

    [JsonPropertyName("updateUserIfExists")]
    public bool? UpdateUserIfExists { get; set; }

    [JsonPropertyName("credentialName")]
    public Fido2CredentialNameParameter? CredentialName { get; set; }

    [JsonPropertyName("credentialAttributes")]
    public Dictionary<string, object>? CredentialAttributes { get; set; }

    public Fido2RegisterOptions()
    {
    }

    public Fido2RegisterOptions(
        bool? createUserIfNotExists,
        bool? updateUserIfExists,
        Fido2CredentialNameParameter? credentialName,
        Dictionary<string, object>? credentialAttributes)
    {
        CreateUserIfNotExists = createUserIfNotExists;
        UpdateUserIfExists = updateUserIfExists;
        CredentialName = credentialName;
        CredentialAttributes = credentialAttributes;
    }
}
