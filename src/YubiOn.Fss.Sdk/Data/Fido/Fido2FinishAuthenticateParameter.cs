using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Parameters for finishing the FIDO2 authentication process.
/// </summary>
public class Fido2FinishAuthenticateParameter
{
    [JsonPropertyName("requestResponse")]
    public RequestResponseClass? RequestResponse { get; set; }

    public class RequestResponseClass
    {
        [JsonPropertyName("attestationResponse")]
        public PublicKeyCredentialForAuthentication? AttestationResponse { get; set; }
    }
}
