using YubiOn.Fss.Sdk.Config;
using YubiOn.Fss.Sdk.Data.Credential;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;
using YubiOn.Fss.Sdk.Internal;
using YubiOn.Fss.Sdk.Result;
using Xunit;

namespace YubiOn.Fss.Sdk.Tests.UnitTests;

public class YubiOnFssSdkUnitTest
{
    private class FakeApiRequester : IFssApiRequester
    {
        public string? LastPath { get; private set; }
        public object? LastParam { get; private set; }
        public string? LastCookie { get; private set; }

        public object? ResponseToReturn { get; set; }
        public string? CookieToReturn { get; set; }

        public Task<T> RequestAsync<T>(string path, object? param = null, CancellationToken cancellationToken = default)
        {
            LastPath = path;
            LastParam = param;
            return Task.FromResult((T)ResponseToReturn!);
        }

        public Task<FssApiCookieResponse<T>> RequestWithCookieResponseAsync<T>(
            string path,
            object? param = null,
            bool withAuth = true,
            string? cookie = null,
            CancellationToken cancellationToken = default)
        {
            LastPath = path;
            LastParam = param;
            LastCookie = cookie;
            return Task.FromResult(new FssApiCookieResponse<T>(CookieToReturn, (T)ResponseToReturn!));
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

        public void Dispose()
        {
        }
    }

    private readonly FssSdkConfig _config = new("test.rp", "test-auth", FssApiAuthType.AccessKeyAuth, "key");

    [Fact]
    public void GetAllUsers_SendsCorrectPathAndParam()
    {
        var fake = new FakeApiRequester();
        fake.ResponseToReturn = new GetUsersResponse
        {
            Users = new List<UserDataWithCredentialCount>
            {
                new() { UserId = "u1", UserName = "Alice", CredentialCount = 2 }
            }
        };

        using var sdk = new YubiOnFssSdk(_config, fake);
        var resp = sdk.GetAllUsers(withDisabledUser: true);

        Assert.Equal("getAllUsers", fake.LastPath);
        var dict = Assert.IsAssignableFrom<IDictionary<string, object>>(fake.LastParam);
        Assert.True((bool)dict["withDisabledUser"]);
        Assert.Single(resp.Users!);
        Assert.Equal("u1", resp.Users![0].UserId);
    }

    [Fact]
    public void GetUser_SendsCorrectPathAndParams()
    {
        var fake = new FakeApiRequester();
        fake.ResponseToReturn = new GetUserResponse
        {
            User = new UserDataWithCredentialCount { UserId = "u1", UserName = "Bob" }
        };

        using var sdk = new YubiOnFssSdk(_config, fake);
        var resp = sdk.GetUser("u1", withDisabledUser: false, withDisabledCredential: true);

        Assert.Equal("getUser", fake.LastPath);
        var dict = Assert.IsAssignableFrom<IDictionary<string, object>>(fake.LastParam);
        Assert.Equal("u1", dict["userId"]);
        Assert.False((bool)dict["withDisabledUser"]);
        Assert.True((bool)dict["withDisabledCredential"]);
        Assert.Equal("Bob", resp.User!.UserName);
    }

    [Fact]
    public void RegisterUser_SendsUserData()
    {
        var fake = new FakeApiRequester();
        fake.ResponseToReturn = new RegisterUserResponse
        {
            User = new UserDataWithCredentialCount { UserId = "new-u", UserName = "Charlie" }
        };

        using var sdk = new YubiOnFssSdk(_config, fake);
        var prm = new UserDataRegisterParameter("new-u", "Charlie", "Charlie Doe");
        var resp = sdk.RegisterUser(prm);

        Assert.Equal("registerUser", fake.LastPath);
        var dict = Assert.IsAssignableFrom<IDictionary<string, object>>(fake.LastParam);
        Assert.Same(prm, dict["user"]);
        Assert.Equal("Charlie", resp.User!.UserName);
    }

    [Fact]
    public void UpdateUser_SendsUpdateData()
    {
        var fake = new FakeApiRequester();
        fake.ResponseToReturn = new UpdateUserResponse
        {
            User = new UserDataWithCredentialCount { UserId = "u1", UserName = "UpdatedName" }
        };

        using var sdk = new YubiOnFssSdk(_config, fake);
        var prm = new UserDataUpdateParameter("u1") { UserName = "UpdatedName" };
        var resp = sdk.UpdateUser(prm, withUpdatedCheck: true);

        Assert.Equal("updateUser", fake.LastPath);
        var dict = Assert.IsAssignableFrom<IDictionary<string, object>>(fake.LastParam);
        Assert.Same(prm, dict["user"]);
        Assert.Equal("UpdatedName", resp.User!.UserName);
    }

