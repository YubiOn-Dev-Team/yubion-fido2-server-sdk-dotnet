# yubion-fido2-server-sdk-dotnet

[English](README.md) | [日本語](README.ja.md)

## yubion-fido2-server-sdk-dotnet とは

`yubion-fido2-server-sdk-dotnet` は、YubiOn FIDO2 Server サービス（以下、YubiOn FSS）を C# / .NET 環境から利用するための公式ライブラリです。  
ASP.NET Core やデスクトップ・コンソールなどのサーバーサイドアプリケーションから、FIDO2 / WebAuthn 認証（Passkey）を容易に組み込むことができます。

### 主な特徴
- **マルチターゲット対応**: .NET 8 および .NET 6 をターゲットとしており、最新の .NET 8 / 9 や将来の .NET 10、既存の .NET 6 環境からも直接利用可能です。
- **モダンな非同期 / 同期 API**: `async`/`await` を用いた非同期メソッド（`*Async`）と同期呼び出しメソッドの両方を提供。
- **Java / Node.js 版 SDK との互換性**: メソッド名やプロパティ名に C# の標準的な PascalCase 命名を採用しつつ、既存 SDK からの移行がスムーズに行えるエイリアスを備えています。
- **3つのAPI認証方式**: ノンス署名認証（ECDSA P-256）、日時署名認証（ECDSA P-256）、アクセスキー認証をサポート。

---

## インストール・ビルド方法

### 1. NuGet からのインストール（推奨）

.NET CLI または Visual Studio の NuGet パッケージマネージャーから直接インストールできます。

```bash
dotnet add package YubiOn.Fss.Sdk
```

### 2. ソースコードからのビルドとパッケージ生成

Visual Studio 2022 または .NET CLI でソースコードからビルドすることも可能です。

付属のビルドスクリプト（`build.bat` または `build.ps1`）を引数なしで実行すると、ビルド・テスト・パッケージングが一括で行われます。

```cmd
# Windows コマンドプロンプト
build.bat

# PowerShell
./build.ps1
```

ビルドが完了すると、`artifacts/` ディレクトリ配下に以下が出力されます：
- `artifacts/YubiOn.Fss.Sdk.1.0.0.nupkg` (NuGet パッケージ)
- `artifacts/dll/net8.0/` (.NET 8 向け DLL および XML ドキュメント)
- `artifacts/dll/net6.0/` (.NET 6 向け DLL および XML ドキュメント)

#### ローカル NuGet パッケージとして参照する場合
生成された `.nupkg` を以下のコマンドでローカル指定してインストールします。
```bash
dotnet add package YubiOn.Fss.Sdk -s ./artifacts
```

#### 直接 DLL を参照する場合
`artifacts/dll/net8.0/` または `net6.0/` 内の `YubiOn.Fss.Sdk.dll` をプロジェクトで参照設定してください。

---

## クイックスタート

### 1. SDKクライアントの初期化

```csharp
using YubiOn.Fss.Sdk;
using YubiOn.Fss.Sdk.Config;

// Nonce署名認証による設定例
var config = new FssSdkConfig(
    rpId: "your-relying-party.example.com",
    apiAuthId: "your-api-auth-id",
    apiAuthType: FssApiAuthType.NonceSignAuth,
    secretKey: "YOUR_BASE64URL_ENCODED_PKCS8_EC_PRIVATE_KEY"
);

// SDKクライアントの作成 (IDisposable を実装)
using var sdk = new YubiOnFssSdk(config);
```

### 2. ユーザー登録

```csharp
using YubiOn.Fss.Sdk.Data.User;

var registerParam = new UserDataRegisterParameter(
    userId: "user_12345",
    userName: "alice@example.com",
    displayName: "Alice Smith"
);

var response = await sdk.RegisterUserAsync(registerParam);
Console.WriteLine($"ユーザー作成成功: {response.User.UserName} (ID: {response.User.UserId})");
```

