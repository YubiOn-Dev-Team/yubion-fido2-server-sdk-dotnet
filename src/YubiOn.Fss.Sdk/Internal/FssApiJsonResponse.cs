using System.Text.Json;
using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Result;

namespace YubiOn.Fss.Sdk.Internal;

/// <summary>
/// Represents a generic JSON response envelope from the FSS API.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
public class FssApiJsonResponse<T>
{
    [JsonPropertyName("appStatus")]
    public FssApiResultStatus AppStatus { get; set; } = FssApiResultStatus.OK;

    [JsonPropertyName("appSubStatus")]
    public JsonElement? AppSubStatus { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    // Java-compatible accessors
    public FssApiResultStatus GetAppStatus() => AppStatus;
    public JsonElement? GetAppSubStatus() => AppSubStatus;
    public T? GetData() => Data;
    public string? GetMessage() => Message;
}
