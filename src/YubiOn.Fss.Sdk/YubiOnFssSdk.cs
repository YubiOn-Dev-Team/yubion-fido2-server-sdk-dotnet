using YubiOn.Fss.Sdk.Config;
using YubiOn.Fss.Sdk.Data.Credential;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;
using YubiOn.Fss.Sdk.Internal;
using YubiOn.Fss.Sdk.Result;

namespace YubiOn.Fss.Sdk;

/// <summary>
/// Main SDK class for interacting with the YubiOn FIDO2 Server Service (FSS).
/// Provides user management, credential management, and FIDO ceremony flows.
/// </summary>
public class YubiOnFssSdk : IDisposable
{
    private readonly IFssApiRequester _requester;
    private readonly FssSdkConfig _config;
    private readonly bool _disposeRequester;

    /// <summary>
    /// Initializes a new instance of the SDK with default options.
    /// </summary>
    /// <param name="config">The SDK configuration.</param>
    public YubiOnFssSdk(FssSdkConfig config)
        : this(config, false, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the SDK with advanced debug/validation options.
    /// </summary>
    /// <param name="config">The SDK configuration.</param>
    /// <param name="denyUnknownResponseMember">Whether to fail if unknown properties are returned in API responses.</param>
    /// <param name="allowBadSslForDebug">Whether to bypass SSL certificate validation (for testing/debug only).</param>
    public YubiOnFssSdk(FssSdkConfig config, bool denyUnknownResponseMember, bool allowBadSslForDebug)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _requester = FssApiRequesterFactory.Create(config, denyUnknownResponseMember, allowBadSslForDebug);
        _disposeRequester = true;
    }

