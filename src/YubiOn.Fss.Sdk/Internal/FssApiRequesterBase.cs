using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Config;
using YubiOn.Fss.Sdk.Result;

namespace YubiOn.Fss.Sdk.Internal;

/// <summary>
/// Base class for sending HTTP requests to the YubiOn FSS API.
/// </summary>
public abstract class FssApiRequesterBase : IFssApiRequester
{
    protected readonly FssSdkConfig Config;
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions SerializerOptions;
    private readonly bool _disposeClient;

    protected FssApiRequesterBase(FssSdkConfig config, bool denyUnknownResponseMember = false, bool allowBadSslForDebug = false, HttpClient? customHttpClient = null)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));

        SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

#if NET8_0_OR_GREATER
        if (denyUnknownResponseMember)
        {
            SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        }
#endif

        if (customHttpClient != null)
        {
            HttpClient = customHttpClient;
            _disposeClient = false;
        }
        else
        {
            var handler = new HttpClientHandler();
            if (allowBadSslForDebug)
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            HttpClient = new HttpClient(handler);
            _disposeClient = true;
        }
    }

    protected abstract Task<Dictionary<string, string>> GetAdditionalHeadersAsync(byte[] body, CancellationToken cancellationToken);
    protected abstract Dictionary<string, string> GetAdditionalHeaders(byte[] body);

    public async Task<T> RequestAsync<T>(string path, object? param = null, CancellationToken cancellationToken = default)
    {
        var cookieResponse = await RequestWithCookieResponseAsync<T>(path, param, withAuth: true, cookie: null, cancellationToken).ConfigureAwait(false);
        return cookieResponse.Response!;
    }

    public async Task<FssApiCookieResponse<T>> RequestWithCookieResponseAsync<T>(
        string path,
        object? param = null,
        bool withAuth = true,
        string? cookie = null,
        CancellationToken cancellationToken = default)
    {
        byte[] bodyBytes;
        try
        {
            string json = JsonSerializer.Serialize(param ?? new object(), SerializerOptions);
            bodyBytes = Encoding.UTF8.GetBytes(json);
        }
        catch (Exception ex)
        {
            throw new FssApiException("Failed to serialize request body.", FssApiResultStatus.UNEXPECTED_ERROR, null, ex);
        }

        var uri = BuildUri(path);
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = new ByteArrayContent(bodyBytes);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrEmpty(Config.Agent))
        {
            request.Headers.TryAddWithoutValidation("User-Agent", Config.Agent);
        }

        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        if (withAuth)
        {
            try
            {
                var additionalHeaders = await GetAdditionalHeadersAsync(bodyBytes, cancellationToken).ConfigureAwait(false);
                foreach (var header in additionalHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            catch (Exception ex) when (ex is not FssApiException)
            {
                throw new FssApiException("Failed to generate authentication headers.", FssApiResultStatus.UNEXPECTED_ERROR, null, ex);
            }
        }

        HttpResponseMessage response;
        try
        {
            response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FssApiException("Communication failed.", FssApiResultStatus.COMMUNICATION_FAILED, null, ex);
        }

        using (response)
        {
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            int statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                // Attempt to parse API error response
                FssApiJsonResponse<object>? errorObj = null;
                try
                {
                    errorObj = JsonSerializer.Deserialize<FssApiJsonResponse<object>>(responseString, SerializerOptions);
                }
                catch
                {
                    // Ignored
                }

                if (errorObj != null && errorObj.AppStatus != FssApiResultStatus.OK)
                {
                    throw new FssApiException(
                        $"API returned an error: {errorObj.AppStatus}",
                        errorObj.AppStatus,
                        errorObj.AppSubStatus,
                        null,
                        statusCode,
                        responseString);
                }

                throw new FssApiException(
                    $"Unexpected HTTP code: {statusCode}",
                    FssApiResultStatus.COMMUNICATION_FAILED,
                    null,
                    null,
                    statusCode,
                    responseString);
            }

            FssApiJsonResponse<T>? apiResponse;
            try
            {
                apiResponse = JsonSerializer.Deserialize<FssApiJsonResponse<T>>(responseString, SerializerOptions);
            }
            catch (Exception ex)
            {
                throw new FssApiException("Failed to parse API response JSON.", FssApiResultStatus.BAD_JSON_FORMAT, null, ex, statusCode, responseString);
            }

            if (apiResponse == null)
            {
                throw new FssApiException("Response body is null.", FssApiResultStatus.COMMUNICATION_FAILED, null, null, statusCode, responseString);
            }

            if (apiResponse.AppStatus != FssApiResultStatus.OK)
            {
                throw new FssApiException(
                    $"API returned an error: {apiResponse.AppStatus}",
                    apiResponse.AppStatus,
                    apiResponse.AppSubStatus,
                    null,
                    statusCode,
                    responseString);
            }

            string? responseCookie = ExtractCookie(response);
            return new FssApiCookieResponse<T>(responseCookie, apiResponse.Data);
        }
    }

    public T Request<T>(string path, object? param = null)
    {
        return RequestAsync<T>(path, param).GetAwaiter().GetResult();
    }

    public FssApiCookieResponse<T> RequestWithCookieResponse<T>(
        string path,
        object? param = null,
        bool withAuth = true,
        string? cookie = null)
    {
        return RequestWithCookieResponseAsync<T>(path, param, withAuth, cookie).GetAwaiter().GetResult();
    }

    protected Uri BuildUri(string path)
    {
        string endpoint = Config.Endpoint.TrimEnd('/');
        string relativePath = path.TrimStart('/');
        return new Uri($"{endpoint}/{relativePath}");
    }

    private static string? ExtractCookie(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            var formatted = values.Select(c => c.Split(';')[0].Trim());
            return string.Join("; ", formatted);
        }
        return null;
    }

    public virtual void Dispose()
    {
        if (_disposeClient)
        {
            HttpClient.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
