## Why

前台 client 目前所有的「登入用戶」資訊都是 hardcode（layout 顯示 `Alex (Creative Dept)` + `dicebear seed=Felix`、Logout 按鈕沒接任何邏輯），且 User.API 沒有任何認證相關的 endpoint。後續所有需要識別會員身份的功能（揪團、訂單、profile）都被這個前置條件卡住，因此必須一次把會員認證從 0 補到能登入登出的狀態。

## What Changes

### Backend (User.API)

- 新增 `POST /api/user/auth/register`：建立未驗證帳號，發送驗證信
- 新增 `POST /api/user/auth/verify-email`：以 token 完成 Email 驗證
- 新增 `POST /api/user/auth/login`：Email + Password 登入，回傳 access_token + refresh_token，更新 `LastLoginAt`
- 新增 `POST /api/user/auth/refresh`：Refresh Token Rotation（重複使用偵測會撤銷該用戶所有 token）
- 新增 `POST /api/user/auth/logout`：撤銷指定 refresh_token
- 新增 `GET /api/user/profile`：取得當前用戶 profile
- 新增 `PUT /api/user/profile`：更新 name / avatar / notification_type
- 新增 `IEmailSender` 介面 + `LogEmailSender` 實作（透過 Serilog log 出收件人 / 主旨 / 驗證連結；標 TODO 之後接真實 mailer）
- `VerificationService.CreateAndSendVerification` 移除 TODO，改透過 `IEmailSender.SendAsync` 寄送

### Frontend (web/apps/client)

- 加入 `pinia` + `@pinia/nuxt`，client 不再純靠 `useState`
- 新增 `stores/auth.ts`（login / register / verifyEmail / refresh / logout / fetchProfile，token 透過 `useCookie` 持久化）
- 新增 `composables/useUserApi.ts`（仿 admin 的 useAdminApi，注入 Authorization header + 401 → refresh → retry → 失敗導 `/login`）
- 新增 `middleware/auth.global.ts`（未登入導 `/login`，已登入訪問 `/login` / `/register` 導回首頁）
- 新增 page：`/login`、`/register`、`/verify-email`
- 修改 `layouts/default.vue`：header avatar / dropdown / footer status bar 全部改用 auth store 的真實 user，Logout 接 `store.logout()`，未登入時 dropdown 顯示「登入」入口
- 修改 `pages/profile.vue`：接 `GET /profile` + `PUT /profile`

### Codegen

- 後端完工後執行 `pnpm generate` 重新產出 `web/internal/api-types/src/user.d.ts`

### 不在本次範圍

- Google OAuth（`/api/user/auth/google` + Google Identity Services 接線）— 留 future work
- 真實 SMTP / SendGrid / Resend 寄信 — 由後續 change 替換 `LogEmailSender`
- Forgot Password 流程（已預留 `VerificationEmailType.ForgotPassword`，本次不實作 endpoint）

## Capabilities

### New Capabilities

- `user-auth`: 前台會員的認證流程（註冊、Email 驗證、登入、Refresh Token Rotation、登出），不含 Google OAuth
- `user-profile`: 前台會員自身的個人資料讀取與編輯（不含 Email / Password / Status，這些另有流程）

### Modified Capabilities

- `client-layout`: Header 右側 avatar dropdown、Footer Status Bar 必須反映目前登入會員，未登入時提供登入入口；新增全域 auth middleware 守護登入狀態

## Impact

### Affected code

- `api/User.API/`：新增 `Controllers/AuthController.cs`、`Controllers/ProfileController.cs`
- `api/Application/`：新增 `Services/UserAuthService.cs`、`Services/UserProfileService.cs`、`Interfaces/IEmailSender.cs`、`Mappings/UserAuthMapper.cs`、`Mappings/UserProfileMapper.cs`、Request/Response DTOs；`Services/VerificationService.cs` 接上 `IEmailSender`
- `api/Infrastructure/`：新增 `Services/LogEmailSender.cs`，DI 註冊 `IEmailSender`
- `web/apps/client/`：新增 `stores/auth.ts`、`composables/useUserApi.ts`、`middleware/auth.global.ts`、`pages/login.vue`、`pages/register.vue`、`pages/verify-email.vue`；修改 `layouts/default.vue`、`pages/profile.vue`、`nuxt.config.ts`、`package.json`
- `web/internal/api-types/`：自動更新 `src/user.d.ts`

### Dependencies

- 新增前端 dependency：`pinia`、`@pinia/nuxt`
- 後端不新增任何 NuGet 套件（Argon2 / JWT / Serilog 都已存在）

### Database

- **不需要 migration**：`User`、`UserRefreshToken`、`VerificationEmail` entity 都已存在

### Configuration

- User.API `appsettings.json` 已含 `Jwt` 與 `Security:Pepper`，**不需新增**任何 key（驗證信目前不需要 SMTP 設定，純靠 Serilog）
