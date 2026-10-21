using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Represents the authenticator selection criteria.
/// </summary>
public class AuthenticatorSelectionCriteria
{
    [JsonPropertyName("authenticatorAttachment")]
    public AuthenticatorAttachment? AuthenticatorAttachment { get; set; }

    [JsonPropertyName("residentKey")]
    public ResidentKeyRequirement? ResidentKey { get; set; }

    [JsonPropertyName("requireResidentKey")]
    public bool? RequireResidentKey { get; set; }

    [JsonPropertyName("userVerification")]
    public UserVerificationRequirement? UserVerification { get; set; }
}
