using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;
using YubiOn.Fss.Sdk.Result;
using YubiOn.Fss.Sdk.Tests.Util;
using Xunit;

namespace YubiOn.Fss.Sdk.Tests.IntegrationTests;

/// <summary>
/// Live integration tests for FIDO2 ceremony endpoints matching NodeJS yubion-fss-sdk-fido.test.ts
/// and Java YubiOnFssSdkFidoTest.java suites.
/// Verifies startRegister, verifyRegister, finishRegister, duplicate username checks, and authenticate.
/// </summary>
[Collection("FssLiveTest")]
public class YubiOnFssSdkFidoLiveTest : IDisposable
{
    private readonly bool _allowBadSsl = YubiOnFssSdkTestConfig.AllowBadSsl;
    private readonly bool _denyUnknown = YubiOnFssSdkTestConfig.DenyUnknownResponseMember;

    private static string MakeUserId(Func<int, byte> func, int length = 64)
    {
        var buf = new byte[length];
        for (int i = 0; i < length; i++)
        {
            buf[i] = func(i);
        }
        return Convert.ToBase64String(buf)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private YubiOnFssSdk CreateSdk()
    {
        return new YubiOnFssSdk(
            YubiOnFssSdkTestConfig.CreateNonceConfig(true),
            _denyUnknown,
            _allowBadSsl);
    }

    private readonly UserDataRegisterParameter[] _testUsers;
    private readonly UserDataRegisterParameter[] _testDuplicatedUserNameUsers;

    public YubiOnFssSdkFidoLiveTest()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials())
        {
            _testUsers = Array.Empty<UserDataRegisterParameter>();
            _testDuplicatedUserNameUsers = Array.Empty<UserDataRegisterParameter>();
            return;
        }

