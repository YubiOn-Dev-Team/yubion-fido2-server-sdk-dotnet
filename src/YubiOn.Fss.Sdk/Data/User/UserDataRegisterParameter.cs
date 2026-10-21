using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.User;

/// <summary>
/// Parameters for registering a new user.
/// </summary>
public class UserDataRegisterParameter
{
    /// <summary>
    /// Optional user ID. If omitted, the server will auto-generate a unique ID.
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    /// <summary>
    /// The unique login username (e.g. email address).
    /// </summary>
    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    /// <summary>
    /// The friendly display name of the user.
    /// </summary>
    [JsonPropertyName("displayName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Custom user attributes dictionary.
    /// </summary>
    [JsonPropertyName("userAttributes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<string, object>? UserAttributes { get; set; }

    /// <summary>
    /// Whether the user should be created in disabled state.
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Initializes a new empty instance of <see cref="UserDataRegisterParameter"/>.
    /// </summary>
    public UserDataRegisterParameter()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UserDataRegisterParameter"/> with userId and userName.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="userName">The username.</param>
    public UserDataRegisterParameter(string? userId, string? userName)
    {
        UserId = userId;
        UserName = userName;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UserDataRegisterParameter"/> with userId, userName, and displayName.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="userName">The username.</param>
    /// <param name="displayName">The display name.</param>
    public UserDataRegisterParameter(string? userId, string? userName, string? displayName)
        : this(userId, userName)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UserDataRegisterParameter"/> with all parameters.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="userName">The username.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="userAttributes">Custom user attributes.</param>
    /// <param name="disabled">Whether the user is disabled.</param>
    public UserDataRegisterParameter(string? userId, string? userName, string? displayName, Dictionary<string, object>? userAttributes, bool disabled)
        : this(userId, userName)
    {
        DisplayName = displayName;
        UserAttributes = userAttributes;
        Disabled = disabled;
    }

    /// <summary>
    /// Copy constructor from an existing <see cref="UserDataRegisterParameter"/>.
    /// </summary>
    /// <param name="basePrm">The source parameter object.</param>
    public UserDataRegisterParameter(UserDataRegisterParameter basePrm)
    {
        if (basePrm == null) throw new ArgumentNullException(nameof(basePrm));
        UserId = basePrm.UserId;
        UserName = basePrm.UserName;
        DisplayName = basePrm.DisplayName;
        UserAttributes = basePrm.UserAttributes != null ? new Dictionary<string, object>(basePrm.UserAttributes) : null;
        Disabled = basePrm.Disabled;
    }

    /// <summary>
    /// Copy constructor from an existing <see cref="UserData"/>.
    /// </summary>
    /// <param name="baseData">The source user data.</param>
    public UserDataRegisterParameter(UserData baseData)
    {
        if (baseData == null) throw new ArgumentNullException(nameof(baseData));
        UserId = baseData.UserId;
        UserName = baseData.UserName;
        DisplayName = baseData.DisplayName;
        UserAttributes = baseData.UserAttributes != null ? new Dictionary<string, object>(baseData.UserAttributes) : null;
        Disabled = baseData.Disabled;
    }
}
