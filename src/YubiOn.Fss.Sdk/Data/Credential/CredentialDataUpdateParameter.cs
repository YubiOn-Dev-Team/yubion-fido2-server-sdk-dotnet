using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Credential;

/// <summary>
/// Parameters for updating an existing credential.
/// </summary>
public class CredentialDataUpdateParameter
{
    /// <summary>
    /// The user ID of the credential owner.
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    /// <summary>
    /// The unique credential ID to update.
    /// </summary>
    [JsonPropertyName("credentialId")]
    public string? CredentialId { get; set; }

    /// <summary>
    /// The new friendly name for the credential.
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
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<string, object>? CredentialAttributes { get; set; }

    /// <summary>
    /// Whether the credential is disabled.
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// The updated timestamp for optimistic concurrency check (when withUpdatedCheck is true).
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    /// Initializes a new empty instance of <see cref="CredentialDataUpdateParameter"/>.
    /// </summary>
    public CredentialDataUpdateParameter()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CredentialDataUpdateParameter"/> with identifiers.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="credentialId">The credential ID.</param>
    public CredentialDataUpdateParameter(string? userId, string? credentialId)
    {
        UserId = userId;
        CredentialId = credentialId;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CredentialDataUpdateParameter"/> with details.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="credentialName">The new credential name.</param>
    /// <param name="credentialAttributes">Custom attributes.</param>
    /// <param name="disabled">Disabled state.</param>
    public CredentialDataUpdateParameter(string? userId, string? credentialId, string? credentialName, Dictionary<string, object>? credentialAttributes, bool disabled)
        : this(userId, credentialId)
    {
        CredentialName = credentialName;
        CredentialAttributes = credentialAttributes;
        Disabled = disabled;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CredentialDataUpdateParameter"/> including updated timestamp for conflict checking.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="credentialName">The new credential name.</param>
    /// <param name="credentialAttributes">Custom attributes.</param>
    /// <param name="disabled">Disabled state.</param>
    /// <param name="updated">Timestamp for conflict check.</param>
    public CredentialDataUpdateParameter(string? userId, string? credentialId, string? credentialName, Dictionary<string, object>? credentialAttributes, bool disabled, DateTimeOffset? updated)
        : this(userId, credentialId, credentialName, credentialAttributes, disabled)
    {
        Updated = updated;
    }

    /// <summary>
    /// Copy constructor from an existing <see cref="CredentialDataUpdateParameter"/>.
    /// </summary>
    /// <param name="basePrm">Source update parameter.</param>
    public CredentialDataUpdateParameter(CredentialDataUpdateParameter basePrm)
    {
        if (basePrm == null) throw new ArgumentNullException(nameof(basePrm));
        UserId = basePrm.UserId;
        CredentialId = basePrm.CredentialId;
        CredentialName = basePrm.CredentialName;
        CredentialAttributes = basePrm.CredentialAttributes != null ? new Dictionary<string, object>(basePrm.CredentialAttributes) : null;
        Disabled = basePrm.Disabled;
        Updated = basePrm.Updated;
    }
}
