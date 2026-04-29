## ADDED Requirements

### Requirement: Email/Password 註冊

User.API SHALL 提供 `POST /api/user/auth/register` 端點接受 `name`、`email`、`password`，建立 `EmailVerified=false`、`Status=Active`、`IsGoogleConnected=false`、`PasswordHash` 為 Argon2id（Salt + Pepper）雜湊的 User，並透過 `IEmailSender` 寄送 Register 類型的驗證信。Email 唯一性比對 MUST 不分大小寫。

#### Scenario: 註冊成功

- **WHEN** 客戶端送出有效的 register 請求且 email 不重複
- **THEN** 系統建立 User 並設 `EmailVerified=false`、`Status=Active`、`IsGoogleConnected=false`、`Creator=0`、`Updater=0`，建立 `VerificationEmail`（Type=Register、有效期 24 小時），透過 `IEmailSender.SendVerificationEmailAsync` 觸發寄送，回傳 `200` + `code=0`

#### Scenario: Email 已存在（不分大小寫）

- **WHEN** 客戶端以 `Wayne@Example.com` 註冊但 DB 已有 `wayne@example.com`
- **THEN** 系統回傳 `409` + `code=40301` + `error="EMAIL_ALREADY_EXISTS"`，且 `errors={ "email": ["Email 已被註冊"] }`

#### Scenario: 必填欄位缺漏

- **WHEN** 客戶端送出缺少 `name` / `email` / `password` 任一欄位的請求
- **THEN** 系統回傳 `400` + `code=40001` + `error="VALIDATION_ERROR"`，並在 `errors` 字典標出具體缺漏欄位

### Requirement: Email 驗證

User.API SHALL 提供 `POST /api/user/auth/verify-email` 端點接受 `token`，驗證對應 `VerificationEmail`（Type=Register）未過期、未被使用，成功後 MUST 將該 User `EmailVerified` 設為 `true` 並標記 `VerificationEmail.IsUsed=true` + `UsedAt=now`。

#### Scenario: Token 有效

- **WHEN** 客戶端以有效未使用的 token 呼叫端點
- **THEN** 系統將對應 User `EmailVerified=true`、對應 `VerificationEmail` `IsUsed=true` 且 `UsedAt` 設為當下時間，回傳 `200` + `code=0`

#### Scenario: Token 不存在或過期

- **WHEN** 客戶端送出查無紀錄、`ExpiresAt` 已過、或 Type 不為 Register 的 token
- **THEN** 系統回傳 `400` + `code=40306` + `error="INVALID_TOKEN"`

#### Scenario: Token 已使用

- **WHEN** 客戶端送出已被標記 `IsUsed=true` 的 token
- **THEN** 系統回傳 `400` + `code=40307` + `error="TOKEN_ALREADY_USED"`

### Requirement: Email/Password 登入

User.API SHALL 提供 `POST /api/user/auth/login` 端點接受 `email`、`password`，依序檢查憑證、帳號狀態、Email 驗證狀態，全部通過後 MUST 更新 `LastLoginAt` 並回傳 `access_token`（短效，依 `JwtSettings.AccessTokenExpirationMinutes`）+ `refresh_token`（長效，依 `JwtSettings.RefreshTokenExpirationDays`）。`UserRefreshToken` 記錄 MUST 同步建立。

#### Scenario: 登入成功

- **WHEN** 客戶端送出 `email` 存在、密碼正確、`Status=Active` 且 `EmailVerified=true` 的 User 的請求
- **THEN** 系統更新該 User `LastLoginAt`、建立新 `UserRefreshToken`（`ExpiresAt = now + RefreshTokenExpirationDays`、`RevokedAt=null`），回傳 `200` + `data={ access_token, refresh_token }`

#### Scenario: Email 不存在或密碼錯誤

- **WHEN** 客戶端送出查無對應 email、或密碼經 Argon2id 驗證失敗的請求
- **THEN** 系統回傳 `401` + `code=40302` + `error="INVALID_CREDENTIALS"`，回應 MUST 不洩漏「email 是否存在」的資訊

