using System.Formats.Cbor;
using System.Security.Cryptography;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Tests.Util;
using Xunit;

namespace YubiOn.Fss.Sdk.Tests.UnitTests;

public class AuthenticatorEmulatorTest
{
    [Fact]
    public void PublicKeyToCoseKeyConverter_EncodesValidCbor()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        byte[] coseBytes = PublicKeyToCoseKeyConverter.ToCoseKeyBytes(ecdsa);

        Assert.NotNull(coseBytes);
        Assert.NotEmpty(coseBytes);

        var reader = new CborReader(coseBytes);
        int? count = reader.ReadStartMap();
        Assert.Equal(5, count);

        // Verify keys
        var dict = new Dictionary<int, object>();
        for (int i = 0; i < 5; i++)
        {
            int key = reader.ReadInt32();
            if (key == 1 || key == 3 || key == -1)
            {
                dict[key] = reader.ReadInt32();
            }
            else
            {
                dict[key] = reader.ReadByteString();
            }
        }
        reader.ReadEndMap();

        Assert.Equal(PublicKeyToCoseKeyConverter.CoseKtyEc2, (int)dict[1]);
        Assert.Equal(PublicKeyToCoseKeyConverter.CoseCrvP256, (int)dict[-1]);
        Assert.Equal(PublicKeyToCoseKeyConverter.CoseAlgEs256, (int)dict[3]);
        Assert.Equal(32, ((byte[])dict[-2]).Length);
        Assert.Equal(32, ((byte[])dict[-3]).Length);
    }

    [Fact]
    public void MakeCredAndGetAssr_GeneratesValidResponses()
    {
        string rpId = "example.com";
        var emulator = new AuthenticatorEmulator(rpId);

        // 1. Make Credential
        var createOptions = new PublicKeyCredentialCreationOptionsJson
        {
            Rp = new PublicKeyCredentialRpEntity { Id = rpId, Name = "Example RP" },
            User = new PublicKeyCredentialUserEntityJson { Id = "dXNlcjEyMw", Name = "alice@example.com", DisplayName = "Alice" },
            Challenge = "Y2hhbGxlbmdlMTIz",
            PubKeyCredParams = new PublicKeyCredentialParameters[]
            {
                new PublicKeyCredentialParameters { Type = "public-key", Alg = -7 }
            }
        };

        var cred = emulator.MakeCred("https://" + rpId, createOptions);
        Assert.NotNull(cred);
        Assert.NotNull(cred.Id);
        Assert.NotNull(cred.Response);
        var attResp = Assert.IsType<AuthenticatorAttestationResponse>(cred.Response);
        Assert.NotNull(attResp.AttestationObject);
        Assert.NotNull(attResp.ClientDataJson);

        // 2. Get Assertion
        var requestOptions = new PublicKeyCredentialRequestOptionsJson
        {
            RpId = rpId,
            Challenge = "YXV0aGNoYWxsZW5nZTEyMw",
            AllowCredentials = new PublicKeyCredentialDescriptorJson[]
            {
                new PublicKeyCredentialDescriptorJson { Type = "public-key", Id = cred.Id }
            }
        };

        var authCred = emulator.GetAssr("https://" + rpId, requestOptions);
        Assert.NotNull(authCred);
        Assert.Equal(cred.Id, authCred.Id);
        var assrResp = Assert.IsType<AuthenticatorAssertionResponse>(authCred.Response);
        Assert.NotNull(assrResp.AuthenticatorData);
        Assert.NotNull(assrResp.Signature);
        Assert.NotNull(assrResp.ClientDataJson);
    }
}
