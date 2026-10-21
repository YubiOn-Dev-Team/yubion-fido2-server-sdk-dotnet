using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Options for signaling all accepted credentials.
/// </summary>
public class SignalAllAcceptedCredentialsOptions
{
    /// <summary>
    /// The Relying Party identifier.
    /// </summary>
    [JsonPropertyName("rpId")]
    public string? RpId { get; set; }

    /// <summary>
    /// The user ID.
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    /// <summary>
    /// List of all accepted credential IDs.
    /// </summary>
    [JsonPropertyName("allAcceptedCredentialIds")]
    public List<string>? AllAcceptedCredentialIds { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="SignalAllAcceptedCredentialsOptions"/>.
    /// </summary>
    public SignalAllAcceptedCredentialsOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignalAllAcceptedCredentialsOptions"/> with parameters.
    /// </summary>
    public SignalAllAcceptedCredentialsOptions(string? rpId, string? userId, List<string>? allAcceptedCredentialIds)
    {
        RpId = rpId;
        UserId = userId;
        AllAcceptedCredentialIds = allAcceptedCredentialIds;
    }
}
