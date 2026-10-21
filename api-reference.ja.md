# YubiOn FIDO2 Server SDK for .NET API リファレンス

[English](api-reference.md) | [日本語](api-reference.ja.md)

本ドキュメントは、`yubion-fido2-server-sdk-dotnet` の API リファレンスです。  
ルート名前空間: `YubiOn.Fss.Sdk`

---

## 1. 設定クラス (`YubiOn.Fss.Sdk.Config`)

### `FssSdkConfig`
SDKクライアントの接続・認証設定を保持します。

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

#### コンストラクタ
- `new FssSdkConfig()`
- `new FssSdkConfig(string rpId, string apiAuthId, FssApiAuthType apiAuthType, string secretKey)`
- `new FssSdkConfig(string rpId, string apiAuthId, string apiAuthType, string secretKey)`
- `new FssSdkConfig(string rpId, string apiAuthId, FssApiAuthType apiAuthType, string secretKey, string endpoint)`

### `FssApiAuthType` (Enum)
FSS API との通信認証方式を指定します。
- `NonceSignAuth` (値: 0 / Java互換: `NONCE_SIGN_AUTH`): ノンスを取得し ECDSA P-256 鍵で署名する最も安全な認証方式。
- `DatetimeSignAuth` (値: 1 / Java互換: `DATETIME_SIGN_AUTH`): ISO 8601 UTC 現在時刻を付与して ECDSA P-256 鍵で署名する方式。
- `AccessKeyAuth` (値: 2 / Java互換: `ACCESS_KEY_AUTH`): 事前共有アクセスキーをヘッダーで送信する方式。

---

## 2. メインクライアント (`YubiOn.Fss.Sdk.YubiOnFssSdk`)

すべての操作は非同期メソッド（`*Async`）および同期メソッドの両方を提供しています。`IDisposable` を実装しています。

### ライセンス確認

#### `Task<CheckLicenseResponse> CheckLicenseAsync(CancellationToken cancellationToken = default)`
#### `CheckLicenseResponse CheckLicense()`
現在の RP 設定およびライセンスの有効性を確認します。

**戻り値:** `CheckLicenseResponse`
- `Rp` (`RpData`): RP の設定情報（RP ID、RP名、アイコン等）。

---

### ユーザー管理

#### `Task<GetUserResponse> GetUserAsync(string userId, bool withDisabledUser = false, bool withDisabledCredential = false, CancellationToken cancellationToken = default)`
#### `GetUserResponse GetUser(string userId, bool withDisabledUser = false, bool withDisabledCredential = false)`
指定したユーザーIDのユーザー情報および紐づくクレデンシャル一覧を取得します。

**戻り値:** `GetUserResponse`
- `User` (`UserDataWithCredentialCount`): ユーザー情報。
- `Credentials` (`List<CredentialData>`): 紐づくクレデンシャル一覧。
- `SignalCurrentUserDetailsOptions` (`SignalCurrentUserDetailsOptions`): クライアント側でパスキーの詳細更新シグナルを送るためのオプション。

#### `Task<GetUsersResponse> GetAllUsersAsync(bool withDisabledUser = false, CancellationToken cancellationToken = default)`
#### `GetUsersResponse GetAllUsers(bool withDisabledUser = false)`
全ユーザーの一覧を取得します。

#### `Task<GetUsersResponse> GetUsersByUserNameAsync(string userName, bool withDisabledUser = false, CancellationToken cancellationToken = default)`
#### `GetUsersResponse GetUsersByUserName(string userName, bool withDisabledUser = false)`
指定したユーザー名（ログインID/メールアドレス等）のユーザー一覧を取得します。

#### `Task<RegisterUserResponse> RegisterUserAsync(UserDataRegisterParameter parameter, CancellationToken cancellationToken = default)`
#### `RegisterUserResponse RegisterUser(UserDataRegisterParameter parameter)`
新しいユーザーを登録します。

**引数:** `UserDataRegisterParameter`
- `UserId` (`string`): ユーザーID（省略時はサーバー側で自動採番）。
- `UserName` (`string`): ユーザー名（ユニーク）。
- `DisplayName` (`string`): 表示名。
- `UserAttributes` (`Dictionary<string, object>`): 任意の属性データ。
- `Disabled` (`bool`): 無効状態フラグ。

#### `Task<UpdateUserResponse> UpdateUserAsync(UserDataUpdateParameter parameter, bool withUpdatedCheck = false, CancellationToken cancellationToken = default)`
#### `UpdateUserResponse UpdateUser(UserDataUpdateParameter parameter, bool withUpdatedCheck = false)`
ユーザー情報を更新します。

#### `Task<DeleteUserResponse> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)`
#### `DeleteUserResponse DeleteUser(string userId)`
ユーザーを削除します。

---

### クレデンシャル管理

#### `Task<GetCredentialResponse> GetCredentialAsync(string userId, string credentialId, CancellationToken cancellationToken = default)`
#### `GetCredentialResponse GetCredential(string userId, string credentialId)`
指定したクレデンシャルの詳細情報を取得します。

