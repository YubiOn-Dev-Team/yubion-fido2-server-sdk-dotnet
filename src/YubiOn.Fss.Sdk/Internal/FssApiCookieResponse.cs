namespace YubiOn.Fss.Sdk.Internal;

/// <summary>
/// A wrapper class that holds the API response and any associated cookies.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public class FssApiCookieResponse<T>
{
    public string? Cookie { get; set; }
    public T? Response { get; set; }

    public FssApiCookieResponse()
    {
    }

    public FssApiCookieResponse(string? cookie, T? response)
    {
        Cookie = cookie;
        Response = response;
    }

    // Java-compatible accessors
    public string? GetCookie() => Cookie;
    public string? GetResponseCookie() => Cookie;
    public T? GetResponse() => Response;
}
