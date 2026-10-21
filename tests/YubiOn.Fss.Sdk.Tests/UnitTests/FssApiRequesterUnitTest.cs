using System.Net;
using System.Security.Cryptography;
using System.Text;
using YubiOn.Fss.Sdk.Config;
using YubiOn.Fss.Sdk.Internal;
using YubiOn.Fss.Sdk.Result;
using Xunit;

namespace YubiOn.Fss.Sdk.Tests.UnitTests;

public class FssApiRequesterUnitTest
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage>? Handler { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Handler == null)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
            return Task.FromResult(Handler(request));
        }
    }

    private static string GenerateTestPrivateKey()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        return SignAuthApiRequesterBase.Base64UrlEncode(ecdsa.ExportPkcs8PrivateKey());
    }

    [Fact]
    public void AccessKeyAuth_AttachesCorrectHeaders()
    {
        var mock = new MockHttpMessageHandler();
        HttpRequestMessage? capturedRequest = null;

        mock.Handler = req =>
        {
            capturedRequest = req;
            var responseJson = "{\"appStatus\":\"OK\",\"data\":{\"status\":\"ok\"}}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        };

        var config = new FssSdkConfig("test-rp", "auth-id-1", FssApiAuthType.AccessKeyAuth, "secret-access-key");
        using var httpClient = new HttpClient(mock);
        using var requester = new AccessKeyAuthApiRequester(config, customHttpClient: httpClient);

        var result = requester.Request<Dictionary<string, string>>("testPath", new { key = "value" });

        Assert.NotNull(result);
        Assert.Equal("ok", result["status"]);
        Assert.NotNull(capturedRequest);
        Assert.Equal("test-rp", capturedRequest.Headers.GetValues("X-Fss-Rp-Id").First());
        Assert.Equal("auth-id-1", capturedRequest.Headers.GetValues("X-Fss-Api-Auth-Id").First());
        Assert.Equal("secret-access-key", capturedRequest.Headers.GetValues("X-Fss-Auth-Access-Key").First());
    }

    [Fact]
    public void DatetimeSignAuth_AttachesCorrectSignatureHeaders()
    {
        var mock = new MockHttpMessageHandler();
        HttpRequestMessage? capturedRequest = null;

        mock.Handler = req =>
        {
            capturedRequest = req;
            var responseJson = "{\"appStatus\":\"OK\",\"data\":{\"status\":\"ok\"}}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        };

        string privateKey = GenerateTestPrivateKey();
        var config = new FssSdkConfig("test-rp", "auth-id-2", FssApiAuthType.DatetimeSignAuth, privateKey);
        using var httpClient = new HttpClient(mock);
        using var requester = new DatetimeSignAuthApiRequester(config, customHttpClient: httpClient);

        var result = requester.Request<Dictionary<string, string>>("testPath", new { test = 123 });

        Assert.NotNull(capturedRequest);
        Assert.Equal("test-rp", capturedRequest.Headers.GetValues("X-Fss-Rp-Id").First());
        Assert.Equal("auth-id-2", capturedRequest.Headers.GetValues("X-Fss-Api-Auth-Id").First());
        Assert.True(capturedRequest.Headers.Contains("X-Fss-Auth-Request-Time"));
        Assert.True(capturedRequest.Headers.Contains("X-Fss-Auth-Body-Hash"));
        Assert.True(capturedRequest.Headers.Contains("X-Fss-Auth-Signature"));

        // Verify signature format is Base64URL without padding
        string signature = capturedRequest.Headers.GetValues("X-Fss-Auth-Signature").First();
        Assert.DoesNotContain("=", signature);
        Assert.DoesNotContain("+", signature);
        Assert.DoesNotContain("/", signature);
    }

    [Fact]
    public void NonceSignAuth_CallsGetNonceThenSigns()
    {
        var mock = new MockHttpMessageHandler();
        var requests = new List<HttpRequestMessage>();

        mock.Handler = req =>
        {
            requests.Add(req);
            if (req.RequestUri!.AbsolutePath.EndsWith("getNonce"))
            {
                var nonceResp = "{\"appStatus\":\"OK\",\"data\":{\"nonce\":\"test-random-nonce-12345\"}}";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(nonceResp, Encoding.UTF8, "application/json")
                };
            }
            else
            {
                var dataResp = "{\"appStatus\":\"OK\",\"data\":{\"result\":\"success\"}}";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(dataResp, Encoding.UTF8, "application/json")
                };
            }
        };

        string privateKey = GenerateTestPrivateKey();
        var config = new FssSdkConfig("test-rp", "auth-id-3", FssApiAuthType.NonceSignAuth, privateKey);
        using var httpClient = new HttpClient(mock);
        using var requester = new NonceSignAuthApiRequester(config, customHttpClient: httpClient);

        var result = requester.Request<Dictionary<string, string>>("mainEndpoint", new { foo = "bar" });

        Assert.Equal(2, requests.Count);
        Assert.EndsWith("getNonce", requests[0].RequestUri!.AbsolutePath);
        Assert.EndsWith("mainEndpoint", requests[1].RequestUri!.AbsolutePath);

        // Verify main request headers
        var mainReq = requests[1];
        Assert.Equal("test-random-nonce-12345", mainReq.Headers.GetValues("X-Fss-Auth-Nonce").First());
        Assert.True(mainReq.Headers.Contains("X-Fss-Auth-Signature"));
        Assert.True(mainReq.Headers.Contains("X-Fss-Auth-Body-Hash"));
    }

    [Fact]
    public void ApiError_ThrowsFssApiException_WithProperStatus()
    {
        var mock = new MockHttpMessageHandler();

        mock.Handler = req =>
        {
            var errorJson = "{\"appStatus\":\"LICENSE_ERROR\",\"appSubStatus\":{\"errorCode\":\"LICENSE_LIMIT_EXCEEDED\",\"errorMessage\":\"Limit reached\"},\"message\":\"User limit reached\"}";
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorJson, Encoding.UTF8, "application/json")
            };
        };

        var config = new FssSdkConfig("test-rp", "auth-id-4", FssApiAuthType.AccessKeyAuth, "key");
        using var httpClient = new HttpClient(mock);
        using var requester = new AccessKeyAuthApiRequester(config, customHttpClient: httpClient);

        var ex = Assert.Throws<FssApiException>(() => requester.Request<object>("someEndpoint"));

        Assert.Equal(FssApiResultStatus.LICENSE_ERROR, ex.AppStatus);
        Assert.Equal(400, ex.HttpStatusCode);

        var subStatus = ex.GetParsedSubStatus();
        Assert.NotNull(subStatus);
        Assert.Equal(Fido2ErrorStatus.LICENSE_LIMIT_EXCEEDED, subStatus.ErrorCode);
        Assert.Equal("Limit reached", subStatus.ErrorMessage);
    }

    [Fact]
    public void CookieHandling_StoresAndTransmitsCookie()
    {
        var mock = new MockHttpMessageHandler();
        string? transmittedCookie = null;

        mock.Handler = req =>
        {
            if (req.Headers.TryGetValues("Cookie", out var cookieValues))
            {
                transmittedCookie = string.Join("; ", cookieValues);
            }

            var respJson = "{\"appStatus\":\"OK\",\"data\":{\"step\":\"ok\"}}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(respJson, Encoding.UTF8, "application/json")
            };
            response.Headers.Add("Set-Cookie", "SESSION_ID=abc123xyz; Path=/; HttpOnly");
            return response;
        };

        var config = new FssSdkConfig("test-rp", "auth-id-5", FssApiAuthType.AccessKeyAuth, "key");
        using var httpClient = new HttpClient(mock);
        using var requester = new AccessKeyAuthApiRequester(config, customHttpClient: httpClient);

        // 1. Initial request without cookie, receives cookie
        var initial = requester.RequestWithCookieResponse<Dictionary<string, string>>("step1");
        Assert.Equal("SESSION_ID=abc123xyz", initial.Cookie);

        // 2. Second request with cookie
        var second = requester.RequestWithCookieResponse<Dictionary<string, string>>("step2", null, true, initial.Cookie);
        Assert.Equal("SESSION_ID=abc123xyz", transmittedCookie);
    }
}
