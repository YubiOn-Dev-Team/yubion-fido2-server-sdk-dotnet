namespace YubiOn.Fss.Sdk.Internal;

/// <summary>
/// Interface for sending HTTP requests to the YubiOn FSS API.
/// </summary>
public interface IFssApiRequester : IDisposable
{
    /// <summary>
    /// Sends an API request asynchronously and returns the deserialized data payload.
    /// </summary>
    Task<T> RequestAsync<T>(string path, object? param = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an API request with cookie handling asynchronously.
    /// </summary>
    Task<FssApiCookieResponse<T>> RequestWithCookieResponseAsync<T>(
        string path,
        object? param = null,
        bool withAuth = true,
        string? cookie = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an API request synchronously and returns the deserialized data payload.
    /// </summary>
    T Request<T>(string path, object? param = null);

    /// <summary>
    /// Sends an API request with cookie handling synchronously.
    /// </summary>
    FssApiCookieResponse<T> RequestWithCookieResponse<T>(
        string path,
        object? param = null,
        bool withAuth = true,
        string? cookie = null);
}
