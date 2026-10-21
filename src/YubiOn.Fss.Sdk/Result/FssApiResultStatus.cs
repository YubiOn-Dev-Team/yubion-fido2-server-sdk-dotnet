using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Represents the result status of a YubiOn FSS API response.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FssApiResultStatus
{
    /// <summary>Request succeeded.</summary>
    OK,
    /// <summary>An unexpected server error occurred.</summary>
    UNEXPECTED_ERROR,
    /// <summary>Communication with the server failed.</summary>
    COMMUNICATION_FAILED,
    /// <summary>The JSON response format was invalid.</summary>
    BAD_JSON_FORMAT,
    /// <summary>The request parameters were invalid.</summary>
    PARAMETER_ERROR,
    /// <summary>Failed to insert a database record.</summary>
    INSERT_ERROR,
    /// <summary>Failed to update a database record.</summary>
    UPDATE_ERROR,
    /// <summary>Failed to delete a database record.</summary>
    DELETE_ERROR,
    /// <summary>A duplicate entry was detected.</summary>
    DUPLICATED,
    /// <summary>The requested resource was not found.</summary>
    NOT_FOUND,
    /// <summary>The resource already exists.</summary>
    ALREADY_EXISTS,
    /// <summary>API authentication failed (invalid credentials or signature).</summary>
    AUTHENTICATION_FAILED,
    /// <summary>Unauthorized access.</summary>
    UNAUTHORIZED,
    /// <summary>Permission denied for the requested operation.</summary>
    PERMISSION_ERROR,
    /// <summary>License is invalid, expired, or resource limit reached.</summary>
    LICENSE_ERROR,

    // C# PascalCase aliases
    /// <summary>PascalCase alias for <see cref="OK"/>.</summary>
    Ok = OK,
    /// <summary>PascalCase alias for <see cref="UNEXPECTED_ERROR"/>.</summary>
    UnexpectedError = UNEXPECTED_ERROR,
    /// <summary>PascalCase alias for <see cref="COMMUNICATION_FAILED"/>.</summary>
    CommunicationFailed = COMMUNICATION_FAILED,
    /// <summary>PascalCase alias for <see cref="BAD_JSON_FORMAT"/>.</summary>
    BadJsonFormat = BAD_JSON_FORMAT,
    /// <summary>PascalCase alias for <see cref="PARAMETER_ERROR"/>.</summary>
    ParameterError = PARAMETER_ERROR,
    /// <summary>PascalCase alias for <see cref="INSERT_ERROR"/>.</summary>
    InsertError = INSERT_ERROR,
    /// <summary>PascalCase alias for <see cref="UPDATE_ERROR"/>.</summary>
    UpdateError = UPDATE_ERROR,
    /// <summary>PascalCase alias for <see cref="DELETE_ERROR"/>.</summary>
    DeleteError = DELETE_ERROR,
    /// <summary>PascalCase alias for <see cref="DUPLICATED"/>.</summary>
    Duplicated = DUPLICATED,
    /// <summary>PascalCase alias for <see cref="NOT_FOUND"/>.</summary>
    NotFound = NOT_FOUND,
    /// <summary>PascalCase alias for <see cref="ALREADY_EXISTS"/>.</summary>
    AlreadyExists = ALREADY_EXISTS,
    /// <summary>PascalCase alias for <see cref="AUTHENTICATION_FAILED"/>.</summary>
    AuthenticationFailed = AUTHENTICATION_FAILED,
    /// <summary>PascalCase alias for <see cref="UNAUTHORIZED"/>.</summary>
    Unauthorized = UNAUTHORIZED,
    /// <summary>PascalCase alias for <see cref="PERMISSION_ERROR"/>.</summary>
    PermissionError = PERMISSION_ERROR,
    /// <summary>PascalCase alias for <see cref="LICENSE_ERROR"/>.</summary>
    LicenseError = LICENSE_ERROR
}
