using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Defines the parameters for initiating a FIDO2 authentication process.
/// </summary>
public class Fido2StartAuthenticateParameter
{
    [JsonPropertyName("requestOptionsBase")]
    public Fido2RequestOptionsBase? RequestOptionsBase { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("options")]
    public Fido2AuthenticateOptions? Options { get; set; }

    public Fido2StartAuthenticateParameter()
    {
    }

    public Fido2StartAuthenticateParameter(Fido2RequestOptionsBase? requestOptionsBase)
    {
        RequestOptionsBase = requestOptionsBase;
    }

    public Fido2StartAuthenticateParameter(Fido2RequestOptionsBase? requestOptionsBase, string? userId)
        : this(requestOptionsBase)
    {
        UserId = userId;
    }

    public Fido2StartAuthenticateParameter(Fido2RequestOptionsBase? requestOptionsBase, string? userId, Fido2AuthenticateOptions? options)
        : this(requestOptionsBase, userId)
    {
        Options = options;
    }
}