### 3. FIDO2 クレデンシャル登録フロー (Passkey登録)

```csharp
using YubiOn.Fss.Sdk.Data.Fido;

// Step 1: 登録セレモニーの開始 (サーバーからクレデンシャル作成オプションを取得)
var startParam = new Fido2StartRegisterParameter
{
    UserId = "user_12345",
    UserName = "alice@example.com"
};

var startResponse = await sdk.StartRegisterCredentialAsync(startParam);
string sessionCookie = startResponse.SessionCookie; // セッション間で保持

// クライアント側（ブラウザ/WebAuthn API）へ渡すJSON
var creationOptions = startResponse.Data.CreationOptions;

// Step 2: クライアント側で navigator.credentials.create() を実行後、結果を受信して完了
var finishParam = new Fido2FinishRegisterParameter
{
    Credential = clientPublicKeyCredentialJson // ブラウザから返却されたJSONデータ
};

var finishResponse = await sdk.FinishRegisterCredentialAsync(finishParam, sessionCookie);
Console.WriteLine($"クレデンシャル登録完了: {finishResponse.Credential.CredentialId}");
```

### 4. FIDO2 認証フロー (ログイン)

```csharp
// Step 1: 認証セレモニーの開始
var authStartParam = new Fido2StartAuthenticateParameter
{
    UserName = "alice@example.com"
};

var authStartResponse = await sdk.StartAuthenticateAsync(authStartParam);
string sessionCookie = authStartResponse.SessionCookie;

// Step 2: クライアント側で navigator.credentials.get() を実行後、結果を受信して完了
var authFinishParam = new Fido2FinishAuthenticateParameter
{
    Credential = clientAuthCredentialJson
};

var authFinishResponse = await sdk.FinishAuthenticateAsync(authFinishParam, sessionCookie);
Console.WriteLine($"認証成功! 認証されたユーザーID: {authFinishResponse.Credential.UserId}");
```

---

## テストの実行

### 単体テスト (オフライン実行可能)
外部サーバー接続を必要としないモックテスト、エミュレータテスト、暗号検証テストが含まれています。
```bash
dotnet test
```

### 結合テスト (実サーバー接続)
実サーバーへのE2Eテストを実行する場合は、`tests/YubiOn.Fss.Sdk.Tests/` 配下に `environment.properties` を作成するか、環境変数（`FSS_TEST_*`）を設定してください。設定が存在する場合のみ自動的に実行されます。

| 環境変数名 / プロパティ名 | 内容 |
|---|---|
| `FSS_TEST_ENDPOINT` | テスト用 API エンドポイント URL (省略可) |
| `FSS_TEST_RP_ID` | テスト用 RP ID (※データが変更されるため本番環境のRPIDは指定しないでください) |
| `FSS_TEST_NONCE_API_AUTH_ID` | Nonce 認証用 API 認証 ID |
| `FSS_TEST_NONCE_CORRECT_API_SECRET_KEY` | Nonce 認証用の正しい秘密鍵 (PKCS#8 Base64URL) |
| `FSS_TEST_NONCE_INCORRECT_API_SECRET_KEY` | 認証エラー確認用の誤った秘密鍵 |
| `FSS_TEST_DATETIME_API_AUTH_ID` | 日時認証用 API 認証 ID |
| `FSS_TEST_DATETIME_CORRECT_API_SECRET_KEY` | 日時認証用の正しい秘密鍵 |
| `FSS_TEST_ACCESSKEY_API_AUTH_ID` | アクセスキー認証用 API 認証 ID |
| `FSS_TEST_ACCESSKEY_CORRECT_API_SECRET_KEY` | アクセスキー認証用の正しいシークレットキー |

---

## ドキュメント

- [API リファレンス (日本語)](api-reference.ja.md)
- [API Reference (English)](api-reference.md)

---

## ライセンス

本プロジェクトは [MIT License](LICENSE) の下で公開されています。  
Copyright (c) SOFTGIKEN Co.,Ltd. All rights reserved.
