# YubiOn FIDO2 Server Service (FSS) .NET SDK サンプルアプリケーション

これは、**YubiOn FIDO2 Server Service SDK for .NET** を使用して、ASP.NET Core Web API（バックエンド）および Vue 3 SPA（フロントエンド）による FIDO2 パスキー登録・認証・クレデンシャル管理を実装したサンプルアプリケーションです。

Node.js や npm のインストールは一切不要で、**.NET SDK または Visual Studio だけで一括起動**して動作を確認できます。

---

## 主な機能

- **ユーザー新規登録 & パスキー登録**: ユーザー名と表示名を入力し、FIDO2 Authenticator（Windows Hello / Touch ID / YubiKey等）でパスキーを作成して登録
- **パスキーログイン**: 登録したパスキーを用いてパスワードレスでログイン
- **ユーザー一覧表示**: 登録されているユーザー一覧の確認
- **クレデンシャル管理**:
  - ユーザーに紐づくパスキー一覧の確認
  - ログイン中ユーザーへの新しいパスキー（予備キーなど）の追加登録
  - 不要になったパスキーの削除

---

## 事前準備

サンプルを実行する前に、FSS 管理 WEB コンソールで RP 関連の設定を行います。

1. **RP（Relying Party）の登録**
   - FSS 管理 WEB で、アプリケーションを新しい RP として登録します。
   - ローカル開発の場合、RP ID として `.localhost` で終わるドメイン名を使用できます（例: `your-app.localhost`）。
2. **API キーの作成**
   - 登録した RP に対して、新しい API キーを作成します。
   - **API 認証種別**は、「**Nonce署名認証**」（推奨）を選択してください。
3. **接続設定（appsettings.json または環境変数）**
   - `sample/YubionFssSdkSample/appsettings.json` を開き、取得した情報を設定します。
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
   *※環境変数 `FSS_RP_ID`、`FSS_API_KEY_ID`、`FSS_API_SECRET` による上書き設定も可能です。*

---

## アプリケーションの実行方法

### 方法 1: Visual Studio で起動（推奨）

1. `sample/YubionFssSdkSample.sln` を Visual Studio 2022 で開きます。
2. `YubionFssSdkSample` をスタートアッププロジェクトに設定します。
3. `F5`（デバッグ起動）または `Ctrl + F5`（デバッグなしで開始）を押します。
4. ブラウザが自動的に起動します。

### 方法 2: PowerShell スクリプトで起動

PowerShell を開き、`sample` ディレクトリ内のスクリプトを実行します。

```powershell
cd sample
./run.ps1
```

または、ポートや URL を指定して起動することも可能です：
```powershell
./run.ps1 -Url "http://your-app.localhost:5000"
```

### 方法 3: .NET CLI で起動

```bash
cd sample/YubionFssSdkSample
dotnet run --urls "http://your-app.localhost:5000"
```

---

## アプリケーションへのアクセス

WebAuthn（パスキー）の仕様により、FSS に登録した **RP ID** とブラウザでアクセスするドメインが一致している必要があります。

1. ブラウザを開き、設定した RP ID でアクセスします：
   ```text
   http://<あなたのRPID>:5000
   ```
   例: RP ID が `your-app.localhost` の場合:
   ```text
   http://your-app.localhost:5000
   ```

> [!TIP]
> **ローカル開発時の注意点**:
> - `.localhost` で終わるドメイン（例: `myapp.localhost`）は、モダンブラウザ（Chrome, Edge, Firefox等）では標準で `127.0.0.1` にループバック解決され、かつ Secure Context（安全なコンテキスト）として扱われるため、HTTPS なしでも WebAuthn が正常に機能します。
> - それ以外のドメインを使用する場合は、OS の `hosts` ファイルで `127.0.0.1` を向ける設定を行ってください。

---

## プロジェクト構成

```text
sample/
├── YubionFssSdkSample.sln         # サンプルソリューション
├── YubionFssSdkSample/            # ASP.NET Core Web API + SPA
│   ├── YubionFssSdkSample.csproj  # SDKへのプロジェクト相対参照
│   ├── Program.cs                 # サービス構成・セッション・静的ファイル配信
│   ├── Controllers/
│   │   ├── AuthController.cs      # 登録・ログイン・セッション管理
│   │   └── UsersController.cs     # ユーザー・クレデンシャル管理
│   ├── Models/                    # DTO モデルクラス
│   ├── appsettings.json           # 接続設定
│   ├── appsettings.example.json   # 接続設定テンプレート
│   └── wwwroot/                   # フロントエンド (Vue 3 SPA)
│       ├── index.html             # SPA エントリHTML
│       ├── css/style.css          # スタイルシート
│       └── js/
│           ├── app.js             # Vue 3 SPA本体 (画面遷移・UIロジック)
│           └── yubion-fido2-server-sdk-client.min.js # WebAuthnバイナリ変換ライブラリ
├── run.ps1                        # PowerShell起動スクリプト
├── README.ja.md                   # 本ドキュメント
└── README.md                      # 英語版ドキュメント
```
