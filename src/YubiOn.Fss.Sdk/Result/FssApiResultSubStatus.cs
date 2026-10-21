using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Represents the detailed sub-status of an API error.
/// </summary>
public class FssApiResultSubStatus
{
    /// <summary>
    /// Detailed error code classification (Fido2ErrorStatus).
    /// </summary>
    [JsonPropertyName("errorCode")]
    public Fido2ErrorStatus? ErrorCode { get; set; }

    /// <summary>
    /// Human-readable error message explaining the cause.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional context or metadata concerning the error.
    /// </summary>
    [JsonPropertyName("info")]
    public object? Info { get; set; }

    /// <summary>
    /// Initializes a new empty instance of <see cref="FssApiResultSubStatus"/>.
    /// </summary>
    public FssApiResultSubStatus()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FssApiResultSubStatus"/> with error details.
    /// </summary>
    /// <param name="errorCode">The detailed error status code.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="info">Optional additional error info.</param>
    public FssApiResultSubStatus(Fido2ErrorStatus? errorCode, string? errorMessage, object? info = null)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        Info = info;
    }
}
