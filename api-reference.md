# YubiOn FIDO2 Server SDK for .NET API Reference

[English](api-reference.md) | [日本語](api-reference.ja.md)

This document provides the API reference for `yubion-fido2-server-sdk-dotnet`.  
Root Namespace: `YubiOn.Fss.Sdk`

---

## 1. Configuration Class (`YubiOn.Fss.Sdk.Config`)

### `FssSdkConfig`
Holds the connection and authentication parameters for communicating with YubiOn FSS.

```csharp
public class FssSdkConfig
{
    public string? RpId { get; set; }
    public string? ApiAuthId { get; set; }
    public FssApiAuthType ApiAuthType { get; set; }
    public string? SecretKey { get; set; }
    public string Endpoint { get; set; } = "https://fss-app.yubion.com/api/";
    public string Agent { get; set; } = "yubion-fido2-server-sdk-dotnet";
}
```

#### Constructors
- `new FssSdkConfig()`
- `new FssSdkConfig(string rpId, string apiAuthId, FssApiAuthType apiAuthType, string secretKey)`
- `new FssSdkConfig(string rpId, string apiAuthId, string apiAuthType, string secretKey)`
- `new FssSdkConfig(string rpId, string apiAuthId, FssApiAuthType apiAuthType, string secretKey, string endpoint)`

### `FssApiAuthType` (Enum)
Specifies the authentication method used when calling the YubiOn FSS API.
- `NonceSignAuth` (Value: 0 / Java compatibility: `NONCE_SIGN_AUTH`): Fetches a nonce and signs the request using an ECDSA P-256 private key. Most secure.
- `DatetimeSignAuth` (Value: 1 / Java compatibility: `DATETIME_SIGN_AUTH`): Signs using the current ISO 8601 UTC timestamp and ECDSA P-256 private key.
- `AccessKeyAuth` (Value: 2 / Java compatibility: `ACCESS_KEY_AUTH`): Transmits a pre-shared access key secret in the request headers.

---

## 2. Main Client (`YubiOn.Fss.Sdk.YubiOnFssSdk`)

All operations provide both async (`*Async`) and synchronous methods. Implements `IDisposable`.

### License Check

#### `Task<CheckLicenseResponse> CheckLicenseAsync(CancellationToken cancellationToken = default)`
#### `CheckLicenseResponse CheckLicense()`
Verifies the current RP settings and license validity.

**Return value:** `CheckLicenseResponse`
- `Rp` (`RpData`): RP information (RP ID, RP name, icon, etc.).

---

### User Management

#### `Task<GetUserResponse> GetUserAsync(string userId, bool withDisabledUser = false, bool withDisabledCredential = false, CancellationToken cancellationToken = default)`
#### `GetUserResponse GetUser(string userId, bool withDisabledUser = false, bool withDisabledCredential = false)`
Retrieves user information and associated credentials by user ID.

**Return value:** `GetUserResponse`
- `User` (`UserDataWithCredentialCount`): User details.
- `Credentials` (`List<CredentialData>`): List of registered credentials.
- `SignalCurrentUserDetailsOptions` (`SignalCurrentUserDetailsOptions`): Options for updating passkey details in the browser.

#### `Task<GetUsersResponse> GetAllUsersAsync(bool withDisabledUser = false, CancellationToken cancellationToken = default)`
#### `GetUsersResponse GetAllUsers(bool withDisabledUser = false)`
Retrieves a list of all users.

#### `Task<GetUsersResponse> GetUsersByUserNameAsync(string userName, bool withDisabledUser = false, CancellationToken cancellationToken = default)`
#### `GetUsersResponse GetUsersByUserName(string userName, bool withDisabledUser = false)`
Retrieves a list of users matching the given username.

#### `Task<RegisterUserResponse> RegisterUserAsync(UserDataRegisterParameter parameter, CancellationToken cancellationToken = default)`
#### `RegisterUserResponse RegisterUser(UserDataRegisterParameter parameter)`
Registers a new user.

**Parameter:** `UserDataRegisterParameter`
- `UserId` (`string`): User identifier (auto-generated if omitted).
- `UserName` (`string`): Unique username.
- `DisplayName` (`string`): Display name.
- `UserAttributes` (`Dictionary<string, object>`): Custom attributes.
- `Disabled` (`bool`): Disabled state flag.

#### `Task<UpdateUserResponse> UpdateUserAsync(UserDataUpdateParameter parameter, bool withUpdatedCheck = false, CancellationToken cancellationToken = default)`
#### `UpdateUserResponse UpdateUser(UserDataUpdateParameter parameter, bool withUpdatedCheck = false)`
Updates user information.

#### `Task<DeleteUserResponse> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)`
#### `DeleteUserResponse DeleteUser(string userId)`
Deletes a user.

---

### Credential Management

