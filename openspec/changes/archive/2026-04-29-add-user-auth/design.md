## Context

### 現有可重用資產

後端會員認證所需的基礎設施已經完成 80%，本次屬於把零件接上來：

| 資產 | 位置 | 狀態 |
|------|------|------|
| `User` entity | `api/Domain/Entities/User.cs` | ✅ 完整（Email, PasswordHash?, Avatar, NotificationType, Status, EmailVerified, IsGoogleConnected, LastLoginAt） |
| `UserRefreshToken` entity | `api/Domain/Entities/UserRefreshToken.cs` | ✅ |
| `VerificationEmail` entity + Type enum | `api/Domain/Entities/VerificationEmail.cs` + `Domain/Enums/VerificationEmailType.cs` | ✅ |
| `IJwtTokenService` / `JwtTokenService` | `api/Application/Interfaces/` + `api/Infrastructure/Services/` | ✅ User.API 已 `AddJwtAuthentication` |
| `IPasswordHasher`（Argon2id + Pepper） | `api/Application/Interfaces/IPasswordHasher.cs` | ✅ |
| `HttpCurrentUserContext` | `api/Infrastructure/Services/` | ✅ 從 `ClaimTypes.NameIdentifier` 取 UserId |
| Error codes 4-03-XX | `api/Application/Constants/ErrorCodes.cs` | ✅ 全部已預留（INVALID_CREDENTIALS, EMAIL_NOT_VERIFIED, ACCOUNT_INACTIVE, INVALID_TOKEN, EMAIL_ALREADY_EXISTS） |
| `AdminAuthService` 完整參考實作 | `api/Application/Services/AdminAuthService.cs` | ✅ 直接照抄 GenerateAndSaveTokens / RevokeAllTokens / Refresh Rotation 流程 |
| Admin 端 auth client 完整參考 | `web/apps/admin/app/stores/auth.ts` + `composable/useAdminApi.ts` + `middleware/auth.global.ts` | ✅ 直接 port 到 client |
| `VerificationService.CreateAndSendVerification` | `api/Application/Services/VerificationService.cs` | ⚠️ 已生成 token + 寫 DB，但實際發信為 TODO |

### 現有 hardcode 痛點

`web/apps/client/app/layouts/default.vue`：
- L84：avatar 寫死 `dicebear seed=Felix`
- L94：dropdown 顯示 `Alex (Creative Dept)`
- L113：Logout 按鈕無 click handler
- L160：Footer 寫死 `Current User: Alex (Creative Dept)`

## Goals / Non-Goals

**Goals:**
- User.API 提供完整的 Email/Password 註冊 → 驗證 → 登入 → Refresh → 登出 → Profile 流程
- Refresh Token Rotation 行為與 admin 端一致（含「重複使用偵測撤銷全部」邏輯）
- Client 完整接線：未登入導 `/login`，登入後 layout 全部反映真實會員
- 驗證信寄送以 `IEmailSender` 抽象封裝，目前 dev 期間用 Serilog log 出來，未來換真 mailer 不用改 service code
- Client 採用 Pinia 作為 state management（與 admin 一致），不再混用 `useState`

**Non-Goals:**
- Google OAuth（保留 `IsGoogleConnected` 欄位但 endpoint / 整合留 future change）
- 真實 SMTP / SendGrid / Resend 實作
- Forgot Password 流程（`VerificationEmailType.ForgotPassword` 已存在，但本次不暴露對應 endpoint）
- 「mock 期間」的種子帳號 / seed 資料（不在 spec 範圍）
- 帳號註銷（軟刪除）— spec 沒提，不做

## Decisions

### Decision 1: `IEmailSender` 抽象 + Serilog 實作

**選擇**：定義 `Drink.Application.Interfaces.IEmailSender`，dev 期間用 `Drink.Infrastructure.Services.LogEmailSender` 把 `to / subject / verificationLink` 寫入 Serilog。`VerificationService.CreateAndSendVerification` 改注入 `IEmailSender` 並呼叫之。

```csharp
public interface IEmailSender
{
  Task SendVerificationEmailAsync(string to, VerificationEmailType type, string token);
}

// Infrastructure/Services/LogEmailSender.cs
public class LogEmailSender : IEmailSender
{
  private readonly ILogger<LogEmailSender> _logger;
  // TODO: 之後改為真實 SMTP / SendGrid / Resend，刪掉這個實作
  public Task SendVerificationEmailAsync(string to, VerificationEmailType type, string token)
  {
    _logger.LogInformation("[MOCK EMAIL] to={To} type={Type} token={Token}", to, type, token);
    return Task.CompletedTask;
  }
}
```

**理由**：使用者明確要求 Email 寄送先用 Serilog mock，但希望抽象化讓之後好替換。`VerificationService` 已有 `CreateAndSendVerification` 私有方法且邏輯完整，只剩寄信尚未實作 — 此 design 把 TODO 轉成 DI 接口。

**Alternatives considered**：
- 不抽象，直接在 `VerificationService` 內 `_logger.LogInformation` — 之後換真 mailer 要改 service 程式碼，違反「之後好替換」目標
- 直接接 SMTP — 使用者明確指定先 mock

### Decision 2: Google OAuth 完全不做

**選擇**：不寫 endpoint、不寫 mock validator、appsettings 不加 `GoogleAuth` 區塊、`/login` 頁不放 Google 按鈕。`IsGoogleConnected` 欄位保留（migration 已建立）但永遠 false。