        _testUsers = new[]
        {
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 30)),
                "live_fido_user1@example.com",
                "user 1",
                new Dictionary<string, object> { ["attr1"] = "user1" },
                false
            ),
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 31)),
                "live_fido_user2@example.com",
                "user 2",
                new Dictionary<string, object> { ["attr1"] = "user2" },
                false
            ),
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 32)),
                "live_fido_user3@example.com",
                "user 3",
                new Dictionary<string, object> { ["attr1"] = "user3" },
                false
            ),
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 33)),
                "live_fido_user4@example.com",
                "user 4",
                new Dictionary<string, object> { ["attr1"] = "user4" },
                true
            ),
        };

        _testDuplicatedUserNameUsers = new[]
        {
            // idx: 0 base
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 40)),
                "live_fido_dup_user@example.com",
                "duplicated_username",
                new Dictionary<string, object> { ["attr1"] = "duplicated_username" },
                false
            ),
            // idx: 1 different userId, but same userName
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 41)),
                "live_fido_dup_user@example.com",
                "duplicated_username",
                new Dictionary<string, object> { ["attr1"] = "duplicated_username" },
                false
            ),
            // idx: 2 different userId, different userName (will update to same userName later)
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 42)),
                "live_fido_dup_other@example.com",
                "duplicated_username_other",
                new Dictionary<string, object> { ["attr1"] = "duplicated_username_other" },
                false
            ),
        };

        SetUp();
    }

    private void SetUp()
    {
        var sdk = CreateSdk();
        try
        {
            var users = sdk.GetAllUsers(withDisabledUser: true);
            var testUserIds = _testUsers.Select(u => u.UserId)
                .Concat(_testDuplicatedUserNameUsers.Select(u => u.UserId))
                .ToHashSet();

            if (users.Users != null)
            {
                foreach (var user in users.Users)
                {
                    if (user.UserId != null && (testUserIds.Contains(user.UserId) || user.UserName?.StartsWith("live_fido_") == true))
                    {
                        try { sdk.DeleteUser(user.UserId); } catch { }
                    }
                }
            }
        }
        catch { }

        foreach (var user in _testUsers)
        {
            sdk.RegisterUser(user);
        }
    }

    public void Dispose()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;

        var sdk = CreateSdk();
        try
        {
            foreach (var user in _testUsers.Concat(_testDuplicatedUserNameUsers))
            {
                if (user.UserId != null)
                {
                    try { sdk.DeleteUser(user.UserId); } catch { }
                }
            }
        }
        catch { }
    }

    [Fact]
    public void Live_Register_Success_And_Verify_And_Duplicated()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;
        var sdk = CreateSdk();

        // 1. Parameter error with empty/null
        {
            var ex = Assert.Throws<FssApiException>(() =>
                sdk.StartRegisterCredential(new Fido2StartRegisterParameter(null!, null!)));
            Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, ex.AppStatus);
        }

        // 2. Normal registration with emulator
        {
            var authenticator = new AuthenticatorEmulator(sdk.RpId!);
            var origin = "https://" + sdk.RpId;

            var startResp = sdk.StartRegisterCredential(new Fido2StartRegisterParameter(
                new Fido2CreationOptionsBase
                {
                    Attestation = AttestationConveyancePreference.None,
                    AuthenticatorSelection = new AuthenticatorSelectionCriteria
                    {
                        UserVerification = UserVerificationRequirement.Required,
                        ResidentKey = ResidentKeyRequirement.Preferred
                    }
                },
                _testUsers[0]
            ));

            Assert.NotNull(startResp);
            Assert.NotNull(startResp.Data.CreationOptions);

            var clientCredResp = authenticator.MakeCred(origin, startResp.Data.CreationOptions);
            var finishParam = new Fido2FinishRegisterParameter
            {
                CreateResponse = new Fido2FinishRegisterParameter.CreateResponseClass
                {
                    AttestationResponse = clientCredResp,
                    Transports = new[] { "usb" }
                }
            };

            // 3. Verify register credential
            var verifyResp = sdk.VerifyRegisterCredential(finishParam, startResp.SessionCookie!);
            Assert.NotNull(verifyResp);

            // 4. Finish register credential
            var finishResp = sdk.FinishRegisterCredential(finishParam, startResp.SessionCookie!);
            Assert.NotNull(finishResp);
            Assert.NotNull(finishResp.Credential);

            // 5. Verify user has 1 credential
            var userResp = sdk.GetUser(_testUsers[0].UserId!);
            Assert.NotNull(userResp.Credentials);
            Assert.Single(userResp.Credentials);

            // 6. Duplicated user name : register base user with createUserIfNotExists
            var startBaseResp = sdk.StartRegisterCredential(new Fido2StartRegisterParameter(
                new Fido2CreationOptionsBase(),
                _testDuplicatedUserNameUsers[0],
                new Fido2RegisterOptions(createUserIfNotExists: true, updateUserIfExists: false, null, null)
            ));
            Assert.NotNull(startBaseResp);

            // 7. Duplicated user name : register different userId, but same userName with createUserIfNotExists -> DUPLICATED
            var exDup = Assert.Throws<FssApiException>(() =>
                sdk.StartRegisterCredential(new Fido2StartRegisterParameter(
                    new Fido2CreationOptionsBase(),
                    _testDuplicatedUserNameUsers[1],
                    new Fido2RegisterOptions(createUserIfNotExists: true, updateUserIfExists: false, null, null)
                )));
            Assert.Equal(FssApiResultStatus.DUPLICATED, exDup.AppStatus);

            // 8. Duplicated user name : register different userId, different userName with createUserIfNotExists
            var startUpdateResp = sdk.StartRegisterCredential(new Fido2StartRegisterParameter(
                new Fido2CreationOptionsBase(),
                _testDuplicatedUserNameUsers[2],
                new Fido2RegisterOptions(createUserIfNotExists: true, updateUserIfExists: false, null, null)
            ));
            Assert.NotNull(startUpdateResp);

            // 9. Duplicated user name : update to existing user name with updateUserIfExists -> DUPLICATED
            var dupTargetUser = new UserDataRegisterParameter(
                _testDuplicatedUserNameUsers[2].UserId,
                _testDuplicatedUserNameUsers[0].UserName,
                _testDuplicatedUserNameUsers[2].DisplayName,
                _testDuplicatedUserNameUsers[2].UserAttributes,
                _testDuplicatedUserNameUsers[2].Disabled
            );

            var exDupUpdate = Assert.Throws<FssApiException>(() =>
                sdk.StartRegisterCredential(new Fido2StartRegisterParameter(
                    new Fido2CreationOptionsBase(),
                    dupTargetUser,
                    new Fido2RegisterOptions(createUserIfNotExists: false, updateUserIfExists: true, null, null)
                )));
            Assert.Equal(FssApiResultStatus.DUPLICATED, exDupUpdate.AppStatus);
        }
    }

    [Fact]
    public void Live_Authenticate_Success()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;
        var sdk = CreateSdk();

        var authenticator = new AuthenticatorEmulator(sdk.RpId!);
        var origin = "https://" + sdk.RpId;

        // 1. Register credential for testUsers[1]
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
            _testUsers[1]
        ));
        Assert.NotNull(startRegResp);

        var clientCredResp = authenticator.MakeCred(origin, startRegResp.Data.CreationOptions!);
        var finishRegResp = sdk.FinishRegisterCredential(new Fido2FinishRegisterParameter
        {
            CreateResponse = new Fido2FinishRegisterParameter.CreateResponseClass
            {
                AttestationResponse = clientCredResp,
                Transports = new[] { "usb" }
            }
        }, startRegResp.SessionCookie!);
        Assert.NotNull(finishRegResp);

        var userResp = sdk.GetUser(_testUsers[1].UserId!);
        Assert.NotNull(userResp.Credentials);
        Assert.Single(userResp.Credentials);

        // 2. Start authenticate
        var startAuthResp = sdk.StartAuthenticate(new Fido2StartAuthenticateParameter(
            new Fido2RequestOptionsBase
            {
                UserVerification = UserVerificationRequirement.Required
            }
        )
        {
            UserId = _testUsers[1].UserId
        });
        Assert.NotNull(startAuthResp);
        Assert.NotNull(startAuthResp.Data.RequestOptions);

        // 3. Authenticator Get Assertion
        var clientAuthResp = authenticator.GetAssr(origin, startAuthResp.Data.RequestOptions);

        // 4. Finish authenticate
        var finishAuthResp = sdk.FinishAuthenticate(new Fido2FinishAuthenticateParameter
        {
            RequestResponse = new Fido2FinishAuthenticateParameter.RequestResponseClass
            {
                AttestationResponse = clientAuthResp
            }
        }, startAuthResp.SessionCookie!);

        Assert.NotNull(finishAuthResp);
        Assert.NotNull(finishAuthResp.Credential);
        Assert.Equal(finishRegResp.Credential!.CredentialId, finishAuthResp.Credential.CredentialId);
    }
}
