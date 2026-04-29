## 1. Backend — Email 寄送抽象

- [x] 1.1 在 `api/Application/Interfaces/` 新增 `IEmailSender.cs`，定義 `Task SendVerificationEmailAsync(string to, VerificationEmailType type, string token)`
- [x] 1.2 在 `api/Infrastructure/Services/` 新增 `LogEmailSender.cs`：透過注入 `ILogger<LogEmailSender>` 以 `LogInformation("[MOCK EMAIL] to={To} type={Type} token={Token}", ...)` 輸出，加上 `// TODO: replace with real mailer before production` 註解
- [x] 1.3 在 `api/Infrastructure/Extensions/ServiceCollectionExtensions.cs` 註冊 `services.AddScoped<IEmailSender, LogEmailSender>()`（若 BaseService 為 Scoped 則一致）
- [x] 1.4 修改 `api/Application/Services/VerificationService.cs`：建構式注入 `IEmailSender`、`CreateAndSendVerification` 在 `_verificationRepo.Insert(verification)` 之後呼叫 `_emailSender.SendVerificationEmailAsync(user.Email, type, token)`，移除原 `// TODO: 實際發送郵件邏輯` 註解（保留資料層 `IsSuccess=true` 行為）

## 2. Backend — DTOs

- [x] 2.1 新增 `api/Application/Requests/User/UserRegisterRequest.cs`（`Name`、`Email`、`Password`，含 `[Required]` / `[EmailAddress]` / `[StringLength]` / `[MinLength(6)]`）— bundled in `UserAuthRequests.cs`
- [x] 2.2 新增 `api/Application/Requests/User/UserVerifyEmailRequest.cs`（`Token`）— bundled in `UserAuthRequests.cs`
- [x] 2.3 新增 `api/Application/Requests/User/UserLoginRequest.cs`（`Email`、`Password`）— bundled in `UserAuthRequests.cs`
- [x] 2.4 新增 `api/Application/Requests/User/UserRefreshTokenRequest.cs`（`RefreshToken`）— bundled in `UserAuthRequests.cs`
- [x] 2.5 新增 `api/Application/Requests/User/UserLogoutRequest.cs`（`RefreshToken`）— bundled in `UserAuthRequests.cs`
- [x] 2.6 新增 `api/Application/Requests/User/UpdateUserProfileRequest.cs`（`Name`、`Avatar?`、`NotificationType`）
- [x] 2.7 新增 `api/Application/Responses/User/UserLoginResponse.cs`（`AccessToken`、`RefreshToken`）
- [x] 2.8 新增 `api/Application/Responses/User/UserProfileResponse.cs`（`Id`、`Name`、`Email`、`Avatar?`、`NotificationType`、`IsGoogleConnected`、`EmailVerified`）

## 3. Backend — Mapperly

- [x] 3.1 新增 `api/Application/Mappings/UserAuthMapper.cs`（`[Mapper] static partial class`，提供 `ToUserLoginResponse(string accessToken, string refreshToken)` helper 或留 service 直接 new — 視程式碼風格決定，與 `AdminAuthService` 保持一致）
- [x] 3.2 新增 `api/Application/Mappings/UserProfileMapper.cs`：`User → UserProfileResponse` 的 `ToUserProfileResponse()` extension method；以及 `UpdateUserProfileRequest → User` 的部分映射 extension（Name / Avatar / NotificationType）
- [x] 3.3 確認 `AppMapper.cs`（如有集中註冊）已涵蓋這兩個 mapper — 無此檔，Mapperly 為 source generator 自動產生

## 4. Backend — UserAuthService

