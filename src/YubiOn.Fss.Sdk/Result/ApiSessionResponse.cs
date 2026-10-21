namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Wraps an API response that includes a session cookie from the FSS server.
/// </summary>
/// <typeparam name="T">The type of the response payload.</typeparam>
public class ApiSessionResponse<T>
{
    /// <summary>
    /// The response payload.
    /// </summary>
    public T Data { get; set; } = default!;

    /// <summary>
    /// The session cookie string returned in the Set-Cookie header.
    /// </summary>
    public string? SessionCookie { get; set; }

    /// <summary>
    /// Initializes a new empty instance of <see cref="ApiSessionResponse{T}"/>.
    /// </summary>
    public ApiSessionResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ApiSessionResponse{T}"/> with data and cookie.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <param name="sessionCookie">The session cookie.</param>
    public ApiSessionResponse(T data, string? sessionCookie)
    {
        Data = data;
        SessionCookie = sessionCookie;
    }
}
