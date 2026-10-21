using System.Text.Json;
using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Represents the authenticator attachment modality.
/// </summary>
[JsonConverter(typeof(AuthenticatorAttachmentConverter))]
public enum AuthenticatorAttachment
{
    Any,
    Platform,
    CrossPlatform,

    // Aliases
    ANY = Any,
    PLATFORM = Platform,
    CROSS_PLATFORM = CrossPlatform
}

public class AuthenticatorAttachmentConverter : JsonConverter<AuthenticatorAttachment>
{
    public override AuthenticatorAttachment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str?.ToLowerInvariant() switch
        {
            "platform" => AuthenticatorAttachment.Platform,
            "cross-platform" => AuthenticatorAttachment.CrossPlatform,
            _ => AuthenticatorAttachment.Any
        };
    }

    public override void Write(Utf8JsonWriter writer, AuthenticatorAttachment value, JsonSerializerOptions options)
    {
        var str = value switch
        {
            AuthenticatorAttachment.Platform => "platform",
            AuthenticatorAttachment.CrossPlatform => "cross-platform",
            _ => null
        };

        if (str == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(str);
    }
}
