# YubiOn FIDO2 Server Service (FSS) .NET SDK Sample Application

This is a sample application demonstrating how to use the **YubiOn FIDO2 Server Service SDK for .NET** with ASP.NET Core Web API (backend) and a Vue 3 SPA (frontend) for FIDO2 passkey registration, authentication, and credential management.

No Node.js or npm installation is required. You can launch and run both the server and client using only the **.NET SDK or Visual Studio**.

---

## Features

- **User Registration & Passkey Creation**: Register a new user and create a passkey with a FIDO2 authenticator (Windows Hello, Touch ID, YubiKey, etc.).
- **Passkey Login**: Authenticate passwordlessly using the registered passkey.
- **User List**: View registered users.
- **Credential Management**:
  - View all registered passkeys for a user.
  - Add backup/additional passkeys for the logged-in user.
  - Delete unwanted passkeys.

---

## Prerequisites

1. **Register a Relying Party (RP)** in the FSS management console.
   - For local development, you can use any domain ending in `.localhost` (e.g. `your-app.localhost`).
2. **Create an API Key**
   - Select **Nonce Signature Authentication** (`NonceSignAuth`).
3. **Configure Settings**
   - Update `sample/YubionFssSdkSample/appsettings.json` with your credentials:
   ```json
   {
     "Fss": {
       "RpId": "your-app.localhost",
       "ApiAuthId": "your_api_key_id",
       "SecretKey": "your_api_secret",
       "ApiAuthType": "NonceSignAuth"
     }
   }
   ```
   *(Environment variables `FSS_RP_ID`, `FSS_API_KEY_ID`, `FSS_API_SECRET` are also supported.)*

---

## How to Run

### Method 1: Visual Studio (Recommended)

1. Open `sample/YubionFssSdkSample.sln` in Visual Studio 2022.
2. Set `YubionFssSdkSample` as the Startup Project.
3. Press `F5` (or `Ctrl + F5`). The browser will open automatically.

### Method 2: PowerShell Script

```powershell
cd sample
./run.ps1
```

Or specify custom URL:
```powershell
./run.ps1 -Url "http://your-app.localhost:5000"
```

### Method 3: .NET CLI

```bash
cd sample/YubionFssSdkSample
dotnet run --urls "http://your-app.localhost:5000"
```

---

## Accessing the Application

Access the application using your configured **RP ID** (not `localhost`), for example:
```text
http://your-app.localhost:5000
```