#### `Task<GetCredentialResponse> GetCredentialAsync(string userId, string credentialId, CancellationToken cancellationToken = default)`
#### `GetCredentialResponse GetCredential(string userId, string credentialId)`
Retrieves details for a specific credential.

#### `Task<UpdateCredentialResponse> UpdateCredentialAsync(CredentialDataUpdateParameter parameter, bool withUpdatedCheck = false, CancellationToken cancellationToken = default)`
#### `UpdateCredentialResponse UpdateCredential(CredentialDataUpdateParameter parameter, bool withUpdatedCheck = false)`
Updates credential name or disabled state.

#### `Task<DeleteCredentialResponse> DeleteCredentialAsync(string userId, string credentialId, CancellationToken cancellationToken = default)`
#### `DeleteCredentialResponse DeleteCredential(string userId, string credentialId)`
Deletes a credential.

---

### FIDO2 / WebAuthn Ceremonies

#### `Task<ApiSessionResponse<StartRegisterCredentialResponse>> StartRegisterCredentialAsync(Fido2StartRegisterParameter parameter, CancellationToken cancellationToken = default)`
#### `ApiSessionResponse<StartRegisterCredentialResponse> StartRegisterCredential(Fido2StartRegisterParameter parameter)`
Initiates a credential registration (passkey creation) ceremony.

**Parameters:** `Fido2StartRegisterParameter`
- `UserId` (`string`): Target user ID.
- `UserName` (`string`): Target username.
- `DisplayName` (`string`): Display name.
- `CreationOptions` (`Fido2CreationOptionsRegisterCredential`): Optional custom creation options.

**Return value:** `ApiSessionResponse<StartRegisterCredentialResponse>`
- `SessionCookie`: Session cookie required when finishing the ceremony.
- `Data.CreationOptions`: Options object to pass to `navigator.credentials.create()`.

#### `Task<FinishRegisterCredentialResponse> FinishRegisterCredentialAsync(Fido2FinishRegisterParameter parameter, string sessionCookie, CancellationToken cancellationToken = default)`
#### `FinishRegisterCredentialResponse FinishRegisterCredential(Fido2FinishRegisterParameter parameter, string sessionCookie)`
Verifies the client's generated public key credential and completes registration.

**Parameters:** `Fido2FinishRegisterParameter`
- `Credential` (`PublicKeyCredentialJson`): Credential JSON received from browser.
- `CredentialName` (`string`): Optional user-friendly name for the key.
- `sessionCookie` (`string`): Session cookie from `StartRegisterCredential`.

#### `Task<ApiSessionResponse<StartAuthenticateResponse>> StartAuthenticateAsync(Fido2StartAuthenticateParameter parameter, CancellationToken cancellationToken = default)`
#### `ApiSessionResponse<StartAuthenticateResponse> StartAuthenticate(Fido2StartAuthenticateParameter parameter)`
Initiates an authentication (login) ceremony.

**Parameters:** `Fido2StartAuthenticateParameter`
- `UserName` (`string`): Target username (can be omitted for discoverable passkey login).
- `RequestOptions` (`Fido2RequestOptionsAuthenticate`): Optional custom request options.

**Return value:** `ApiSessionResponse<StartAuthenticateResponse>`
- `SessionCookie`: Session cookie required when finishing the ceremony.
- `Data.RequestOptions`: Options object to pass to `navigator.credentials.get()`.

#### `Task<FinishAuthenticateResponse> FinishAuthenticateAsync(Fido2FinishAuthenticateParameter parameter, string sessionCookie, CancellationToken cancellationToken = default)`
#### `FinishAuthenticateResponse FinishAuthenticate(Fido2FinishAuthenticateParameter parameter, string sessionCookie)`
Verifies the assertion response and completes authentication.

---

## 3. Exception & Error Handling (`YubiOn.Fss.Sdk.Result`)

### `FssApiException`
Thrown when an API call fails or an HTTP network error occurs.

```csharp
try
{
    var user = await sdk.GetUserAsync("invalid_user_id");
}
catch (FssApiException ex)
{
    Console.WriteLine($"Error message: {ex.Message}");
    Console.WriteLine($"AppStatus: {ex.AppStatus}");
    Console.WriteLine($"HTTP Status: {ex.HttpStatusCode}");

    var subStatus = ex.GetParsedSubStatus();
    if (subStatus != null)
    {
        Console.WriteLine($"Error code: {subStatus.ErrorCode}");
        Console.WriteLine($"Detail message: {subStatus.ErrorMessage}");
    }
}
```

### Main `FssApiResultStatus`
- `OK`: Success
- `AUTHENTICATION_FAILED`: Invalid API credentials or signature
- `LICENSE_ERROR`: License invalid or limit exceeded
- `BAD_REQUEST`: Invalid request payload format
- `NOT_FOUND`: Target resource not found
- `UNEXPECTED_ERROR`: Internal server error