    /// <summary>
    /// Initializes a new instance of the SDK with a custom requester (for mocking or DI).
    /// </summary>
    /// <param name="config">The SDK configuration.</param>
    /// <param name="requester">The custom API requester instance.</param>
    public YubiOnFssSdk(FssSdkConfig config, IFssApiRequester requester)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _requester = requester ?? throw new ArgumentNullException(nameof(requester));
        _disposeRequester = false;
    }

    /// <summary>
    /// Gets the RP ID configured in the SDK configuration.
    /// </summary>
    public string? RpId => _config.RpId;

    /// <summary>
    /// Java-compatible accessor for the RP ID.
    /// </summary>
    public string? GetRpId() => RpId;

    #region FIDO Ceremony Flow

    /// <summary>
    /// Starts a FIDO credential registration ceremony.
    /// </summary>
    /// <param name="parameter">The registration parameter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The start response containing creation options and session cookie.</returns>
    public async Task<ApiSessionResponse<StartRegisterCredentialResponse>> StartRegisterCredentialAsync(
        Fido2StartRegisterParameter parameter,
        CancellationToken cancellationToken = default)
    {
        var cookieResponse = await _requester.RequestWithCookieResponseAsync<StartRegisterCredentialResponse>(
            "registerCredential/start", parameter, withAuth: true, cookie: null, cancellationToken).ConfigureAwait(false);
        return new ApiSessionResponse<StartRegisterCredentialResponse>(cookieResponse.Response!, cookieResponse.Cookie);
    }

    /// <summary>
    /// Synchronously starts a FIDO credential registration ceremony.
    /// </summary>
    public ApiSessionResponse<StartRegisterCredentialResponse> StartRegisterCredential(Fido2StartRegisterParameter parameter)
    {
        return StartRegisterCredentialAsync(parameter).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Verifies a FIDO credential registration ceremony.
    /// </summary>
    /// <param name="parameter">The registration finish parameter.</param>
    /// <param name="sessionCookie">The session cookie from startRegisterCredential.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The verification response.</returns>
    public async Task<FinishRegisterCredentialResponse> VerifyRegisterCredentialAsync(
        Fido2FinishRegisterParameter parameter,
        string sessionCookie,
        CancellationToken cancellationToken = default)
    {
        var cookieResponse = await _requester.RequestWithCookieResponseAsync<FinishRegisterCredentialResponse>(
            "registerCredential/verify", parameter, withAuth: true, cookie: sessionCookie, cancellationToken).ConfigureAwait(false);
        return cookieResponse.Response!;
    }

    /// <summary>
    /// Synchronously verifies a FIDO credential registration ceremony.
    /// </summary>
    public FinishRegisterCredentialResponse VerifyRegisterCredential(Fido2FinishRegisterParameter parameter, string sessionCookie)
    {
        return VerifyRegisterCredentialAsync(parameter, sessionCookie).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Finishes a FIDO credential registration ceremony.
    /// </summary>
    /// <param name="parameter">The registration finish parameter.</param>
    /// <param name="sessionCookie">The session cookie from startRegisterCredential.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The finish response.</returns>
    public async Task<FinishRegisterCredentialResponse> FinishRegisterCredentialAsync(
        Fido2FinishRegisterParameter parameter,
        string sessionCookie,
        CancellationToken cancellationToken = default)
    {
        var cookieResponse = await _requester.RequestWithCookieResponseAsync<FinishRegisterCredentialResponse>(
            "registerCredential/finish", parameter, withAuth: true, cookie: sessionCookie, cancellationToken).ConfigureAwait(false);
        return cookieResponse.Response!;
    }

    /// <summary>
    /// Synchronously finishes a FIDO credential registration ceremony.
    /// </summary>
    public FinishRegisterCredentialResponse FinishRegisterCredential(Fido2FinishRegisterParameter parameter, string sessionCookie)
    {
        return FinishRegisterCredentialAsync(parameter, sessionCookie).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Starts a FIDO credential authentication ceremony.
    /// </summary>
    /// <param name="parameter">The authentication start parameter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The start response containing request options and session cookie.</returns>
    public async Task<ApiSessionResponse<StartAuthenticateResponse>> StartAuthenticateAsync(
        Fido2StartAuthenticateParameter parameter,
        CancellationToken cancellationToken = default)
    {
        var cookieResponse = await _requester.RequestWithCookieResponseAsync<StartAuthenticateResponse>(
            "authenticate/start", parameter, withAuth: true, cookie: null, cancellationToken).ConfigureAwait(false);
        return new ApiSessionResponse<StartAuthenticateResponse>(cookieResponse.Response!, cookieResponse.Cookie);
    }

    /// <summary>
    /// Synchronously starts a FIDO credential authentication ceremony.
    /// </summary>
    public ApiSessionResponse<StartAuthenticateResponse> StartAuthenticate(Fido2StartAuthenticateParameter parameter)
    {
        return StartAuthenticateAsync(parameter).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Finishes a FIDO credential authentication ceremony.
    /// </summary>
    /// <param name="parameter">The authentication finish parameter.</param>
    /// <param name="sessionCookie">The session cookie from startAuthenticate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The finish response.</returns>
    public async Task<FinishAuthenticateResponse> FinishAuthenticateAsync(
        Fido2FinishAuthenticateParameter parameter,
        string sessionCookie,
        CancellationToken cancellationToken = default)
    {
        var cookieResponse = await _requester.RequestWithCookieResponseAsync<FinishAuthenticateResponse>(
            "authenticate/finish", parameter, withAuth: true, cookie: sessionCookie, cancellationToken).ConfigureAwait(false);
        return cookieResponse.Response!;
    }

    /// <summary>
    /// Synchronously finishes a FIDO credential authentication ceremony.
    /// </summary>
    public FinishAuthenticateResponse FinishAuthenticate(Fido2FinishAuthenticateParameter parameter, string sessionCookie)
    {
        return FinishAuthenticateAsync(parameter, sessionCookie).GetAwaiter().GetResult();
    }

    #endregion

    #region User Management

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="withDisabledUser">Whether to include disabled users.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<GetUsersResponse> GetAllUsersAsync(bool withDisabledUser = false, CancellationToken cancellationToken = default)
    {
        var param = new Dictionary<string, object>
        {
            ["withDisabledUser"] = withDisabledUser
        };
        return await _requester.RequestAsync<GetUsersResponse>("getAllUsers", param, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously gets all users.
    /// </summary>
    public GetUsersResponse GetAllUsers(bool withDisabledUser = false)
    {
        return GetAllUsersAsync(withDisabledUser).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets users matching the specified user name.
    /// </summary>
    /// <param name="userName">The user name to search for.</param>
    /// <param name="withDisabledUser">Whether to include disabled users.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<GetUsersResponse> GetUsersByUserNameAsync(string userName, bool withDisabledUser = false, CancellationToken cancellationToken = default)
    {
        var param = new Dictionary<string, object>
        {
            ["userName"] = userName,
            ["withDisabledUser"] = withDisabledUser
        };
        return await _requester.RequestAsync<GetUsersResponse>("getUsersByUserName", param, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously gets users matching the specified user name.
    /// </summary>
    public GetUsersResponse GetUsersByUserName(string userName, bool withDisabledUser = false)
    {
        return GetUsersByUserNameAsync(userName, withDisabledUser).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a user by user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="withDisabledUser">Whether to include disabled users.</param>
    /// <param name="withDisabledCredential">Whether to include disabled credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<GetUserResponse> GetUserAsync(string userId, bool withDisabledUser = false, bool withDisabledCredential = false, CancellationToken cancellationToken = default)
    {
        var param = new Dictionary<string, object>
        {
            ["userId"] = userId,
            ["withDisabledUser"] = withDisabledUser,
            ["withDisabledCredential"] = withDisabledCredential
        };
        return await _requester.RequestAsync<GetUserResponse>("getUser", param, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously gets a user by user ID.
    /// </summary>
    public GetUserResponse GetUser(string userId, bool withDisabledUser = false, bool withDisabledCredential = false)
    {
        return GetUserAsync(userId, withDisabledUser, withDisabledCredential).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="parameter">The user registration parameter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<RegisterUserResponse> RegisterUserAsync(UserDataRegisterParameter parameter, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>
        {
            ["user"] = parameter
        };
        return await _requester.RequestAsync<RegisterUserResponse>("registerUser", body, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously registers a new user.
    /// </summary>
    public RegisterUserResponse RegisterUser(UserDataRegisterParameter parameter)
    {
        return RegisterUserAsync(parameter).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="parameter">The user update parameter.</param>
    /// <param name="withUpdatedCheck">Whether to perform optimistic concurrency check based on updated timestamp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<UpdateUserResponse> UpdateUserAsync(UserDataUpdateParameter parameter, bool withUpdatedCheck = false, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>
        {
            ["user"] = parameter,
            ["options"] = new Dictionary<string, object>
            {
                ["withUpdatedCheck"] = withUpdatedCheck
            }
        };
        return await _requester.RequestAsync<UpdateUserResponse>("updateUser", body, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously updates an existing user.
    /// </summary>
    public UpdateUserResponse UpdateUser(UserDataUpdateParameter parameter, bool withUpdatedCheck = false)
    {
        return UpdateUserAsync(parameter, withUpdatedCheck).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="userId">The user ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<DeleteUserResponse> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>
        {
            ["userId"] = userId
        };
        return await _requester.RequestAsync<DeleteUserResponse>("deleteUser", body, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously deletes a user.
    /// </summary>
    public DeleteUserResponse DeleteUser(string userId)
    {
        return DeleteUserAsync(userId).GetAwaiter().GetResult();
    }

    #endregion

    #region Credential Management

    /// <summary>
    /// Gets a credential.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="withDisabledUser">Whether to include disabled users.</param>
    /// <param name="withDisabledCredential">Whether to include disabled credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<GetCredentialResponse> GetCredentialAsync(
        string userId,
        string credentialId,
        bool withDisabledUser = false,
        bool withDisabledCredential = false,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>
        {
            ["userId"] = userId,
            ["credentialId"] = credentialId,
            ["withDisabledUser"] = withDisabledUser,
            ["withDisabledCredential"] = withDisabledCredential
        };
        return await _requester.RequestAsync<GetCredentialResponse>("getCredential", body, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously gets a credential.
    /// </summary>
    public GetCredentialResponse GetCredential(
        string userId,
        string credentialId,
        bool withDisabledUser = false,
        bool withDisabledCredential = false)
    {
        return GetCredentialAsync(userId, credentialId, withDisabledUser, withDisabledCredential).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Updates a credential.
    /// </summary>
    /// <param name="parameter">The credential update parameter.</param>
    /// <param name="withUpdatedCheck">Whether to check for updated date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<UpdateCredentialResponse> UpdateCredentialAsync(
        CredentialDataUpdateParameter parameter,
        bool withUpdatedCheck = false,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>
        {
            ["credential"] = parameter,
            ["options"] = new Dictionary<string, object>
            {
                ["withUpdatedCheck"] = withUpdatedCheck
            }
        };
        return await _requester.RequestAsync<UpdateCredentialResponse>("updateCredential", body, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously updates a credential.
    /// </summary>
    public UpdateCredentialResponse UpdateCredential(CredentialDataUpdateParameter parameter, bool withUpdatedCheck = false)
    {
        return UpdateCredentialAsync(parameter, withUpdatedCheck).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Deletes a credential.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="credentialId">The credential ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<DeleteCredentialResponse> DeleteCredentialAsync(string userId, string credentialId, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>
        {
            ["userId"] = userId,
            ["credentialId"] = credentialId
        };
        return await _requester.RequestAsync<DeleteCredentialResponse>("deleteCredential", body, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously deletes a credential.
    /// </summary>
    public DeleteCredentialResponse DeleteCredential(string userId, string credentialId)
    {
        return DeleteCredentialAsync(userId, credentialId).GetAwaiter().GetResult();
    }

    #endregion

    /// <summary>
    /// Releases the unmanaged and managed resources used by the <see cref="YubiOnFssSdk"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposeRequester)
        {
            _requester.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
