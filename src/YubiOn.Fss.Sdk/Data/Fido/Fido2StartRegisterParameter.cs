using System.Text.Json.Serialization;
using YubiOn.Fss.Sdk.Data.User;

namespace YubiOn.Fss.Sdk.Data.Fido;

/// <summary>
/// Defines the parameters for initiating a FIDO2 registration process.
/// </summary>
public class Fido2StartRegisterParameter
{
    [JsonPropertyName("creationOptionsBase")]
    public Fido2CreationOptionsBase? CreationOptionsBase { get; set; }

    [JsonPropertyName("user")]
    public UserDataRegisterParameter? User { get; set; }

    [JsonPropertyName("options")]
    public Fido2RegisterOptions? Options { get; set; }

    public Fido2StartRegisterParameter()
    {
    }

    public Fido2StartRegisterParameter(Fido2CreationOptionsBase? creationOptionsBase)
    {
        CreationOptionsBase = creationOptionsBase;
    }

    public Fido2StartRegisterParameter(Fido2CreationOptionsBase? creationOptionsBase, UserDataRegisterParameter? user)
        : this(creationOptionsBase)
    {
        User = user;
    }

    public Fido2StartRegisterParameter(Fido2CreationOptionsBase? creationOptionsBase, UserDataRegisterParameter? user, Fido2RegisterOptions? options)
        : this(creationOptionsBase, user)
    {
        Options = options;
    }
}
