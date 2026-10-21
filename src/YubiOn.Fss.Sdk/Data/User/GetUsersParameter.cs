using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Data.User;

/// <summary>
/// Parameters for the getUsers API call.
/// </summary>
public class GetUsersParameter
{
    /// <summary>
    /// List of user IDs to retrieve.
    /// </summary>
    [JsonPropertyName("userIds")]
    public List<string>? UserIds { get; set; }

    /// <summary>
    /// Whether to include disabled users in the query result.
    /// </summary>
    [JsonPropertyName("withDisabled")]
    public bool WithDisabled { get; set; }

    /// <summary>
    /// Initializes a new empty instance of <see cref="GetUsersParameter"/>.
    /// </summary>
    public GetUsersParameter()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="GetUsersParameter"/> with user IDs and disabled filter.
    /// </summary>
    /// <param name="userIds">List of user IDs.</param>
    /// <param name="withDisabled">Whether to include disabled users.</param>
    public GetUsersParameter(List<string>? userIds, bool withDisabled = false)
    {
        UserIds = userIds;
        WithDisabled = withDisabled;
    }
}