**理由**：使用者要求「之後再補」。寫 mock 但不接真 Google 反而會留下要刪的死碼。

### Decision 3: Client 改用 Pinia

**選擇**：在 `web/apps/client/package.json` 新增 `pinia` + `@pinia/nuxt`，在 `nuxt.config.ts` 的 `modules` 加入 `'@pinia/nuxt'`。auth state 走 `defineStore('auth', ...)`，token 透過 `useCookie` 跨 SPA reload 持久化（admin 端同樣模式）。

**理由**：使用者明確要求 Pinia。與 admin store 寫法一致便於對照維護。

**Alternatives considered**：
- 用 Nuxt 內建 `useState` — 使用者明確拒絕

### Decision 4: Refresh Token Rotation 行為對齊 admin

**選擇**：UserAuthService 完整 port `AdminAuthService.Refresh` 邏輯：
1. 找對應 token，不存在或過期 → `INVALID_TOKEN` 400
2. 已被撤銷（`RevokedAt is not null`）→ 撤銷該 user 所有 token，回 `INVALID_TOKEN`（重複使用偵測）
3. 確認 user 仍 Active；非 Active → `ACCOUNT_INACTIVE` 403
4. 撤銷舊 token、發新 access + refresh

**理由**：spec 與 admin 端行為一致，且 admin 端是經過驗證的實作。

### Decision 5: Login 帳號狀態判斷順序

**選擇**：`Login` 流程依序判斷：
1. Email 找不到 / 密碼錯 → `INVALID_CREDENTIALS` 401
2. `Status == Inactive` → `ACCOUNT_INACTIVE` 403
3. `EmailVerified == false` → `EMAIL_NOT_VERIFIED` 403
4. 通過 → 更新 `LastLoginAt`，發 token

**理由**：先驗密碼再判狀態，避免「枚舉 email 看是否存在」的 enumeration attack。Status / EmailVerified 順序依使用者體驗排（停用比未驗證更嚴重）。

### Decision 6: Email 唯一性 — 不分大小寫

**選擇**：`UserAuthService.Register` 比對 email 時用 `EF.Functions.ILike` 或 `string.Equals(... StringComparison.OrdinalIgnoreCase)`，DB 儲存時也轉 lowercase。

**理由**：legacy spec 明訂「Email 唯一（不分大小寫）」。

### Decision 7: Client 401 retry 策略

**選擇**：完全 port `useAdminApi.ts` 的 middleware 模式 — `onResponse` 攔 401，有 refresh_token 就嘗試 `refresh()`，成功就用新 token 重打原 request；失敗或無 refresh_token 就 `clearTokens()` + `navigateTo('/login')`。`isRefreshing` flag 防併發 refresh。

**理由**：admin 端模式已驗證可用，無需重新發明。

### Decision 8: Auth Middleware 的 public 路由白名單

**選擇**：`middleware/auth.global.ts` 視為「預設要登入」，下列路徑為 public：
- `/login`
- `/register`
- `/verify-email`（需要在 query string 收 token）

已登入訪 `/login` 或 `/register` → 導回 `/`。

**理由**：與 admin 同模式，僅多出 register / verify-email 兩個 public 路徑。

### Decision 9: UserAuthMapper / UserProfileMapper 切兩個

**選擇**：`UserAuthMapper`（負責 Login/Refresh response 組合）與 `UserProfileMapper`（負責 `User → UserProfileResponse` 與 `UpdateProfileRequest → User` 部分映射）拆開兩個檔。

**理由**：對齊現有 module 切分（`AdminUserMapper`、`AdminAuthService` 各自獨立），避免一個 Mapper 涵蓋過多功能。

## Risks / Trade-offs

- **[Serilog mock 寄信被部署到非 dev 環境]** → DI 註冊只在 User.API 啟用，並在 `LogEmailSender` 加 `// TODO: replace before production` 註解；同時在 proposal / tasks 標明「上線前必須替換」
- **[Pinia 引入的 hydration 風險]** → client 已 `ssr: false`（純 SPA），不會有 SSR/CSR mismatch；`useCookie` 在 SPA 模式下退化為 `localStorage` 包裝
- **[註冊後使用者看不到驗證信]** → 開發期間透過 dev console 取 verification link；Register 成功頁顯式提示「請至 dev log 取得驗證連結」，避免 UX 困惑
- **[email 大小寫一致性]** → 註冊時統一轉 lowercase 寫入；查詢時 ILike 比對。要保留使用者輸入大小寫請改 schema 加 `EmailNormalized` 欄位 — 本次不做，spec 也不要求
- **[401 retry 死循環]** → `isRefreshing` flag + 單一 `refreshPromise` 防併發；refresh 失敗即 `clearTokens` 不再嘗試（與 admin 端一致）
- **[`UpdateProfileRequest` 含 avatar 但圖片上傳尚未串通]** → 本次只接受 `avatar` 為 string URL（前端可先傳 dicebear 連結或從 Upload.API 拿到的 URL），不在這個 change 處理 avatar upload UI

## Migration Plan

純 additive，不需要 DB migration、不需要既有資料轉換。部署順序：

1. 部署後端：User.API 新增 endpoint，舊 client 不受影響（仍是 hardcode）
2. 部署前端：client 加入 auth store / middleware / pages
3. 既有未登入使用者第一次造訪會被導去 `/login`

Rollback：純前端 / 純後端各自獨立可 rollback；後端 endpoint 為新增，rollback 不影響既有功能。

## Open Questions

無。範圍與決策都已在 proposal 階段對齊。
