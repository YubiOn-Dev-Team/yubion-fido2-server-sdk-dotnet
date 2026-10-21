using YubiOn.Fss.Sdk.Data.Credential;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;
using YubiOn.Fss.Sdk.Result;
using YubiOn.Fss.Sdk.Tests.Util;
using Xunit;

namespace YubiOn.Fss.Sdk.Tests.IntegrationTests;

[Collection("FssLiveTest")]
public class YubiOnFssSdkLiveTest
{
    private static bool IsLiveTestConfigured => YubiOnFssSdkTestConfig.HasLiveTestCredentials();

    [Fact]
    public void Live_GetAllUsers_WhenConfigured()
    {
        if (!IsLiveTestConfigured) return;

        var config = YubiOnFssSdkTestConfig.CreateNonceConfig(true);
        using var sdk = new YubiOnFssSdk(config, YubiOnFssSdkTestConfig.DenyUnknownResponseMember, YubiOnFssSdkTestConfig.AllowBadSsl);

        var users = sdk.GetAllUsers(false);
        Assert.NotNull(users);
        Assert.NotNull(users.Users);
    }

    [Fact]
    public void Live_AuthFail_WithIncorrectKey()
    {
        if (!IsLiveTestConfigured) return;

        var config = YubiOnFssSdkTestConfig.CreateNonceConfig(false);
        using var sdk = new YubiOnFssSdk(config, YubiOnFssSdkTestConfig.DenyUnknownResponseMember, YubiOnFssSdkTestConfig.AllowBadSsl);

        var ex = Assert.Throws<FssApiException>(() => sdk.GetAllUsers());
        Assert.Equal(FssApiResultStatus.AUTHENTICATION_FAILED, ex.AppStatus);
    }

    private static string MakeUserId()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    [Fact]
    public void Live_UserCrudCycle_WhenConfigured()
    {
        if (!IsLiveTestConfigured) return;

        var config = YubiOnFssSdkTestConfig.CreateNonceConfig(true);
        using var sdk = new YubiOnFssSdk(config, YubiOnFssSdkTestConfig.DenyUnknownResponseMember, YubiOnFssSdkTestConfig.AllowBadSsl);

        string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        string testUserId = MakeUserId();
        string testUserName = "live_user_" + uniqueSuffix + "@example.com";
        try
        {
            // 1. Register
            var regResp = sdk.RegisterUser(new UserDataRegisterParameter(testUserId, testUserName, "Live User Display"));
            Assert.NotNull(regResp.User);
            Assert.Equal(testUserName, regResp.User.UserName);

            // 2. Get
            var getResp = sdk.GetUser(testUserId);
            Assert.NotNull(getResp.User);
            Assert.Equal(testUserName, getResp.User.UserName);

            // 3. Update
            var updateParam = new UserDataUpdateParameter(regResp.User!) { DisplayName = "Updated Display" };
            var updateResp = sdk.UpdateUser(updateParam);
            Assert.Equal("Updated Display", updateResp.User!.DisplayName);
        }
        finally
        {
            // Cleanup
            try
            {
                sdk.DeleteUser(testUserId);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void Live_DatetimeAuth_WhenConfigured()
    {
        if (!IsLiveTestConfigured) return;

        var config = YubiOnFssSdkTestConfig.CreateDatetimeConfig(true);
        using var sdk = new YubiOnFssSdk(config, YubiOnFssSdkTestConfig.DenyUnknownResponseMember, YubiOnFssSdkTestConfig.AllowBadSsl);

        var users = sdk.GetAllUsers(false);
        Assert.NotNull(users);
        Assert.NotNull(users.Users);
    }

    [Fact]
    public void Live_AccessKeyAuth_WhenConfigured()
    {
        if (!IsLiveTestConfigured) return;

        var config = YubiOnFssSdkTestConfig.CreateAccessKeyConfig(true);
        using var sdk = new YubiOnFssSdk(config, YubiOnFssSdkTestConfig.DenyUnknownResponseMember, YubiOnFssSdkTestConfig.AllowBadSsl);

        var users = sdk.GetAllUsers(false);
        Assert.NotNull(users);
        Assert.NotNull(users.Users);
    }

    [Fact]
    public void Live_FidoRegisterAndAuthenticate_WhenConfigured()
    {
        if (!IsLiveTestConfigured) return;

        var config = YubiOnFssSdkTestConfig.CreateNonceConfig(true);
        using var sdk = new YubiOnFssSdk(config, YubiOnFssSdkTestConfig.DenyUnknownResponseMember, YubiOnFssSdkTestConfig.AllowBadSsl);

        string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        string testUserId = MakeUserId();
        string testUserName = "fido_user_" + uniqueSuffix + "@example.com";
        var userParam = new UserDataRegisterParameter(testUserId, testUserName, "FIDO Test User");

        try
        {
            // 1. Register User
            var regUserResp = sdk.RegisterUser(userParam);
            Assert.NotNull(regUserResp.User);

            // 2. Start Register Credential
            var authenticator = new AuthenticatorEmulator(sdk.RpId!);
            var startRegResp = sdk.StartRegisterCredential(new Fido2StartRegisterParameter(
                new Fido2CreationOptionsBase
                {
                    Attestation = AttestationConveyancePreference.None,
                    AuthenticatorSelection = new AuthenticatorSelectionCriteria
                    {
                        UserVerification = UserVerificationRequirement.Required,
                        ResidentKey = ResidentKeyRequirement.Preferred
                    }
                },
                userParam,
                new Fido2RegisterOptions(true, false, null, null)
            ));

            Assert.NotNull(startRegResp);
            Assert.NotNull(startRegResp.Data.CreationOptions);

            // 3. Authenticator Make Credential
            var clientCredResp = authenticator.MakeCred("https://" + sdk.RpId, startRegResp.Data.CreationOptions);

            // 4. Finish Register Credential
            var finishRegParam = new Fido2FinishRegisterParameter
            {
                CreateResponse = new Fido2FinishRegisterParameter.CreateResponseClass
                {
                    AttestationResponse = clientCredResp,
                    Transports = new[] { "usb" }
                }
            };
            var finishRegResp = sdk.FinishRegisterCredential(finishRegParam, startRegResp.SessionCookie!);
            Assert.NotNull(finishRegResp);

            // 5. Start Authenticate
            var startAuthResp = sdk.StartAuthenticate(new Fido2StartAuthenticateParameter(
                new Fido2RequestOptionsBase
                {
                    UserVerification = UserVerificationRequirement.Required
                }
            )
            {
                UserId = testUserId
            });
            Assert.NotNull(startAuthResp);
            Assert.NotNull(startAuthResp.Data.RequestOptions);

            // 6. Authenticator Get Assertion
            var clientAuthResp = authenticator.GetAssr("https://" + sdk.RpId, startAuthResp.Data.RequestOptions);

            // 7. Finish Authenticate
            var finishAuthParam = new Fido2FinishAuthenticateParameter
            {
                RequestResponse = new Fido2FinishAuthenticateParameter.RequestResponseClass
                {
                    AttestationResponse = clientAuthResp
                }
            };
            var finishAuthResp = sdk.FinishAuthenticate(finishAuthParam, startAuthResp.SessionCookie!);
            Assert.NotNull(finishAuthResp);
        }
        finally
        {
            try
            {
                sdk.DeleteUser(testUserId);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
