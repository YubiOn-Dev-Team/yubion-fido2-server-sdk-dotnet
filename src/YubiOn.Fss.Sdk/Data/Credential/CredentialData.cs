using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Credential;

/// <summary>
/// Represents credential (passkey) data stored in the YubiOn FSS server.
/// </summary>
public class CredentialData
{
    /// <summary>
    /// The Relying Party ID the credential belongs to.
    /// </summary>
    [JsonPropertyName("rpId")]
    public string? RpId { get; set; }

    /// <summary>
    /// The user ID this credential is registered to.
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    /// <summary>
    /// The unique credential ID (Base64URL encoded).
    /// </summary>
    [JsonPropertyName("credentialId")]
    public string? CredentialId { get; set; }

    /// <summary>
    /// The friendly name given to this credential.
    /// </summary>
    [JsonPropertyName("credentialName")]
    public string? CredentialName { get; set; }

    /// <summary>
    /// Alias for CredentialName.
    /// </summary>
    [JsonIgnore]
    public string? Name
    {
        get => CredentialName;
        set => CredentialName = value;
    }

    /// <summary>
    /// Custom credential attributes dictionary.
    /// </summary>
    [JsonPropertyName("credentialAttributes")]
    public Dictionary<string, object>? CredentialAttributes { get; set; }

    /// <summary>
    /// Attestation statement format (e.g. "none", "packed").
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// Whether user presence was verified during registration.
    /// </summary>
    [JsonPropertyName("userPresence")]
    public bool UserPresence { get; set; }

    /// <summary>
    /// Whether user verification (e.g. biometric / PIN) was performed.
    /// </summary>
    [JsonPropertyName("userVerification")]
    public bool UserVerification { get; set; }

    /// <summary>
    /// Whether the credential is backup eligible (multi-device passkey).
    /// </summary>
    [JsonPropertyName("backupEligibility")]
    public bool BackupEligibility { get; set; }

    /// <summary>
    /// Whether the credential is currently backed up to the cloud.
    /// </summary>
    [JsonPropertyName("backupState")]
    public bool BackupState { get; set; }

    /// <summary>
    /// Indicates whether attested credential data is present.
    /// </summary>
    [JsonPropertyName("attestedCredentialData")]
    public bool AttestedCredentialData { get; set; }

    /// <summary>
    /// Indicates whether extension data is present.
    /// </summary>
    [JsonPropertyName("extensionData")]
    public bool ExtensionData { get; set; }

    /// <summary>
    /// Authenticator Attestation GUID (AAGUID).
    /// </summary>
    [JsonPropertyName("aaguid")]
    public string? Aaguid { get; set; }

    /// <summary>
    /// The model name of the authenticator identified by AAGUID.
    /// </summary>
    [JsonPropertyName("aaguidModelName")]
    public string? AaguidModelName { get; set; }

    /// <summary>
    /// COSE encoded public key (Base64URL).
    /// </summary>
    [JsonPropertyName("publicKey")]
    public string? PublicKey { get; set; }

    /// <summary>
    /// Raw transports string returned from the browser.
    /// </summary>
    [JsonPropertyName("transportsRaw")]
    public string? TransportsRaw { get; set; }

    /// <summary>
    /// Whether BLE transport is supported.
    /// </summary>
    [JsonPropertyName("transportsBle")]
    public bool? TransportsBle { get; set; }

    /// <summary>
    /// Whether Hybrid transport is supported.
    /// </summary>
    [JsonPropertyName("transportsHybrid")]
    public bool? TransportsHybrid { get; set; }

    /// <summary>
    /// Whether Internal platform transport is supported.
    /// </summary>
    [JsonPropertyName("transportsInternal")]
    public bool? TransportsInternal { get; set; }

    /// <summary>
    /// Whether NFC transport is supported.
    /// </summary>
    [JsonPropertyName("transportsNfc")]
    public bool? TransportsNfc { get; set; }

    /// <summary>
    /// Whether USB transport is supported.
    /// </summary>
    [JsonPropertyName("transportsUsb")]
    public bool? TransportsUsb { get; set; }

    /// <summary>
    /// Whether this is a discoverable (resident) credential.
    /// </summary>
    [JsonPropertyName("discoverableCredential")]
    public bool? DiscoverableCredential { get; set; }

    /// <summary>
    /// Whether enterprise attestation was used.
    /// </summary>
    [JsonPropertyName("enterpriseAttestation")]
    public bool EnterpriseAttestation { get; set; }

    /// <summary>
    /// Vendor ID of the authenticator.
    /// </summary>
    [JsonPropertyName("vendorId")]
    public string? VendorId { get; set; }

    /// <summary>
    /// Authenticator ID.
    /// </summary>
    [JsonPropertyName("authenticatorId")]
    public string? AuthenticatorId { get; set; }

    /// <summary>
    /// Raw attestation object (Base64URL).
    /// </summary>
    [JsonPropertyName("attestationObject")]
    public string? AttestationObject { get; set; }

    /// <summary>
    /// Authenticator attachment ("platform" or "cross-platform").
    /// </summary>
    [JsonPropertyName("authenticatorAttachment")]
    public string? AuthenticatorAttachment { get; set; }

    /// <summary>
    /// Credential type (typically "public-key").
    /// </summary>
    [JsonPropertyName("credentialType")]
    public string? CredentialType { get; set; }

    /// <summary>
    /// Client data JSON string.
    /// </summary>
    [JsonPropertyName("clientDataJson")]
    public string? ClientDataJson { get; set; }

    /// <summary>
    /// Raw client data JSON string.
    /// </summary>
    [JsonPropertyName("clientDataJsonRaw")]
    public string? ClientDataJsonRaw { get; set; }

    /// <summary>
    /// Timestamp of last successful authentication.
    /// </summary>
    [JsonPropertyName("lastAuthenticated")]
    public DateTimeOffset? LastAuthenticated { get; set; }

    /// <summary>
    /// Signature counter recorded during the last authentication.
    /// </summary>
    [JsonPropertyName("lastSignCounter")]
    public long? LastSignCounter { get; set; }

    /// <summary>
    /// Whether the credential is disabled.
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Timestamp when this credential was registered.
    /// </summary>
    [JsonPropertyName("registered")]
    public DateTimeOffset? Registered { get; set; }

    /// <summary>
    /// Timestamp when this credential was last updated.
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTimeOffset? Updated { get; set; }
}