#### Scenario: 帳號被停用

- **WHEN** 通過密碼驗證但 User `Status=Inactive`
- **THEN** 系統回傳 `403` + `code=40304` + `error="ACCOUNT_INACTIVE"`

#### Scenario: Email 尚未驗證

- **WHEN** 通過密碼驗證、`Status=Active`，但 `EmailVerified=false`
- **THEN** 系統回傳 `403` + `code=40303` + `error="EMAIL_NOT_VERIFIED"`

### Requirement: Refresh Token Rotation

User.API SHALL 提供 `POST /api/user/auth/refresh` 端點接受 `refresh_token`，採 Refresh Token Rotation：每次 refresh 撤銷舊 token、發放新的 access_token + refresh_token，並 MUST 偵測重複使用攻擊。

#### Scenario: Refresh 成功

- **WHEN** 客戶端送出未過期、未被撤銷、對應 User `Status=Active` 的 refresh_token
- **THEN** 系統將舊 token `RevokedAt=now`、建立新 `UserRefreshToken`、回傳 `200` + `data={ access_token, refresh_token }`

#### Scenario: Token 無效或過期

- **WHEN** 客戶端送出查無紀錄或 `ExpiresAt` 已過的 token
- **THEN** 系統回傳 `400` + `code=40306` + `error="INVALID_TOKEN"`

#### Scenario: 重複使用偵測

- **WHEN** 客戶端送出 `RevokedAt` 已設值的 refresh_token
- **THEN** 系統 MUST 透過 `ExecuteUpdate` 將該 User 所有 `RevokedAt=null` 的 token 全部撤銷，並回傳 `400` + `code=40306` + `error="INVALID_TOKEN"`

#### Scenario: 用戶已停用

- **WHEN** 客戶端送出有效 refresh_token 但對應 User `Status=Inactive`
- **THEN** 系統回傳 `403` + `code=40304` + `error="ACCOUNT_INACTIVE"`

### Requirement: 登出

User.API SHALL 提供 `POST /api/user/auth/logout` 端點（需 `[Authorize]`）接受 `refresh_token`，撤銷該 token。logout 操作 MUST 為冪等：傳入不存在或已撤銷的 token 仍回 `200`，避免洩漏 token 狀態。

#### Scenario: 撤銷有效 token

- **WHEN** 已認證使用者送出自己有效未撤銷的 refresh_token
- **THEN** 系統將該 token `RevokedAt=now`，回傳 `200` + `code=0`

#### Scenario: token 不存在或已撤銷

- **WHEN** 已認證使用者送出查無紀錄、或 `RevokedAt` 已設值的 token
- **THEN** 系統不修改任何資料，回傳 `200` + `code=0`

### Requirement: 驗證信寄送抽象

`api/Application/Interfaces/IEmailSender` SHALL 提供 `SendVerificationEmailAsync(to, type, token)` 方法。`VerificationService.CreateAndSendVerification` MUST 在建立 `VerificationEmail` 紀錄後呼叫此方法觸發寄送。Dev 階段 SHALL 註冊 `LogEmailSender` 實作（透過 Serilog 輸出收件人、類型與 token，不實際發信）。

#### Scenario: 註冊時觸發寄送

- **WHEN** Register 流程執行 `CreateAndSendVerification`
- **THEN** 系統建立 `VerificationEmail` 後呼叫 `IEmailSender.SendVerificationEmailAsync`，dev 期間 `LogEmailSender` 透過 Serilog 輸出 `[MOCK EMAIL] to=... type=Register token=...`

#### Scenario: 後台重發驗證信

- **WHEN** 後台 `VerificationsController.Resend` 觸發 `CreateAndSendVerification`
- **THEN** 系統同樣呼叫 `IEmailSender.SendVerificationEmailAsync`，行為與註冊時一致