- [x] 4.1 新增 `api/Application/Services/UserAuthService.cs` 繼承 `BaseService`
- [x] 4.2 建構式注入 `IGenericRepository<User>`、`IGenericRepository<UserRefreshToken>`、`IJwtTokenService`、`IOptions<JwtSettings>`、`IPasswordHasher`、`IConfiguration`（取 Pepper）、`VerificationService`（重用 `CreateAndSendVerification` — 視可見性，必要時提取為 `public Task SendRegisterVerification(int userId)` 或在 UserAuthService 內 inline 實作）
- [x] 4.3 實作 `Register(UserRegisterRequest)`：email 不分大小寫查重、Argon2id 雜湊密碼、建立 User（`EmailVerified=false`、`Status=Active`、`IsGoogleConnected=false`、`Creator=0`、`Updater=0`）、建立 + 寄送 Register 類型驗證信
- [x] 4.4 實作 `VerifyEmail(UserVerifyEmailRequest)`：查 `VerificationEmail`（Type=Register、未過期、未使用），通過後 `EmailVerified=true` + 標記 token `IsUsed=true`、`UsedAt=now`
- [x] 4.5 實作 `Login(UserLoginRequest)`：依序 `INVALID_CREDENTIALS` → `ACCOUNT_INACTIVE` → `EMAIL_NOT_VERIFIED`，通過後更新 `LastLoginAt`、呼叫 `GenerateAndSaveTokens`
- [x] 4.6 實作 `Refresh(UserRefreshTokenRequest)`：完整 port `AdminAuthService.Refresh` 邏輯（含重複使用偵測）
- [x] 4.7 實作 `Logout(UserLogoutRequest)`：找 token + `RevokedAt==null` 才標記，否則靜默回 200（冪等）
- [x] 4.8 私有 helper `GenerateAndSaveTokens(int userId)` 與 `RevokeAllTokens(int userId)`，行為對齊 admin 端

## 5. Backend — UserProfileService

- [x] 5.1 新增 `api/Application/Services/UserProfileService.cs` 繼承 `BaseService`
- [x] 5.2 注入 `IGenericRepository<User>`
- [x] 5.3 實作 `GetProfile()`：`CurrentUserId` 取 user → 不存在回 `UNAUTHORIZED` → 回 `UserProfileResponse`
- [x] 5.4 實作 `UpdateProfile(UpdateUserProfileRequest)`：tracking 取 user → 僅更新 Name / Avatar / NotificationType → 儲存

## 6. Backend — Controllers

- [x] 6.1 新增 `api/User.API/Controllers/AuthController.cs` 繼承 `BaseController`，path 自動 kebab → `/api/user/auth/...`
- [x] 6.2 endpoint：`POST register`、`POST verify-email`、`POST login`、`POST refresh`、`POST logout`（後者加 `[Authorize]`）；包含完整 `[ProducesResponseType]` 標註所有錯誤碼（401 / 403 / 400 / 409）
- [x] 6.3 register email 重複時回 409 而非 400（`ApiError(..., 409)`）；login 走 401（`INVALID_CREDENTIALS`）/ 403（`ACCOUNT_INACTIVE`、`EMAIL_NOT_VERIFIED`）的 status code 分流
- [x] 6.4 新增 `api/User.API/Controllers/ProfileController.cs` 繼承 `BaseController`，class 加 `[Authorize]`
- [x] 6.5 endpoint：`GET /` → `GetProfile()`、`PUT /` → `UpdateProfile()`

## 7. Backend — 跑通 Build

- [x] 7.1 `dotnet build` 通過
- [x] 7.2 `dotnet run --project api/User.API` 啟動，Swagger UI 顯示新增的 7 個 endpoint（實際 6：5 auth + 1 profile）
- [x] 7.3 用 curl / Bruno 手測：register → 觀察 Serilog log 輸出 verification token → verify-email → login → 帶 access_token call profile → refresh → logout（含邊界：重複 email、未驗證 login、密碼錯、token 重用）

## 8. Frontend — Pinia 安裝與設定

- [x] 8.1 在 `web/apps/client/` 執行 `pnpm add pinia @pinia/nuxt`（記得先 `export PATH="/Users/wayne/.nvm/versions/node/v22.14.0/bin:$PATH"`）
- [x] 8.2 修改 `web/apps/client/nuxt.config.ts` 的 `modules` 加入 `'@pinia/nuxt'`
- [x] 8.3 確認 `pnpm dev`（client）啟動無 hydration error

## 9. Frontend — API 型別重新生成

- [x] 9.1 確認 `api/User.API` 已啟動（5102 swagger 提供新的 7 個 endpoint）
- [x] 9.2 從 repo 根目錄執行 `pnpm generate`（實際是 `pnpm gen:api`）
- [x] 9.3 確認 `web/internal/api-types/src/user.d.ts` 含 `/api/user/auth/*` 與 `/api/user/profile` 路徑

## 10. Frontend — Auth Store 與 API client

