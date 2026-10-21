using System.Formats.Cbor;
using System.Security.Cryptography;

namespace YubiOn.Fss.Sdk.Tests.Util;

public static class PublicKeyToCoseKeyConverter
{
    public const int CoseKtyEc2 = 2;
    public const int CoseAlgEs256 = -7;
    public const int CoseCrvP256 = 1;

    /// <summary>
    /// Converts an ECDSA P-256 public key to COSE Key CBOR bytes.
    /// </summary>
    public static byte[] ToCoseKeyBytes(ECDsa ecdsa)
    {
        var parameters = ecdsa.ExportParameters(false);
        if (parameters.Q.X == null || parameters.Q.Y == null)
        {
            throw new ArgumentException("ECDsa public key coordinates cannot be null.", nameof(ecdsa));
        }

        var writer = new CborWriter();
        writer.WriteStartMap(5);

        // 1: kty = 2 (EC2)
        writer.WriteInt32(1);
        writer.WriteInt32(CoseKtyEc2);

        // 3: alg = -7 (ES256)
        writer.WriteInt32(3);
        writer.WriteInt32(CoseAlgEs256);

        // -1: crv = 1 (P-256)
        writer.WriteInt32(-1);
        writer.WriteInt32(CoseCrvP256);

        // -2: x coordinate
        writer.WriteInt32(-2);
        writer.WriteByteString(parameters.Q.X);

        // -3: y coordinate
        writer.WriteInt32(-3);
        writer.WriteByteString(parameters.Q.Y);

        writer.WriteEndMap();
        return writer.Encode();
    }
}
