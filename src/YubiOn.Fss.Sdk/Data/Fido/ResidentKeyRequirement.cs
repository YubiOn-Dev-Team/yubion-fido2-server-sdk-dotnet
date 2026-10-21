using System.Text.Json;
using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Represents the resident key requirement.
/// </summary>
[JsonConverter(typeof(ResidentKeyRequirementConverter))]
public enum ResidentKeyRequirement
{
    Any,
    Discouraged,
    Preferred,
    Required,

    // Aliases
    ANY = Any,
    DISCOURAGED = Discouraged,
    PREFERRED = Preferred,
    REQUIRED = Required
}

public class ResidentKeyRequirementConverter : JsonConverter<ResidentKeyRequirement>
{
    public override ResidentKeyRequirement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str?.ToLowerInvariant() switch
        {
            "discouraged" => ResidentKeyRequirement.Discouraged,
            "preferred" => ResidentKeyRequirement.Preferred,
            "required" => ResidentKeyRequirement.Required,
            _ => ResidentKeyRequirement.Any
        };
    }

    public override void Write(Utf8JsonWriter writer, ResidentKeyRequirement value, JsonSerializerOptions options)
    {
        var str = value switch
        {
            ResidentKeyRequirement.Discouraged => "discouraged",
            ResidentKeyRequirement.Preferred => "preferred",
            ResidentKeyRequirement.Required => "required",
            _ => null
        };

        if (str == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(str);
    }
}
