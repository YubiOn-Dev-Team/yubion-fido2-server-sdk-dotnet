using YubiOn.Fss.Sdk.Data.Credential;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;
using YubiOn.Fss.Sdk.Result;
using YubiOn.Fss.Sdk.Tests.Util;
using Xunit;

namespace YubiOn.Fss.Sdk.Tests.IntegrationTests;

/// <summary>
/// Comprehensive live integration tests for Credential API endpoints matching Java and JS SDK test suites.
/// Verifies NOT_FOUND, PARAMETER_ERROR, UPDATE_ERROR (optimistic lock), and credential lifecycle.
/// </summary>
[Collection("FssLiveTest")]
public class YubiOnFssSdkCredentialLiveTest : IDisposable
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
        return Base64UrlEncode(buf);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
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
    private readonly List<CredentialDataUpdateParameter> _testCreds = new();

    public YubiOnFssSdkCredentialLiveTest()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials())
        {
            _testUsers = Array.Empty<UserDataRegisterParameter>();
            return;
        }

        _testUsers = new[]
        {
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 40)),
                "live_cred_user1@example.com",
                "cred user 1",
                new Dictionary<string, object> { ["attr1"] = "user1" },
                false
            ),
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 41)),
                "live_cred_user2@example.com",
                "cred user 2",
                new Dictionary<string, object> { ["attr1"] = "user2" },
                false
            ),
        };

        SetUp();
    }

    private void SetUp()
    {
        var sdk = CreateSdk();

        // 1. Cleanup existing users
        try
        {
            var users = sdk.GetAllUsers(withDisabledUser: true);
            var testUserIds = _testUsers.Select(u => u.UserId).ToHashSet();
            if (users.Users != null)
            {
                foreach (var user in users.Users)
                {
                    if (user.UserId != null && (testUserIds.Contains(user.UserId) || user.UserName?.StartsWith("live_cred_") == true))
                    {
                        try { sdk.DeleteUser(user.UserId); } catch { }
                    }
                }
            }
        }
        catch { }

        // 2. Register users
        foreach (var user in _testUsers)
        {
            sdk.RegisterUser(user);
        }

        // 3. Register credentials via AuthenticatorEmulator
        // Cred 0: User 0, Enabled
        // Cred 1: User 0, Disabled
        // Cred 2: User 1, Enabled (then user 1 is disabled)
        var credDefinitions = new[]
        {
            new { UserIndex = 0, Name = "user 1 - credential 1", Disabled = false },
            new { UserIndex = 0, Name = "user 1 - credential 2 (disabled)", Disabled = true },
            new { UserIndex = 1, Name = "user 2 - credential 1", Disabled = false },
        };

        foreach (var def in credDefinitions)
        {
            var targetUser = _testUsers[def.UserIndex];
            var authenticator = new AuthenticatorEmulator(sdk.RpId!);

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
                targetUser
            ));

            var clientCredResp = authenticator.MakeCred("https://" + sdk.RpId, startResp.Data.CreationOptions!);
            var finishParam = new Fido2FinishRegisterParameter
            {
                CreateResponse = new Fido2FinishRegisterParameter.CreateResponseClass
                {
                    AttestationResponse = clientCredResp,
                    Transports = new[] { "usb" }
                }
            };
            var finishResp = sdk.FinishRegisterCredential(finishParam, startResp.SessionCookie!);
            string credentialId = finishResp.Credential!.CredentialId!;

            var updatePrm = new CredentialDataUpdateParameter(
                targetUser.UserId,
                credentialId,
                def.Name,
                new Dictionary<string, object> { ["cred-attr1"] = "val1" },
                def.Disabled
            );

            sdk.UpdateCredential(updatePrm);
            _testCreds.Add(updatePrm);
        }

        // Disable User 2
        var user2Update = new UserDataUpdateParameter(_testUsers[1])
        {
            Disabled = true
        };
        sdk.UpdateUser(user2Update);
    }

    public void Dispose()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;

        var sdk = CreateSdk();
        try
        {
            foreach (var user in _testUsers)
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
    public void Live_GetCredential_Success_And_NotFound_And_DisabledFilters()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;
        if (_testCreds.Count < 3) return;

        var sdk = CreateSdk();

        // 1. NOT_FOUND on non-existent user
        var exUserNotFound = Assert.Throws<FssApiException>(() =>
            sdk.GetCredential("non_existent_user_id", _testCreds[0].CredentialId!));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exUserNotFound.AppStatus);

        // 2. NOT_FOUND on non-existent credential
        var exCredNotFound = Assert.Throws<FssApiException>(() =>
            sdk.GetCredential(_testCreds[0].UserId!, "non_existent_cred_id"));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exCredNotFound.AppStatus);

        // 3. Normal active credential
        var activeCred = sdk.GetCredential(_testCreds[0].UserId!, _testCreds[0].CredentialId!);
        Assert.NotNull(activeCred.Credential);
        Assert.Equal(_testCreds[0].CredentialId, activeCred.Credential.CredentialId);
        Assert.Equal(_testCreds[0].CredentialName, activeCred.Credential.CredentialName);
        Assert.False(activeCred.Credential.Disabled);

        // 4. Disabled credential without withDisabledCredential should throw NOT_FOUND
        var exDisabledCred = Assert.Throws<FssApiException>(() =>
            sdk.GetCredential(_testCreds[1].UserId!, _testCreds[1].CredentialId!, withDisabledCredential: false));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exDisabledCred.AppStatus);

        // 5. Disabled credential with withDisabledCredential = true
        var disabledCred = sdk.GetCredential(_testCreds[1].UserId!, _testCreds[1].CredentialId!, withDisabledCredential: true);
        Assert.NotNull(disabledCred.Credential);
        Assert.True(disabledCred.Credential.Disabled);

        // 6. Credential of disabled user without withDisabledUser should throw NOT_FOUND
        var exDisabledUser = Assert.Throws<FssApiException>(() =>
            sdk.GetCredential(_testCreds[2].UserId!, _testCreds[2].CredentialId!, withDisabledUser: false));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exDisabledUser.AppStatus);

        // 7. Credential of disabled user with withDisabledUser = true
        var credOfDisabledUser = sdk.GetCredential(_testCreds[2].UserId!, _testCreds[2].CredentialId!, withDisabledUser: true);
        Assert.NotNull(credOfDisabledUser.Credential);
        Assert.Equal(_testCreds[2].CredentialId, credOfDisabledUser.Credential.CredentialId);
    }

    [Fact]
    public void Live_UpdateCredential_Success_And_NotFound_And_ParameterError_And_UpdateError()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;
        if (_testCreds.Count < 1) return;

        var sdk = CreateSdk();
        var targetCred = _testCreds[0];

        // 1. NOT_FOUND on non-existent user
        var notFoundUserPrm = new CredentialDataUpdateParameter(targetCred)
        {
            UserId = "non_existent_user_id"
        };
        var exUserNotFound = Assert.Throws<FssApiException>(() => sdk.UpdateCredential(notFoundUserPrm));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exUserNotFound.AppStatus);

        // 2. NOT_FOUND on non-existent credential
        var notFoundCredPrm = new CredentialDataUpdateParameter(targetCred)
        {
            CredentialId = "non_existent_cred_id"
        };
        var exCredNotFound = Assert.Throws<FssApiException>(() => sdk.UpdateCredential(notFoundCredPrm));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exCredNotFound.AppStatus);

        // 3. PARAMETER_ERROR on empty credential name
        var emptyNamePrm = new CredentialDataUpdateParameter(targetCred)
        {
            CredentialName = ""
        };
        var exEmptyName = Assert.Throws<FssApiException>(() => sdk.UpdateCredential(emptyNamePrm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exEmptyName.AppStatus);

        // 4. Normal Update
        var normalUpdatePrm = new CredentialDataUpdateParameter(targetCred)
        {
            CredentialName = "updated_cred_name",
            CredentialAttributes = new Dictionary<string, object> { ["cred-attr2"] = "val2" }
        };
        var updateRes = sdk.UpdateCredential(normalUpdatePrm);
        Assert.NotNull(updateRes.Credential);
        Assert.Equal("updated_cred_name", updateRes.Credential.CredentialName);

        // Null attributes update
        normalUpdatePrm.CredentialAttributes = null;
        var nullAttrUpdateRes = sdk.UpdateCredential(normalUpdatePrm);
        Assert.Null(nullAttrUpdateRes.Credential?.CredentialAttributes);
        sdk.UpdateCredential(targetCred);

        // 5. UPDATE_ERROR with optimistic lock (withUpdatedCheck = true with altered timestamp)
        var freshCred = sdk.GetCredential(targetCred.UserId!, targetCred.CredentialId!);
        var conflictPrm = new CredentialDataUpdateParameter(targetCred)
        {
            CredentialName = "conflict_name",
            Updated = DateTimeOffset.UtcNow.AddMinutes(-10) // altered past timestamp
        };
        var exUpdateError = Assert.Throws<FssApiException>(() => sdk.UpdateCredential(conflictPrm, withUpdatedCheck: true));
        Assert.Equal(FssApiResultStatus.UPDATE_ERROR, exUpdateError.AppStatus);

        // 6. Restore original state
        sdk.UpdateCredential(targetCred);
    }

    [Fact]
    public void Live_DeleteCredential_Success_And_NotFound()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;
        if (_testCreds.Count < 2) return;

        var sdk = CreateSdk();

        // 1. NOT_FOUND on non-existent user
        var exUserNotFound = Assert.Throws<FssApiException>(() =>
            sdk.DeleteCredential("non_existent_user_id", _testCreds[1].CredentialId!));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exUserNotFound.AppStatus);

        // 2. NOT_FOUND on non-existent credential
        var exCredNotFound = Assert.Throws<FssApiException>(() =>
            sdk.DeleteCredential(_testCreds[1].UserId!, "non_existent_cred_id"));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exCredNotFound.AppStatus);

        // 3. Normal Delete on _testCreds[1]
        var deleteRes = sdk.DeleteCredential(_testCreds[1].UserId!, _testCreds[1].CredentialId!);
        Assert.NotNull(deleteRes.Credential);
        Assert.Equal(_testCreds[1].CredentialId, deleteRes.Credential.CredentialId);

        // Confirm it is gone
        var exAfter = Assert.Throws<FssApiException>(() =>
            sdk.GetCredential(_testCreds[1].UserId!, _testCreds[1].CredentialId!, withDisabledCredential: true));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exAfter.AppStatus);
    }
}