    [Fact]
    public void DeleteUser_SendsUserId()
    {
        var fake = new FakeApiRequester();
        fake.ResponseToReturn = new DeleteUserResponse
        {
            User = new UserDataWithCredentialCount { UserId = "u1" }
        };

        using var sdk = new YubiOnFssSdk(_config, fake);
        var resp = sdk.DeleteUser("u1");

        Assert.Equal("deleteUser", fake.LastPath);
        var dict = Assert.IsAssignableFrom<IDictionary<string, object>>(fake.LastParam);
        Assert.Equal("u1", dict["userId"]);
    }

    [Fact]
    public void CredentialOperations_SendCorrectRequests()
    {
        var fake = new FakeApiRequester();
        fake.ResponseToReturn = new GetCredentialResponse
        {
            Credential = new CredentialData { CredentialId = "c1", Name = "Security Key 1" }
        };

        using var sdk = new YubiOnFssSdk(_config, fake);

        // Get Credential
        var getResp = sdk.GetCredential("u1", "c1");
        Assert.Equal("getCredential", fake.LastPath);
        Assert.Equal("Security Key 1", getResp.Credential!.Name);

        // Update Credential
        fake.ResponseToReturn = new UpdateCredentialResponse
        {
            Credential = new CredentialData { CredentialId = "c1", Name = "Renamed Key" }
        };
        var updateResp = sdk.UpdateCredential(new CredentialDataUpdateParameter("u1", "c1") { Name = "Renamed Key" });
        Assert.Equal("updateCredential", fake.LastPath);
        Assert.Equal("Renamed Key", updateResp.Credential!.Name);

        // Delete Credential
        fake.ResponseToReturn = new DeleteCredentialResponse
        {
            Credential = new CredentialData { CredentialId = "c1" }
        };
        var delResp = sdk.DeleteCredential("u1", "c1");
        Assert.Equal("deleteCredential", fake.LastPath);
    }

    [Fact]
    public void FidoCeremony_StartsAndFinishesWithCookie()
    {
        var fake = new FakeApiRequester();
        fake.CookieToReturn = "SESSION_COOKIE_VALUE";
        fake.ResponseToReturn = new StartRegisterCredentialResponse
        {
            CreationOptions = new PublicKeyCredentialCreationOptionsJson
            {
                Challenge = "test-reg-challenge"
            }
        };

        using var sdk = new YubiOnFssSdk(_config, fake);

        // Start Register
        var startResp = sdk.StartRegisterCredential(new Fido2StartRegisterParameter());
        Assert.Equal("registerCredential/start", fake.LastPath);
        Assert.Equal("SESSION_COOKIE_VALUE", startResp.SessionCookie);
        Assert.Equal("test-reg-challenge", startResp.Data.CreationOptions!.Challenge);

        // Finish Register
        fake.ResponseToReturn = new FinishRegisterCredentialResponse
        {
            Credential = new CredentialData { CredentialId = "new-cred" }
        };
        var finishResp = sdk.FinishRegisterCredential(new Fido2FinishRegisterParameter(), startResp.SessionCookie!);
        Assert.Equal("registerCredential/finish", fake.LastPath);
        Assert.Equal("SESSION_COOKIE_VALUE", fake.LastCookie);
        Assert.Equal("new-cred", finishResp.Credential!.CredentialId);

        // Start Authenticate
        fake.ResponseToReturn = new StartAuthenticateResponse
        {
            RequestOptions = new PublicKeyCredentialRequestOptionsJson
            {
                Challenge = "test-auth-challenge"
            }
        };
        var authStart = sdk.StartAuthenticate(new Fido2StartAuthenticateParameter());
        Assert.Equal("authenticate/start", fake.LastPath);
        Assert.Equal("test-auth-challenge", authStart.Data.RequestOptions!.Challenge);

        // Finish Authenticate
        fake.ResponseToReturn = new FinishAuthenticateResponse
        {
            Credential = new CredentialData { CredentialId = "auth-cred" }
        };
        var authFinish = sdk.FinishAuthenticate(new Fido2FinishAuthenticateParameter(), authStart.SessionCookie!);
        Assert.Equal("authenticate/finish", fake.LastPath);
        Assert.Equal("auth-cred", authFinish.Credential!.CredentialId);
    }

    [Fact]
    public void PublicKeyCredentialOptions_JsonSerialization_OmitsNullMembers()
    {
        var reqOptions = new PublicKeyCredentialRequestOptionsJson
        {
            Challenge = "test-challenge",
            RpId = "localhost"
        };
        string reqJson = System.Text.Json.JsonSerializer.Serialize(reqOptions);
        Assert.DoesNotContain("allowCredentials", reqJson);
        Assert.DoesNotContain("extensions", reqJson);
        Assert.DoesNotContain("hints", reqJson);
        Assert.Contains("\"challenge\":\"test-challenge\"", reqJson);
        Assert.Contains("\"rpId\":\"localhost\"", reqJson);

        var creatOptions = new PublicKeyCredentialCreationOptionsJson
        {
            Challenge = "test-challenge"
        };
        string creatJson = System.Text.Json.JsonSerializer.Serialize(creatOptions);
        Assert.DoesNotContain("excludeCredentials", creatJson);
        Assert.DoesNotContain("extensions", creatJson);
    }
}
