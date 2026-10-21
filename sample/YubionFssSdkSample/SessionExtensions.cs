using System.Text.Json;

namespace YubiOn.Fss.Sdk.Sample;

/// <summary>
/// Session helper extensions for typed object storage.
/// </summary>
public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
