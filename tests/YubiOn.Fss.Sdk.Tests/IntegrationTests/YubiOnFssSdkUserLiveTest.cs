using System.Security.Cryptography;
using YubiOn.Fss.Sdk.Data.User;
using YubiOn.Fss.Sdk.Result;
using Xunit;

namespace YubiOn.Fss.Sdk.Tests.IntegrationTests;

/// <summary>
/// Comprehensive live integration tests for User API endpoints matching Java and JS SDK test suites.
/// Verifies DUPLICATED, UPDATE_ERROR, NOT_FOUND, PARAMETER_ERROR, ALREADY_EXISTS, and optimistic locking.
/// </summary>
[Collection("FssLiveTest")]
public class YubiOnFssSdkUserLiveTest : IDisposable
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
    private readonly UserDataRegisterParameter _baseUserPrm;

    public YubiOnFssSdkUserLiveTest()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials())
        {
            _testUsers = Array.Empty<UserDataRegisterParameter>();
            _baseUserPrm = new UserDataRegisterParameter("", "", null);
            return;
        }

        _testUsers = new[]
        {
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)i),
                "live_user1@example.com",
                "user 1",
                new Dictionary<string, object> { ["attr1"] = "user1" },
                false
            ),
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 1)),
                "live_user2@example.com",
                "user 2",
                new Dictionary<string, object> { ["attr1"] = "user2" },
                false
            ),
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 2)),
                "live_user3@example.com",
                "user 3",
                new Dictionary<string, object> { ["attr1"] = "user3" },
                false
            ),
            new UserDataRegisterParameter(
                MakeUserId(i => (byte)(i + 3)),
                "live_user4@example.com",
                "user 4",
                new Dictionary<string, object> { ["attr1"] = "user4" },
                true
            ),
        };

        _baseUserPrm = new UserDataRegisterParameter(
            MakeUserId(i => (byte)(i + 10)),
            "live_register_test@example.com",
            "register user",
            new Dictionary<string, object> { ["attr1"] = "user register" },
            false
        );

        SetUp();
    }

    private void SetUp()
    {
        var sdk = CreateSdk();

        // Cleanup any existing test users
        try
        {
            var users = sdk.GetAllUsers(true);
            var testUserIds = _testUsers.Select(u => u.UserId).ToHashSet();
            testUserIds.Add(_baseUserPrm.UserId);
            testUserIds.Add(MakeUserId(i => (byte)(i + 20)));

            if (users.Users != null)
            {
                foreach (var user in users.Users)
                {
                    if (user.UserId != null && (testUserIds.Contains(user.UserId) || user.UserName?.StartsWith("live_") == true))
                    {
                        try { sdk.DeleteUser(user.UserId); } catch { }
                    }
                }
            }
        }
        catch { }

        // Register initial test users
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
            var users = sdk.GetAllUsers(true);
            if (users.Users != null)
            {
                foreach (var user in users.Users)
                {
                    if (user.UserName?.StartsWith("live_") == true && user.UserId != null)
                    {
                        try { sdk.DeleteUser(user.UserId); } catch { }
                    }
                }
            }
        }
        catch { }
    }

    [Fact]
    public void Live_GetAllUsers_MatchesJavaAndJsSuite()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;

        var sdk = CreateSdk();

        // 1. Default (Disabled users are excluded)
        var resultDefault = sdk.GetAllUsers();
        Assert.NotNull(resultDefault?.Users);
        Assert.DoesNotContain(resultDefault.Users, u => u.Disabled == true && u.UserName?.StartsWith("live_") == true);

        // 2. With withDisabledUser = true
        var resultWithDisabled = sdk.GetAllUsers(withDisabledUser: true);
        Assert.NotNull(resultWithDisabled?.Users);

        foreach (var expectedUser in _testUsers)
        {
            var found = resultWithDisabled.Users.FirstOrDefault(u => u.UserId == expectedUser.UserId);
            Assert.NotNull(found);
            Assert.Equal(expectedUser.UserName, found.UserName);
            Assert.Equal(expectedUser.DisplayName, found.DisplayName);
            Assert.Equal(expectedUser.Disabled, found.Disabled);
        }
    }

    [Fact]
    public void Live_GetUser_Success_And_NotFound()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;

        var sdk = CreateSdk();

        // 1. Normal active user
        var res1 = sdk.GetUser(_testUsers[0].UserId!);
        Assert.NotNull(res1.User);
        Assert.Equal(_testUsers[0].UserId, res1.User.UserId);
        Assert.Equal(_testUsers[0].UserName, res1.User.UserName);
        Assert.Equal(_testUsers[0].DisplayName, res1.User.DisplayName);
        Assert.False(res1.User.Disabled);

        // 2. Disabled user with withDisabledUser = true
        var resDisabled = sdk.GetUser(_testUsers[3].UserId!, withDisabledUser: true);
        Assert.NotNull(resDisabled.User);
        Assert.True(resDisabled.User.Disabled);

        // 3. Disabled user without withDisabledUser should throw NOT_FOUND
        var exNotFound1 = Assert.Throws<FssApiException>(() => sdk.GetUser(_testUsers[3].UserId!, withDisabledUser: false));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exNotFound1.AppStatus);

        // 4. Non-existent user should throw NOT_FOUND
        var nonExistentId = MakeUserId(i => (byte)(i + 99));
        var exNotFound2 = Assert.Throws<FssApiException>(() => sdk.GetUser(nonExistentId, withDisabledUser: true));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exNotFound2.AppStatus);
    }

    [Fact]
    public void Live_GetUsersByUserName_Success_And_NotFound()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;

        var sdk = CreateSdk();

        // 1. Normal active user
        var res1 = sdk.GetUsersByUserName(_testUsers[0].UserName!);
        Assert.NotNull(res1.Users);
        Assert.Single(res1.Users);
        Assert.Equal(_testUsers[0].UserId, res1.Users[0].UserId);

        // 2. Disabled user with withDisabledUser = true
        var resDisabled = sdk.GetUsersByUserName(_testUsers[3].UserName!, withDisabledUser: true);
        Assert.NotNull(resDisabled.Users);
        Assert.Single(resDisabled.Users);
        Assert.True(resDisabled.Users[0].Disabled);

        // 3. Disabled user without withDisabledUser should throw NOT_FOUND
        var ex1 = Assert.Throws<FssApiException>(() => sdk.GetUsersByUserName(_testUsers[3].UserName!, withDisabledUser: false));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, ex1.AppStatus);

        // 4. Non-existent userName should throw NOT_FOUND
        var ex2 = Assert.Throws<FssApiException>(() => sdk.GetUsersByUserName("non_existent_live_user@example.com", withDisabledUser: true));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, ex2.AppStatus);
    }

    [Fact]
    public void Live_RegisterUser_Success_And_Duplicated_And_ParameterErrors()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;

        var sdk = CreateSdk();

        // 1. Normal registration and cleanup
        var regRes = sdk.RegisterUser(_baseUserPrm);
        Assert.NotNull(regRes.User);
        Assert.Equal(_baseUserPrm.UserId, regRes.User.UserId);
        Assert.Equal(_baseUserPrm.UserName, regRes.User.UserName);
        sdk.DeleteUser(_baseUserPrm.UserId!);

        // 2. Duplicated ID -> ALREADY_EXISTS (existing userId with unique userName)
        var dupIdPrm = new UserDataRegisterParameter(_baseUserPrm)
        {
            UserId = _testUsers[0].UserId,
            UserName = "live_unique_for_dup_id@example.com"
        };
        var exAlreadyExists = Assert.Throws<FssApiException>(() => sdk.RegisterUser(dupIdPrm));
        Assert.Equal(FssApiResultStatus.ALREADY_EXISTS, exAlreadyExists.AppStatus);

        // 3. Duplicated User Name -> DUPLICATED (unique userId with existing userName)
        var dupNamePrm = new UserDataRegisterParameter(_baseUserPrm)
        {
            UserId = MakeUserId(i => (byte)(i + 20)),
            UserName = _testUsers[0].UserName // duplicate of existing user1
        };
        var exDuplicated = Assert.Throws<FssApiException>(() => sdk.RegisterUser(dupNamePrm));
        Assert.Equal(FssApiResultStatus.DUPLICATED, exDuplicated.AppStatus);

        // 4. Parameter Errors
        // Empty ID
        var emptyIdPrm = new UserDataRegisterParameter(_baseUserPrm) { UserId = "" };
        var exEmptyId = Assert.Throws<FssApiException>(() => sdk.RegisterUser(emptyIdPrm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exEmptyId.AppStatus);

        // Non-Base64URL ID
        var invalidB64Prm = new UserDataRegisterParameter(_baseUserPrm) { UserId = "!!!" };
        var exInvalidB64 = Assert.Throws<FssApiException>(() => sdk.RegisterUser(invalidB64Prm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exInvalidB64.AppStatus);

        // Too long ID (> 64 bytes)
        var tooLongIdPrm = new UserDataRegisterParameter(_baseUserPrm) { UserId = MakeUserId(i => (byte)(i + 10), 65) };
        var exTooLong = Assert.Throws<FssApiException>(() => sdk.RegisterUser(tooLongIdPrm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exTooLong.AppStatus);

        // Too long ID (2) (139 character string)
        var tooLongId2Prm = new UserDataRegisterParameter(_baseUserPrm)
        {
            UserId = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789"
        };
        var exTooLong2 = Assert.Throws<FssApiException>(() => sdk.RegisterUser(tooLongId2Prm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exTooLong2.AppStatus);

        // Empty Name
        var emptyNamePrm = new UserDataRegisterParameter(_baseUserPrm) { UserId = MakeUserId(i => (byte)(i + 25)), UserName = "" };
        var exEmptyName = Assert.Throws<FssApiException>(() => sdk.RegisterUser(emptyNamePrm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exEmptyName.AppStatus);

        // 5. Null DisplayName registration
        var nullDisplayPrm = new UserDataRegisterParameter(MakeUserId(i => (byte)(i + 26)), "live_null_disp@example.com", null);
        var nullDisplayRes = sdk.RegisterUser(nullDisplayPrm);
        Assert.NotNull(nullDisplayRes.User);
        Assert.Null(nullDisplayRes.User.DisplayName);
        sdk.DeleteUser(nullDisplayPrm.UserId!);

        // 6. Null UserAttributes registration
        var nullAttrPrm = new UserDataRegisterParameter(MakeUserId(i => (byte)(i + 27)), "live_null_attr@example.com", "display", null, false);
        var nullAttrRes = sdk.RegisterUser(nullAttrPrm);
        Assert.NotNull(nullAttrRes.User);
        Assert.Null(nullAttrRes.User.UserAttributes);
        sdk.DeleteUser(nullAttrPrm.UserId!);
    }

    [Fact]
    public void Live_UpdateUser_Success_And_Duplicated_And_UpdateError()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;

        var sdk = CreateSdk();

        // Register base user for updating
        sdk.RegisterUser(_baseUserPrm);

        // 1. NOT_FOUND on non-existent user
        var nonExistentPrm = new UserDataUpdateParameter(MakeUserId(i => (byte)(i + 95)), "noone@example.com");
        var exNotFound = Assert.Throws<FssApiException>(() => sdk.UpdateUser(nonExistentPrm));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exNotFound.AppStatus);

        // 2. Empty Name -> PARAMETER_ERROR
        var emptyNamePrm = new UserDataUpdateParameter(_baseUserPrm.UserId, "");
        var exParam = Assert.Throws<FssApiException>(() => sdk.UpdateUser(emptyNamePrm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exParam.AppStatus);

        // Empty ID -> PARAMETER_ERROR
        var emptyIdPrm = new UserDataUpdateParameter("", "valid@example.com");
        var exEmptyId = Assert.Throws<FssApiException>(() => sdk.UpdateUser(emptyIdPrm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exEmptyId.AppStatus);

        // Non-Base64URL ID -> PARAMETER_ERROR
        var invalidB64Prm = new UserDataUpdateParameter("!!!", "valid@example.com");
        var exInvalidB64 = Assert.Throws<FssApiException>(() => sdk.UpdateUser(invalidB64Prm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exInvalidB64.AppStatus);

        // Too long ID (> 64 bytes) -> PARAMETER_ERROR
        var tooLongIdPrm = new UserDataUpdateParameter(MakeUserId(i => (byte)(i + 20), 65), "valid@example.com");
        var exTooLong = Assert.Throws<FssApiException>(() => sdk.UpdateUser(tooLongIdPrm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exTooLong.AppStatus);

        // Too long ID (2) (139 character string) -> PARAMETER_ERROR
        var tooLongId2Prm = new UserDataUpdateParameter("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", "valid@example.com");
        var exTooLong2 = Assert.Throws<FssApiException>(() => sdk.UpdateUser(tooLongId2Prm));
        Assert.Equal(FssApiResultStatus.PARAMETER_ERROR, exTooLong2.AppStatus);

        // 3. Duplicated User Name -> DUPLICATED
        // Try updating _baseUserPrm's userName to _testUsers[0]'s userName
        var dupNamePrm = new UserDataUpdateParameter(_baseUserPrm.UserId, _testUsers[0].UserName);
        var exDup = Assert.Throws<FssApiException>(() => sdk.UpdateUser(dupNamePrm));
        Assert.Equal(FssApiResultStatus.DUPLICATED, exDup.AppStatus);

        // 4. Normal Update (DisplayName, Attributes, Disabled)
        var updatePrm = new UserDataUpdateParameter(_baseUserPrm.UserId, _baseUserPrm.UserName)
        {
            DisplayName = "Updated Display Name",
            UserAttributes = new Dictionary<string, object> { ["attr2"] = "val2" },
            Disabled = true
        };
        var updateRes = sdk.UpdateUser(updatePrm);
        Assert.NotNull(updateRes.User);
        Assert.Equal("Updated Display Name", updateRes.User.DisplayName);
        Assert.True(updateRes.User.Disabled);

        // Null DisplayName update
        updatePrm.DisplayName = null;
        var nullDispUpdateRes = sdk.UpdateUser(updatePrm);
        Assert.Null(nullDispUpdateRes.User?.DisplayName);
        updatePrm.DisplayName = "Restored Display Name";
        sdk.UpdateUser(updatePrm);

        // Null UserAttributes update
        updatePrm.UserAttributes = null;
        var nullAttrUpdateRes = sdk.UpdateUser(updatePrm);
        Assert.Null(nullAttrUpdateRes.User?.UserAttributes);

        // Disabled toggling (true -> false)
        updatePrm.Disabled = false;
        var falseDisabledRes = sdk.UpdateUser(updatePrm);
        Assert.False(falseDisabledRes.User?.Disabled);

        // 5. UPDATE_ERROR with optimistic lock (withUpdatedCheck = true with altered timestamp)
        var userCurrent = sdk.GetUser(_baseUserPrm.UserId!, withDisabledUser: true);
        var conflictPrm = new UserDataUpdateParameter(userCurrent.User!)
        {
            Updated = DateTimeOffset.UtcNow.AddMinutes(-10) // altered past timestamp
        };
        var exUpdateError = Assert.Throws<FssApiException>(() => sdk.UpdateUser(conflictPrm, withUpdatedCheck: true));
        Assert.Equal(FssApiResultStatus.UPDATE_ERROR, exUpdateError.AppStatus);

        // 6. Normal update recovery (matches Java SDK check updated (success))
        var userFresh = sdk.GetUser(_baseUserPrm.UserId!, withDisabledUser: true);
        var freshPrm = new UserDataUpdateParameter(userFresh.User!)
        {
            DisplayName = "Fresh Display Name"
        };
        var freshRes = sdk.UpdateUser(freshPrm);
        Assert.Equal("Fresh Display Name", freshRes.User?.DisplayName);

        // Cleanup base user
        sdk.DeleteUser(_baseUserPrm.UserId!);
    }

    [Fact]
    public void Live_DeleteUser_Success_And_NotFound()
    {
        if (!YubiOnFssSdkTestConfig.HasLiveTestCredentials()) return;

        var sdk = CreateSdk();

        // 1. NOT_FOUND on non-existent user
        var nonExistentId = MakeUserId(i => (byte)(i + 98));
        var exNotFound = Assert.Throws<FssApiException>(() => sdk.DeleteUser(nonExistentId));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exNotFound.AppStatus);

        // 2. Normal Delete
        var tempUserPrm = new UserDataRegisterParameter(
            MakeUserId(i => (byte)(i + 35)),
            "live_to_delete@example.com",
            "to delete",
            null,
            false
        );
        sdk.RegisterUser(tempUserPrm);

        var delRes = sdk.DeleteUser(tempUserPrm.UserId!);
        Assert.NotNull(delRes.User);
        Assert.Equal(tempUserPrm.UserId, delRes.User.UserId);

        // Confirm it is gone
        var exAfter = Assert.Throws<FssApiException>(() => sdk.GetUser(tempUserPrm.UserId!));
        Assert.Equal(FssApiResultStatus.NOT_FOUND, exAfter.AppStatus);
    }
}
