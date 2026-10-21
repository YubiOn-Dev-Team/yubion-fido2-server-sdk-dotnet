using System.Text.Json.Serialization;

namespace YubiOn.Fss.Sdk.Result;

/// <summary>
/// Represents detailed FIDO2 error codes returned in AppSubStatus.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Fido2ErrorStatus
{
    /// <summary>The specified user does not exist.</summary>
    USER_NOT_FOUND,
    /// <summary>User name is required for registration.</summary>
    REQUIRE_USER_NAME,
    /// <summary>Either userId or userHandle is required.</summary>
    REQUIRE_USER_ID_OR_USER_HANDLE,
    /// <summary>The user account is disabled.</summary>
    USER_IS_DISABLED,
    /// <summary>The returned userHandle does not match the requested user.</summary>
    USER_HANDLE_NOT_MATCH,
    /// <summary>The specified credential does not exist.</summary>
    CREDENTIAL_NOT_FOUND,
    /// <summary>Credential ID is required.</summary>
    REQUIRE_CREDENTIAL_ID,
    /// <summary>The credential is disabled.</summary>
    CREDENTIAL_IS_DISABLED,
    /// <summary>Credential ID does not match expected value.</summary>
    CREDENTIAL_ID_MISMATCH,
    /// <summary>The credential type is not supported.</summary>
    BAD_CREDENTIAL_TYPE,
    /// <summary>The credential has already been registered.</summary>
    CREDENTIAL_ALREADY_REGISTERED,
    /// <summary>Failed to parse clientDataJSON.</summary>
    CLIENT_DATA_JSON_PARSE_FAILED,
    /// <summary>Attested credential data was expected but missing.</summary>
    REQUIRE_ATTESTED_CREDENTIAL_DATA,
    /// <summary>The request type does not match the ceremony step.</summary>
    BAD_REQUEST_TYPE,
    /// <summary>The Relying Party was not found.</summary>
    RP_NOT_FOUND,
    /// <summary>The RP ID hash does not match the expected Relying Party.</summary>
    RP_ID_HASH_MISMATCH,
    /// <summary>The origin is not in the allowed origins list for this RP.</summary>
    ORIGIN_NOT_ALLOWED,
    /// <summary>User verification is required by policy but was not performed.</summary>
    REQUIRE_USER_VERIFICATION,
    /// <summary>The license limit has been exceeded.</summary>
    LICENSE_LIMIT_EXCEEDED,
    /// <summary>An internal server error occurred.</summary>
    INTERNAL_ERROR,
    /// <summary>An unexpected error occurred.</summary>
    UNEXPECTED_ERROR,
    /// <summary>The session cookie is invalid, expired, or missing.</summary>
    INVALID_SESSION
}
