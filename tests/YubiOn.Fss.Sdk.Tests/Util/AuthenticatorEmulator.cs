using System.Formats.Cbor;
using System.Security.Cryptography;
using System.Text;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Internal;

namespace YubiOn.Fss.Sdk.Tests.Util;

public class AuthenticatorEmulator
{
    private class CredentialEntry
    {
        public string CredId { get; }
        public bool ResidentKey { get; }
        public ECDsa Ecdsa { get; }
        public string? UserId { get; }

        public CredentialEntry(string credId, bool residentKey, ECDsa ecdsa, string? userId)
        {
            CredId = credId;
            ResidentKey = residentKey;
            Ecdsa = ecdsa;
            UserId = userId;
        }
    }

    private readonly string _rpId;
    private readonly Dictionary<string, CredentialEntry> _credentials = new();

    public AuthenticatorEmulator(string rpId)
    {
        _rpId = rpId ?? throw new ArgumentNullException(nameof(rpId));
    }

    public PublicKeyCredential MakeCred(string origin, PublicKeyCredentialCreationOptionsJson options)
    {
        if (options.Rp?.Id != _rpId)
        {
            throw new ArgumentException("rpId does not match");
        }

        bool residentKey = false;
        if (options.AuthenticatorSelection != null)
        {
            var sel = options.AuthenticatorSelection;
            if (sel.RequireResidentKey == true)
            {
                residentKey = true;
            }
            if (sel.ResidentKey == ResidentKeyRequirement.Required || sel.ResidentKey == ResidentKeyRequirement.Preferred)
            {
                residentKey = true;
            }
        }

        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        byte[] credIdBytes = new byte[16];
        string credIdStr;
        do
        {
            RandomNumberGenerator.Fill(credIdBytes);
            credIdStr = SignAuthApiRequesterBase.Base64UrlEncode(credIdBytes);
        } while (_credentials.ContainsKey(credIdStr));

        var entry = new CredentialEntry(credIdStr, residentKey, ecdsa, options.User?.Id);
        _credentials[credIdStr] = entry;

        var ret = new PublicKeyCredential
        {
            Id = credIdStr,
            RawId = credIdStr,
            Type = "public-key"
        };

        var attestationResp = new AuthenticatorAttestationResponse();

        using var authDataMs = new MemoryStream();
        using var authDataBw = new BinaryWriter(authDataMs);

        byte[] rpIdHash = SHA256.HashData(Encoding.UTF8.GetBytes(_rpId));
        authDataBw.Write(rpIdHash);
        authDataBw.Write((byte)0x45); // UP, UV, AT
        // signCount (4 bytes, big endian)
        authDataBw.Write(new byte[] { 0, 0, 0, 0 });
        // AAGUID (16 bytes)
        authDataBw.Write(new byte[16]);
        // credIdLen (2 bytes, big endian: 16)
        authDataBw.Write((byte)0);
        authDataBw.Write((byte)16);
        // credId
        authDataBw.Write(credIdBytes);
        // cose key
        byte[] coseKeyBytes = PublicKeyToCoseKeyConverter.ToCoseKeyBytes(ecdsa);
        authDataBw.Write(coseKeyBytes);

        byte[] authData = authDataMs.ToArray();

        // CBOR attestation object
        var cborWriter = new CborWriter();
        cborWriter.WriteStartMap(3);
        cborWriter.WriteTextString("fmt");
        cborWriter.WriteTextString("none");
        cborWriter.WriteTextString("attStmt");
        cborWriter.WriteStartMap(0);
        cborWriter.WriteEndMap();
        cborWriter.WriteTextString("authData");
        cborWriter.WriteByteString(authData);
        cborWriter.WriteEndMap();

        byte[] attestationObjBytes = cborWriter.Encode();
        attestationResp.AttestationObject = SignAuthApiRequesterBase.Base64UrlEncode(attestationObjBytes);

        string clientDataJson = $"{{\"type\":\"webauthn.create\",\"challenge\":\"{options.Challenge}\",\"origin\":\"{origin}\"}}";
        attestationResp.ClientDataJson = SignAuthApiRequesterBase.Base64UrlEncode(Encoding.UTF8.GetBytes(clientDataJson));

        ret.Response = attestationResp;
        return ret;
    }

    public PublicKeyCredentialForAuthentication GetAssr(string origin, PublicKeyCredentialRequestOptionsJson options)
    {
        if (options.RpId != _rpId)
        {
            throw new ArgumentException("rpId does not match");
        }

        CredentialEntry? cred = null;
        if (options.AllowCredentials != null && options.AllowCredentials.Length > 0)
        {
            var allowedIds = new HashSet<string>(options.AllowCredentials.Select(c => c.Id!));
            cred = _credentials.Values.FirstOrDefault(c => allowedIds.Contains(c.CredId));
        }
        else
        {
            cred = _credentials.Values.FirstOrDefault();
        }

        if (cred == null)
        {
            throw new ArgumentException("allowCredentials does not match");
        }

        var ret = new PublicKeyCredentialForAuthentication
        {
            Id = cred.CredId,
            RawId = cred.CredId,
            Type = "public-key"
        };

        var assertionResp = new AuthenticatorAssertionResponse();
        string clientDataJson = $"{{\"type\":\"webauthn.get\",\"challenge\":\"{options.Challenge}\",\"origin\":\"{origin}\"}}";
        byte[] clientDataJsonBin = Encoding.UTF8.GetBytes(clientDataJson);
        assertionResp.ClientDataJson = SignAuthApiRequesterBase.Base64UrlEncode(clientDataJsonBin);
        assertionResp.UserHandle = cred.UserId;

        using var authDataMs = new MemoryStream();
        using var authDataBw = new BinaryWriter(authDataMs);
        byte[] rpIdHash = SHA256.HashData(Encoding.UTF8.GetBytes(_rpId));
        authDataBw.Write(rpIdHash);
        authDataBw.Write((byte)0x05); // UP, UV
        authDataBw.Write(new byte[] { 0, 0, 0, 0 }); // signcount (4 bytes, big endian)

        byte[] authDataBin = authDataMs.ToArray();
        assertionResp.AuthenticatorData = SignAuthApiRequesterBase.Base64UrlEncode(authDataBin);

        byte[] clientDataJsonHash = SHA256.HashData(clientDataJsonBin);
        byte[] signTarget = new byte[authDataBin.Length + clientDataJsonHash.Length];
        Buffer.BlockCopy(authDataBin, 0, signTarget, 0, authDataBin.Length);
        Buffer.BlockCopy(clientDataJsonHash, 0, signTarget, authDataBin.Length, clientDataJsonHash.Length);

        // WebAuthn assertion signature is ASN.1 DER (Rfc3279DerSequence)
        byte[] signatureBytes = cred.Ecdsa.SignData(signTarget, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
        assertionResp.Signature = SignAuthApiRequesterBase.Base64UrlEncode(signatureBytes);

        ret.Response = assertionResp;
        return ret;
    }
}
