using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.User;

/// <summary>
/// Parameters for updating an existing user.
/// </summary>
public class UserDataUpdateParameter : UserDataRegisterParameter
{
    /// <summary>
    /// The updated timestamp for optimistic locking check (used when withUpdatedCheck is true).
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    /// Initializes a new empty instance of <see cref="UserDataUpdateParameter"/>.
    /// </summary>
    public UserDataUpdateParameter()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UserDataUpdateParameter"/> with a userId.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    public UserDataUpdateParameter(string? userId)
        : base(userId, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UserDataUpdateParameter"/> with userId and userName.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="userName">The username.</param>
    public UserDataUpdateParameter(string? userId, string? userName)
        : base(userId, userName)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UserDataUpdateParameter"/> with details.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="userName">The username.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="userAttributes">Custom attributes.</param>
    /// <param name="disabled">Disabled state.</param>
    public UserDataUpdateParameter(string? userId, string? userName, string? displayName, Dictionary<string, object>? userAttributes, bool disabled)
        : base(userId, userName, displayName, userAttributes, disabled)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UserDataUpdateParameter"/> including the updated timestamp.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="userName">The username.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="userAttributes">Custom attributes.</param>
    /// <param name="disabled">Disabled state.</param>
    /// <param name="updated">Timestamp for conflict check.</param>
    public UserDataUpdateParameter(string? userId, string? userName, string? displayName, Dictionary<string, object>? userAttributes, bool disabled, DateTimeOffset? updated)
        : base(userId, userName, displayName, userAttributes, disabled)
    {
        Updated = updated;
    }

    /// <summary>
    /// Copy constructor from an existing <see cref="UserDataUpdateParameter"/>.
    /// </summary>
    /// <param name="basePrm">Source update parameter.</param>
    public UserDataUpdateParameter(UserDataUpdateParameter basePrm)
        : base(basePrm)
    {
        Updated = basePrm.Updated;
    }

    /// <summary>
    /// Copy constructor from a <see cref="UserDataRegisterParameter"/>.
    /// </summary>
    /// <param name="basePrm">Source register parameter.</param>
    public UserDataUpdateParameter(UserDataRegisterParameter basePrm)
        : base(basePrm)
    {
    }

    /// <summary>
    /// Copy constructor from an existing <see cref="UserData"/>.
    /// </summary>
    /// <param name="user">Source user data.</param>
    public UserDataUpdateParameter(UserData user)
        : base(user)
    {
        Updated = user.Updated;
    }
}
