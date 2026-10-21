using System.Text.Json;
using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Represents the authenticator transport methods.
/// </summary>
[JsonConverter(typeof(AuthenticatorTransportConverter))]
public enum AuthenticatorTransport
{
    Usb,
    Nfc,
    Ble,
    Hybrid,
    Internal,

    // Aliases
    USB = Usb,
    NFC = Nfc,
    BLE = Ble,
    HYBRID = Hybrid,
    INTERNAL = Internal
}

public class AuthenticatorTransportConverter : JsonConverter<AuthenticatorTransport>
{
    public override AuthenticatorTransport Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str?.ToLowerInvariant() switch
        {
            "usb" => AuthenticatorTransport.Usb,
            "nfc" => AuthenticatorTransport.Nfc,
            "ble" => AuthenticatorTransport.Ble,
            "hybrid" => AuthenticatorTransport.Hybrid,
            "internal" => AuthenticatorTransport.Internal,
            _ => AuthenticatorTransport.Usb
        };
    }

    public override void Write(Utf8JsonWriter writer, AuthenticatorTransport value, JsonSerializerOptions options)
    {
        var str = value switch
        {
            AuthenticatorTransport.Usb => "usb",
            AuthenticatorTransport.Nfc => "nfc",
            AuthenticatorTransport.Ble => "ble",
            AuthenticatorTransport.Hybrid => "hybrid",
            AuthenticatorTransport.Internal => "internal",
            _ => "usb"
        };
        writer.WriteStringValue(str);
    }
}
