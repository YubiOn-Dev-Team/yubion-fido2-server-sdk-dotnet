using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Sample.Models;

/// <summary>
/// Response for checking current session state.
/// </summary>
public class SessionStateResponse
{
    [JsonPropertyName("loggedIn")]
    public bool LoggedIn { get; set; }

    [JsonPropertyName("user")]
    public SessionUserDto? User { get; set; }
}
