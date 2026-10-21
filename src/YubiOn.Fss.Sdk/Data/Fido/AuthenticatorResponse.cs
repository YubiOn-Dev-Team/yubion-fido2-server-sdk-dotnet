using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Base class for authenticator responses.
/// </summary>
public class AuthenticatorResponse
{
    [JsonPropertyName("clientDataJSON")]
    public string? ClientDataJSON { get; set; }

    [JsonIgnore]
    public string? ClientDataJson
    {
        get => ClientDataJSON;
        set => ClientDataJSON = value;
    }
}
