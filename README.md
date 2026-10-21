# yubion-fido2-server-sdk-dotnet

[English](README.md) | [日本語](README.ja.md)

## What is yubion-fido2-server-sdk-dotnet?

`yubion-fido2-server-sdk-dotnet` is an official SDK library for using the YubiOn FIDO2 Server Service (hereinafter referred to as YubiOn FSS) from C# / .NET environments.  
It enables server-side applications (such as ASP.NET Core web apps, desktop apps, or console utilities) to easily integrate FIDO2 / WebAuthn authentication (Passkeys).

### Key Features
- **Multi-Target Support**: Targets both `.NET 8` and `.NET 6`, ensuring compatibility with current .NET 8 / 9, upcoming .NET 10, and existing .NET 6 runtimes.
- **Modern Async & Sync APIs**: Provides standard async/await methods (`*Async`) alongside synchronous methods.
- **Java / Node.js SDK Compatibility**: Adopts standard C# PascalCase naming conventions while maintaining property aliases for seamless migration from other SDKs.
- **3 API Authentication Methods**: Supports Nonce-based ECDSA signature authentication, Datetime-based ECDSA signature authentication, and Access Key authentication.

---

## Installation & Build

### 1. Installing from NuGet (Recommended)

Install the package directly from NuGet.org using the .NET CLI or Package Manager:

```bash
dotnet add package YubiOn.Fss.Sdk
```

### 2. Building from Source

You can also build the project from source with Visual Studio 2022 or the .NET CLI.

Run the provided build script (`build.bat` on Windows Command Prompt or `build.ps1` in PowerShell) with no arguments:

```cmd
# Windows Command Prompt
build.bat

# PowerShell
./build.ps1
```

After building, the compiled artifacts are placed in the `artifacts/` directory:
- `artifacts/YubiOn.Fss.Sdk.1.0.0.nupkg` (NuGet package)
- `artifacts/dll/net8.0/` (.NET 8 DLL and XML documentation)
- `artifacts/dll/net6.0/` (.NET 6 DLL and XML documentation)

#### Local NuGet Package Reference
Install the built package from the `artifacts/` folder:
```bash
dotnet add package YubiOn.Fss.Sdk -s ./artifacts
```

#### Direct DLL Reference
Reference `YubiOn.Fss.Sdk.dll` located under `artifacts/dll/net8.0/` or `net6.0/`.

---

## Quick Start

### 1. Initializing the SDK Client

```csharp
using YubiOn.Fss.Sdk;
using YubiOn.Fss.Sdk.Config;

// Configuration with Nonce-based signature authentication
var config = new FssSdkConfig(
    rpId: "your-relying-party.example.com",
    apiAuthId: "your-api-auth-id",
    apiAuthType: FssApiAuthType.NonceSignAuth,
    secretKey: "YOUR_BASE64URL_ENCODED_PKCS8_EC_PRIVATE_KEY"
);

// Create SDK instance (implements IDisposable)
using var sdk = new YubiOnFssSdk(config);
```

### 2. Registering a User

```csharp
using YubiOn.Fss.Sdk.Data.User;

var registerParam = new UserDataRegisterParameter(
    userId: "user_12345",
    userName: "alice@example.com",
    displayName: "Alice Smith"
);

var response = await sdk.RegisterUserAsync(registerParam);
Console.WriteLine($"User created: {response.User.UserName} (ID: {response.User.UserId})");
```

### 3. FIDO2 Credential Registration Flow (Passkey Registration)

```csharp
using YubiOn.Fss.Sdk.Data.Fido;

// Step 1: Start ceremony (fetch creation options from server)
var startParam = new Fido2StartRegisterParameter
{
    UserId = "user_12345",
    UserName = "alice@example.com"
};

var startResponse = await sdk.StartRegisterCredentialAsync(startParam);
string sessionCookie = startResponse.SessionCookie; // Preserve session cookie

// Creation options to send to the browser
var creationOptions = startResponse.Data.CreationOptions;

// Step 2: After calling navigator.credentials.create() in the browser, complete registration
var finishParam = new Fido2FinishRegisterParameter
{
    Credential = clientPublicKeyCredentialJson // JSON received from client WebAuthn API
};

var finishResponse = await sdk.FinishRegisterCredentialAsync(finishParam, sessionCookie);
Console.WriteLine($"Credential registered: {finishResponse.Credential.CredentialId}");
```

### 4. FIDO2 Authentication Flow (Login)

```csharp
// Step 1: Start ceremony (fetch request options from server)
var authStartParam = new Fido2StartAuthenticateParameter
{
    UserName = "alice@example.com"
};

var authStartResponse = await sdk.StartAuthenticateAsync(authStartParam);
string sessionCookie = authStartResponse.SessionCookie;

// Step 2: After calling navigator.credentials.get() in the browser, complete authentication
var authFinishParam = new Fido2FinishAuthenticateParameter
{
    Credential = clientAuthCredentialJson
};

var authFinishResponse = await sdk.FinishAuthenticateAsync(authFinishParam, sessionCookie);
Console.WriteLine($"Authentication successful! User ID: {authFinishResponse.Credential.UserId}");
```

---

## Running Tests

### Unit Tests (Offline)
Unit tests, mock HTTP tests, and authenticator emulator tests run without connecting to a live server:
```bash
dotnet test
```

### Integration Tests (Live Server)
To run live end-to-end integration tests, provide an `environment.properties` file or set environment variables (`FSS_TEST_*`).

| Environment Variable / Property | Description |
|---|---|
| `FSS_TEST_ENDPOINT` | Test API Endpoint URL (optional) |
| `FSS_TEST_RP_ID` | Test RP ID (Warning: data will be modified during testing; do not use production RP ID) |
| `FSS_TEST_NONCE_API_AUTH_ID` | API Auth ID for Nonce authentication |
| `FSS_TEST_NONCE_CORRECT_API_SECRET_KEY` | Correct private key for Nonce authentication (PKCS#8 Base64URL) |
| `FSS_TEST_NONCE_INCORRECT_API_SECRET_KEY` | Incorrect private key for negative testing |
| `FSS_TEST_DATETIME_API_AUTH_ID` | API Auth ID for Datetime authentication |
| `FSS_TEST_DATETIME_CORRECT_API_SECRET_KEY` | Correct private key for Datetime authentication |
| `FSS_TEST_ACCESSKEY_API_AUTH_ID` | API Auth ID for Access Key authentication |
| `FSS_TEST_ACCESSKEY_CORRECT_API_SECRET_KEY` | Correct secret key for Access Key authentication |

---

## Documentation

- [API Reference (English)](api-reference.md)
- [API リファレンス (日本語)](api-reference.ja.md)

---

## License

This project is licensed under the [MIT License](LICENSE).  
Copyright (c) SOFTGIKEN Co.,Ltd. All rights reserved.
