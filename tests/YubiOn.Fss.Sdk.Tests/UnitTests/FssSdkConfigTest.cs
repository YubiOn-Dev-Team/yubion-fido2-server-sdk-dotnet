using YubiOn.Fss.Sdk.Config;
using Xunit;

namespace YubiOn.Fss.Sdk.Tests.UnitTests;

public class FssSdkConfigTest
{
    [Fact]
    public void DefaultConfig_HasExpectedDefaults()
    {
        var config = new FssSdkConfig();
        Assert.Equal("https://fss-app.yubion.com/api/", config.Endpoint);
        Assert.Equal(FssApiAuthType.NonceSignAuth, config.ApiAuthType);
        Assert.Equal("yubion-fido2-server-sdk-dotnet", config.Agent);
    }

    [Fact]
    public void ParametrizedConstructor_SetsPropertiesCorrectly()
    {
        var config = new FssSdkConfig(
            rpId: "test-rp.example.com",
            apiAuthId: "auth-123",
            apiAuthType: FssApiAuthType.DatetimeSignAuth,
            secretKey: "secret-key-abc");

        Assert.Equal("test-rp.example.com", config.RpId);
        Assert.Equal("auth-123", config.ApiAuthId);
        Assert.Equal(FssApiAuthType.DatetimeSignAuth, config.ApiAuthType);
        Assert.Equal("secret-key-abc", config.SecretKey);
        Assert.Equal("https://fss-app.yubion.com/api/", config.Endpoint);
    }

    [Fact]
    public void JavaCompatibleConstructor_WithStrings_SetsEnumCorrectly()
    {
        var config = new FssSdkConfig(
            "test-rp.example.com",
            "auth-123",
            "NONCE_SIGN_AUTH",
            "key-1");

        Assert.Equal(FssApiAuthType.NonceSignAuth, config.ApiAuthType);
    }
}
