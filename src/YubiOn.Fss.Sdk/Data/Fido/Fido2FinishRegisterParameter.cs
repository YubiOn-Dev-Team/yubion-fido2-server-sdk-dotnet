using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Parameters for finishing the FIDO2 registration process.
/// </summary>
public class Fido2FinishRegisterParameter
{
    [JsonPropertyName("createResponse")]
    public CreateResponseClass? CreateResponse { get; set; }

    [JsonPropertyName("options")]
    public OptionsClass? Options { get; set; }

    public class CreateResponseClass
    {
        [JsonPropertyName("attestationResponse")]
        public PublicKeyCredential? AttestationResponse { get; set; }

        [JsonPropertyName("transports")]
        public string[]? Transports { get; set; }
    }

    public class OptionsClass
    {
        [JsonPropertyName("credentialName")]
        public Fido2CredentialNameParameter? CredentialName { get; set; }
    }
}
