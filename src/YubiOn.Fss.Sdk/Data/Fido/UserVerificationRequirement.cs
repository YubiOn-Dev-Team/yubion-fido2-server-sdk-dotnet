using System.Text.Json;
using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Represents the user verification requirement.
/// </summary>
[JsonConverter(typeof(UserVerificationRequirementConverter))]
public enum UserVerificationRequirement
{
    Required,
    Preferred,
    Discouraged,

    // Aliases
    REQUIRED = Required,
    PREFERRED = Preferred,
    DISCOURAGED = Discouraged
}

public class UserVerificationRequirementConverter : JsonConverter<UserVerificationRequirement>
{
    public override UserVerificationRequirement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str?.ToLowerInvariant() switch
        {
            "required" => UserVerificationRequirement.Required,
            "preferred" => UserVerificationRequirement.Preferred,
            "discouraged" => UserVerificationRequirement.Discouraged,
            _ => UserVerificationRequirement.Preferred
        };
    }

    public override void Write(Utf8JsonWriter writer, UserVerificationRequirement value, JsonSerializerOptions options)
    {
        var str = value switch
        {
            UserVerificationRequirement.Required => "required",
            UserVerificationRequirement.Preferred => "preferred",
            UserVerificationRequirement.Discouraged => "discouraged",
            _ => "preferred"
        };
        writer.WriteStringValue(str);
    }
}
