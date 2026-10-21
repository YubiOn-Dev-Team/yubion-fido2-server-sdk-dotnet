using System.Text.Json;
using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Represents the attestation conveyance preference.
/// </summary>
[JsonConverter(typeof(AttestationConveyancePreferenceConverter))]
public enum AttestationConveyancePreference
{
    None,
    Indirect,
    Direct,
    Enterprise,

    // Aliases
    NONE = None,
    INDIRECT = Indirect,
    DIRECT = Direct,
    ENTERPRISE = Enterprise
}

public class AttestationConveyancePreferenceConverter : JsonConverter<AttestationConveyancePreference>
{
    public override AttestationConveyancePreference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str?.ToLowerInvariant() switch
        {
            "none" => AttestationConveyancePreference.None,
            "indirect" => AttestationConveyancePreference.Indirect,
            "direct" => AttestationConveyancePreference.Direct,
            "enterprise" => AttestationConveyancePreference.Enterprise,
            _ => AttestationConveyancePreference.None
        };
    }

    public override void Write(Utf8JsonWriter writer, AttestationConveyancePreference value, JsonSerializerOptions options)
    {
        var str = value switch
        {
            AttestationConveyancePreference.None => "none",
            AttestationConveyancePreference.Indirect => "indirect",
            AttestationConveyancePreference.Direct => "direct",
            AttestationConveyancePreference.Enterprise => "enterprise",
            _ => "none"
        };
        writer.WriteStringValue(str);
    }
}