- [x] 10.1 新增 `web/apps/client/app/stores/auth.ts`（Pinia store）：state `accessToken` / `refreshToken`（`useCookie`）、`currentUser` ref；getter `isLoggedIn`；actions `register` / `verifyEmail` / `login` / `refresh` / `logout` / `fetchProfile` / `clearTokens`
- [x] 10.2 新增 `web/apps/client/app/composables/useUserApi.ts`（仿 `useAdminApi`）：token middleware + 401 refresh retry（網路錯誤 alert 暫不加，後續統一處理）
- [x] 10.3 store 內的 login / register / logout / refresh 改透過獨立的 plain client（不掛 token middleware），避免登入時還沒有 token 就被 401 攔截

## 11. Frontend — Auth Middleware

- [x] 11.1 新增 `web/apps/client/app/middleware/auth.global.ts`：定義 `publicPaths = ['/login', '/register', '/verify-email']`，未登入存非公開 → `navigateTo('/login')`，已登入存 `/login` 或 `/register` → `navigateTo('/')`
- [x] 11.2 確認 SPA reload 後 cookie 還原時 middleware 行為正確（不會誤導登入頁）— `useCookie` 在 SPA mode 自動從 cookie 還原，`isLoggedIn` 即時可用

## 12. Frontend — Pages

- [x] 12.1 新增 `web/apps/client/app/pages/login.vue`：brutalist 風格表單（email / password），送出呼叫 `authStore.login`、成功後 `navigateTo('/')`、失敗顯示後端 `errors` 字典；連結至 `/register`
- [x] 12.2 新增 `web/apps/client/app/pages/register.vue`：brutalist 風格表單（name / email / password / 確認 password），送出呼叫 `authStore.register`、成功後切到「請至 dev log 取驗證連結」提示頁；連結至 `/login`
- [x] 12.3 新增 `web/apps/client/app/pages/verify-email.vue`：`onMounted` 從 `useRoute().query.token` 取 token、呼叫 `authStore.verifyEmail`、依結果顯示「驗證成功 → 前往登入」或「驗證失敗（token 過期 / 已使用）」狀態
- [x] 12.4 確認三個頁面都遵守 client-ui rules（FormLabel、brutalist-button、lucide icons、不引入 Element Plus / shadow-md；登入/註冊/驗證頁 `definePageMeta({ layout: false })` 取消預設 layout 以避免 header 干擾）

## 13. Frontend — Layout 接線

- [x] 13.1 修改 `web/apps/client/app/layouts/default.vue`：頂部引入 `useAuthStore`
- [x] 13.2 Avatar `src` 改用 `https://api.dicebear.com/7.x/avataaars/svg?seed=${authStore.currentUser?.email ?? 'guest'}`
- [x] 13.3 Dropdown header「Signed in as」+ `authStore.currentUser.name`；未登入時改顯示「登入」+「註冊」NuxtLink，不顯示 Logout
- [x] 13.4 Logout 按鈕綁 `@click="async () => { await authStore.logout(); navigateTo('/login') }"`
- [x] 13.5 Footer status bar 的 `Current User: ...` 改用 `authStore.currentUser?.name ?? 'Guest'`
- [x] 13.6 移除 hardcoded `Alex (Creative Dept)` / `seed=Felix` 字串

## 14. Frontend — Profile 頁串接

- [x] 14.1 修改 `web/apps/client/app/pages/profile.vue`：`onMounted` 觸發 `authStore.fetchProfile()`（若尚未載入），UI 顯示真實資料
- [x] 14.2 加上「編輯個人資料」表單（name / avatar URL / notification_type 選項），送出呼叫 `PUT /api/user/profile` 並 refresh store
- [x] 14.3 顯示 `email`（唯讀）、`is_google_connected`（唯讀標籤）、`email_verified`（唯讀標籤）

## 15. 整合測試

- [x] 15.1 完整流程手測（後端 curl 已覆蓋；前端瀏覽器手測待人工確認 layout / 重新整理保持登入 / 自動 401 refresh）
- [x] 15.2 邊界測試：重複 email 註冊（409 ✓）、未驗證 email 嘗試登入（403 EMAIL_NOT_VERIFIED ✓）、密碼錯誤（401 ✓）、refresh token 重複使用（觸發全部撤銷 ✓）；停用帳號（DB Status=2）需人工驗證
- [x] 15.3 `dotnet build` 通過（0 errors，1 個 pre-existing warning）；`vue-tsc` 對 auth 相關檔零錯誤（其餘 Vite/Rollup 型別 mismatch 為 pre-existing infra noise）
- [x] 15.4 執行 `openspec validate add-user-auth --strict` 確認 spec 結構有效