#### `Task<UpdateCredentialResponse> UpdateCredentialAsync(CredentialDataUpdateParameter parameter, bool withUpdatedCheck = false, CancellationToken cancellationToken = default)`
#### `UpdateCredentialResponse UpdateCredential(CredentialDataUpdateParameter parameter, bool withUpdatedCheck = false)`
クレデンシャル名や無効フラグを更新します。

#### `Task<DeleteCredentialResponse> DeleteCredentialAsync(string userId, string credentialId, CancellationToken cancellationToken = default)`
#### `DeleteCredentialResponse DeleteCredential(string userId, string credentialId)`
クレデンシャルを削除します。

---

### FIDO2 / WebAuthn セレモニー

#### `Task<ApiSessionResponse<StartRegisterCredentialResponse>> StartRegisterCredentialAsync(Fido2StartRegisterParameter parameter, CancellationToken cancellationToken = default)`
#### `ApiSessionResponse<StartRegisterCredentialResponse> StartRegisterCredential(Fido2StartRegisterParameter parameter)`
クレデンシャル登録（パスキー作成）セレモニーを開始します。

**引数:** `Fido2StartRegisterParameter`
- `UserId` (`string`): 登録対象のユーザーID。
- `UserName` (`string`): 登録対象のユーザー名。
- `DisplayName` (`string`): 表示名。
- `CreationOptions` (`Fido2CreationOptionsRegisterCredential`): カスタム作成オプション（省略可能）。

**戻り値:** `ApiSessionResponse<StartRegisterCredentialResponse>`
- `SessionCookie`: セレモニー完了時に必要なセッションCookie文字列。
- `Data.CreationOptions`: ブラウザの `navigator.credentials.create()` に渡すオプションオブジェクト。

#### `Task<FinishRegisterCredentialResponse> FinishRegisterCredentialAsync(Fido2FinishRegisterParameter parameter, string sessionCookie, CancellationToken cancellationToken = default)`
#### `FinishRegisterCredentialResponse FinishRegisterCredential(Fido2FinishRegisterParameter parameter, string sessionCookie)`
ブラウザが生成した公開鍵クレデンシャルを検証し、登録を完了します。

**引数:** `Fido2FinishRegisterParameter`
- `Credential` (`PublicKeyCredentialJson`): ブラウザから受信したクレデンシャルJSON。
- `CredentialName` (`string`): クレデンシャルに付与する任意の名称。
- `sessionCookie` (`string`): `StartRegisterCredential` で取得したセッションCookie。

#### `Task<ApiSessionResponse<StartAuthenticateResponse>> StartAuthenticateAsync(Fido2StartAuthenticateParameter parameter, CancellationToken cancellationToken = default)`
#### `ApiSessionResponse<StartAuthenticateResponse> StartAuthenticate(Fido2StartAuthenticateParameter parameter)`
認証（ログイン）セレモニーを開始します。

**引数:** `Fido2StartAuthenticateParameter`
- `UserName` (`string`): 認証対象のユーザー名（ユーザー名を指定しない Discoverable Credential / パスキーによるログインの場合は省略可）。
- `RequestOptions` (`Fido2RequestOptionsAuthenticate`): カスタム要求オプション（省略可能）。

**戻り値:** `ApiSessionResponse<StartAuthenticateResponse>`
- `SessionCookie`: セレモニー完了時に必要なセッションCookie文字列。
- `Data.RequestOptions`: ブラウザの `navigator.credentials.get()` に渡す要求オプションオブジェクト。

#### `Task<FinishAuthenticateResponse> FinishAuthenticateAsync(Fido2FinishAuthenticateParameter parameter, string sessionCookie, CancellationToken cancellationToken = default)`
#### `FinishAuthenticateResponse FinishAuthenticate(Fido2FinishAuthenticateParameter parameter, string sessionCookie)`
ブラウザから返却されたアサーションを検証し、認証を完了します。

---

## 3. 例外・エラーハンドリング (`YubiOn.Fss.Sdk.Result`)

### `FssApiException`
API呼び出しがエラーとなった場合、またはHTTP通信エラーが発生した場合にスローされます。

```csharp
try
{
    var user = await sdk.GetUserAsync("invalid_user_id");
}
catch (FssApiException ex)
{
    Console.WriteLine($"エラーメッセージ: {ex.Message}");
    Console.WriteLine($"AppStatus: {ex.AppStatus}");
    Console.WriteLine($"HTTPステータス: {ex.HttpStatusCode}");

    var subStatus = ex.GetParsedSubStatus();
    if (subStatus != null)
    {
        Console.WriteLine($"詳細エラーコード: {subStatus.ErrorCode}");
        Console.WriteLine($"詳細メッセージ: {subStatus.ErrorMessage}");
    }
}
```

### 主な `FssApiResultStatus`
- `OK`: 成功
- `AUTHENTICATION_FAILED`: APIキーまたは署名検証の失敗
- `LICENSE_ERROR`: ライセンスが無効または上限超過
- `BAD_REQUEST`: リクエストパラメータの形式不正
- `NOT_FOUND`: 指定したリソースが存在しない
- `UNEXPECTED_ERROR`: サーバー内部エラー
