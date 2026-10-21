namespace YubiOn.Fss.Sdk.Config;

/// <summary>
/// Specifies the authentication method used when calling the YubiOn FSS API.
/// </summary>
public enum FssApiAuthType
{
    /// <summary>
    /// Nonce-based signature authentication.
    /// </summary>
    NonceSignAuth,

    /// <summary>
    /// ISO 8601 UTC timestamp-based signature authentication.
    /// </summary>
    DatetimeSignAuth,

    /// <summary>
    /// Access key-based authentication.
    /// </summary>
    AccessKeyAuth,

    // Aliases matching Java SDK constants for ease of migration
    NONCE_SIGN_AUTH = NonceSignAuth,
    DATETIME_SIGN_AUTH = DatetimeSignAuth,
    ACCESS_KEY_AUTH = AccessKeyAuth
}
