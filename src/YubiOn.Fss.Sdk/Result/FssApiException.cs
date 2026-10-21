using System.Text.Json;
using YubiOn.Fss.Sdk.Result;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Custom exception representing YubiOn FSS API errors or network failures.
/// </summary>
public class FssApiException : Exception
{
    /// <summary>
    /// The high-level status code returned by the FSS server.
    /// </summary>
    public FssApiResultStatus AppStatus { get; }

    /// <summary>
    /// Raw JSON element of the error sub-status, if returned.
    /// </summary>
    public JsonElement? AppSubStatus { get; }

    /// <summary>
    /// The raw response body string received from the server.
    /// </summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// The HTTP status code of the response (e.g. 200, 400, 500).
    /// </summary>
    public int HttpStatusCode { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="FssApiException"/> with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public FssApiException(string message)
        : base(message)
    {
        AppStatus = FssApiResultStatus.UNEXPECTED_ERROR;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FssApiException"/> with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The underlying inner exception.</param>
    public FssApiException(string message, Exception innerException)
        : base(message, innerException)
    {
        AppStatus = FssApiResultStatus.UNEXPECTED_ERROR;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FssApiException"/> with full details.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="appStatus">The FSS result status.</param>
    /// <param name="appSubStatus">Optional sub-status JSON.</param>
    /// <param name="innerException">Optional inner exception.</param>
    /// <param name="httpStatusCode">HTTP status code.</param>
    /// <param name="responseBody">Raw response body.</param>
    public FssApiException(string message, FssApiResultStatus appStatus, JsonElement? appSubStatus = null, Exception? innerException = null, int httpStatusCode = 0, string? responseBody = null)
        : base(message, innerException)
    {
        AppStatus = appStatus;
        AppSubStatus = appSubStatus;
        HttpStatusCode = httpStatusCode;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Java-compatible getter for AppStatus.
    /// </summary>
    public FssApiResultStatus GetAppStatus() => AppStatus;

    /// <summary>
    /// Java-compatible getter for AppSubStatus.
    /// </summary>
    public JsonElement? GetAppSubStatus() => AppSubStatus;

    /// <summary>
    /// Attempts to parse AppSubStatus as <see cref="FssApiResultSubStatus"/>.
    /// </summary>
    public FssApiResultSubStatus? GetParsedSubStatus()
    {
        if (AppSubStatus.HasValue && AppSubStatus.Value.ValueKind == JsonValueKind.Object)
        {
            try
            {
                return JsonSerializer.Deserialize<FssApiResultSubStatus>(AppSubStatus.Value.GetRawText());
            }
            catch
            {
                return null;
            }
        }
        return null;
    }
}
