## 1. Backend — FileUploadService 重構為可注入 maxDimension

- [x] 1.1 修改 `api/Application/Services/FileUploadService.cs`：把 `MaxDimension` 從 const 改為方法參數；`ResizeIfNeeded` 接收 `maxDimension`；`Upload()` 增加 `int maxDimension = 4000` 參數
- [x] 1.2 確認既有呼叫端（透過 `/api/upload` 進來的）行為不變（傳 4000 與舊行為一致）

## 2. Backend — Avatar 上傳 endpoint

- [x] 2.1 修改 `api/Upload.API/Controllers/FilesController.cs`：新增 `[HttpPost("avatar")]` action，呼叫 `_uploadService.Upload(file, ct, maxDimension: 512)`
- [x] 2.2 修改 `api/User.API/Controllers/UploadController.cs`：新增 `[HttpPost("avatar")]` proxy action（需 `[Authorize]`），轉發到 Upload.API 的 `/api/upload/avatar`
- [x] 2.3 確認 `[ProducesResponseType]` 標註與既有 `/api/upload` 一致

## 3. Backend — UserProfileResponse 加 created_at

- [x] 3.1 修改 `api/Application/Responses/User/UserProfileResponse.cs`：新增 `DateTime CreatedAt { get; set; }`
- [x] 3.2 確認 `UserProfileMapper.ToUserProfileResponse` 自動映射（Mapperly required-target 模式會自動拉 `User.CreatedAt`）

## 4. Backend — 變更密碼 endpoint

- [x] 4.1 修改 `api/Application/Requests/User/UserAuthRequests.cs`：新增 `ChangeUserPasswordRequest`（`OldPassword` + `NewPassword`，`[Required]` + `[MinLength(6)]`）
- [x] 4.2 修改 `api/Application/Services/UserAuthService.cs`：新增 `ChangePassword(ChangeUserPasswordRequest)` 方法 — 驗 old → 雜湊 new → 寫 DB（**不**撤銷 token，user 端與 admin 端不同）；errors 字典 `old_password` 對應 `INVALID_PASSWORD`
- [x] 4.3 修改 `api/User.API/Controllers/AuthController.cs`：新增 `[HttpPut("password")]` action（需 `[Authorize]`），帶完整 `[ProducesResponseType]`（200 / 400 / 401）

## 5. Backend — 登出所有裝置 endpoint

- [x] 5.1 修改 `api/Application/Services/UserAuthService.cs`：新增 `LogoutAll()` 方法 — 直接呼叫私有 helper `RevokeAllTokens(CurrentUserId)`，回 `Success()`
- [x] 5.2 修改 `api/User.API/Controllers/AuthController.cs`：新增 `[HttpPost("logout-all")]` action（需 `[Authorize]`）

## 6. Backend — Build & 手測

- [x] 6.1 `dotnet build /Users/wayne/Desktop/webproj/drink/api/Drink.sln` 通過（0 errors）
- [x] 6.2 啟動 User.API + Upload.API，確認 Swagger 顯示新 endpoints：`PUT /api/user/auth/password`、`POST /api/user/auth/logout-all`、`POST /api/user/upload/avatar`、`POST /api/upload/avatar`、`GET /api/user/profile` response 含 `created_at`
- [x] 6.3 手測：登入 → PUT password 用錯舊密碼回 400 errors / 對舊密碼回 200 → 確認 refresh token **未被** 撤銷（舊 token refresh 仍可用）→ logout-all 行為冪等（呼叫兩次都 200）→ POST avatar 上傳一張 1024x1024 圖，確認 response 路徑指向 512x512 webp

## 7. Codegen

- [x] 7.1 啟動三支 API（5101 / 5102 / 5103），repo 根目錄執行 `pnpm gen:api`
- [x] 7.2 確認 `web/internal/api-types/src/user.d.ts` 含 `/api/user/auth/password` / `/api/user/auth/logout-all` / `/api/user/upload/avatar`
- [x] 7.3 確認 `web/internal/api-types/src/upload.d.ts` 含 `/api/upload/avatar`
- [x] 7.4 確認 `UserProfileResponse` 的 schema 含 `created_at`

## 8. Frontend — Auth store 擴充

- [x] 8.1 修改 `web/apps/client/app/stores/auth.ts`：新增 `changePassword({ oldPassword, newPassword })` action — 走 plain authClient（不帶 token middleware 不行，這個需要 token，要用 user-api）→ 改用 `useUserApi()`；變更成功後不清 token（保持登入），但下一次 401 時 refresh 會失敗，因為後端撤銷了所有 token；UX 上應主動 `clearTokens()` + 導 `/login`
- [x] 8.2 新增 `logoutAll()` action — 呼叫 `POST /api/user/auth/logout-all`，成功後 `clearTokens()` + 導 `/login`

## 9. Frontend — Avatar 上傳 composable

- [x] 9.1 新增 `web/apps/client/app/composables/useAvatarUpload.ts`：包裝「拖拉 / 點選 → POST /api/user/upload/avatar → 拿到 url」流程；提供 `uploadAvatar(file)`、`previewUrl` ref、`uploading` ref、`error` ref
- [x] 9.2 不直接 reuse `useImageUploadQueue`（那個是多檔佇列；avatar 是單檔即時），但可參考其上傳邏輯

## 10. Frontend — `/settings` 頁

- [x] 10.1 新建 `web/apps/client/app/pages/settings.vue`，遵守 client-ui rules（`<BackButton />`、`<FormLabel>`、`brutalist-button` / `brutalist-button-primary`、不引入 Element Plus）
- [x] 10.2 三個 section 切版（Profile / Account / Preferences）
- [x] 10.3 Profile section：顯示名稱輸入 + 頭像 dropzone（拖拉 / 點選），預覽區 + 「儲存」/「取消」按鈕；儲存呼叫 `PUT /api/user/profile`
- [x] 10.4 Account section：Email read-only / 變更密碼 inline 表單（舊 / 新 / 確認新）/ 登出所有裝置按鈕 / Google 連結 disabled 按鈕標「即將推出」
- [x] 10.5 Preferences section：主題切換（Light / Dark / System，綁 `useColorMode().preference`）/ 通知 channel 兩個 toggle（綁 `currentUser.notification_type`，PUT /profile）
- [x] 10.6 失敗顯示後端 `errors` 字典；變更密碼成功 → 清空密碼欄位 + toast 「密碼已變更」，不主動 logout / redirect（使用者自行按「登出所有裝置」才會踢人）

## 11. Frontend — `/favorites` 頁

- [x] 11.1 新建 `web/apps/client/app/pages/favorites.vue`
- [x] 11.2 Mobile：tab 切換（店家 / 飲料），用 `md:hidden` 控制
- [x] 11.3 Desktop：兩個 section 並列（`md:grid md:grid-cols-2`），用 `hidden md:grid` 控制
- [x] 11.4 列表項目寫 mockup 假資料；移除按鈕點擊顯示 toast「Mockup 階段尚未支援」（toast 元件可暫用 `alert()` 或自寫一個 transient `<Transition>` 框）
- [x] 11.5 頁面底部加 mockup 標示「真實功能將於店家瀏覽功能上線後啟用」

## 12. Frontend — `/profile` 重構（read-only）

- [x] 12.1 修改 `web/apps/client/app/pages/profile.vue`：移除所有編輯狀態（`editing`、`form` reactive、`handleSave`、`startEdit`、`cancelEdit`、edit form section）
- [x] 12.2 Identity Header 加「Joined Apr 2026」（從 `currentUser.created_at` 用 `Intl.DateTimeFormat` 格式化）
- [x] 12.3 移除 4 個假 menu icon 卡片（Wallet / Notifications / Security / Settings）
- [x] 12.4 Stats 區塊改為 4 張 mockup 卡：本月花費 / 累計揪團 / Top Shop / Last Drink；卡片內加「Mockup data — coming soon」標籤
- [x] 12.5 新增 My Favorites 區塊：兩列橫向捲動（Shops 5 / Drinks 3 mockup），標題右側「編輯」按鈕 → `navigateTo('/favorites')`
- [x] 12.6 ⚙ 齒輪 icon 點擊 → `navigateTo('/settings')`（不再 inline edit）
- [x] 12.7 移除 Logout 按鈕（已移到 dropdown）

## 13. Frontend — Avatar dropdown 重構

- [x] 13.1 修改 `web/apps/client/app/layouts/default.vue`：dropdown 已登入版本改為「username header / 偏好設定 / 我的收藏 / Logout」
- [x] 13.2 移除原本的 Profile / My Orders 連結與 Settings 假連結
- [x] 13.3 「偏好設定」→ `navigateTo('/settings')`、「我的收藏」→ `navigateTo('/favorites')`，點擊後 dropdown 關閉
- [x] 13.4 username 為 display only（不可點），style 用 `cursor: default`
- [x] 13.5 未登入版本維持「登入 / 註冊」（不變動）

## 14. Frontend — Type check & SPA 測試

- [x] 14.1 `cd web/apps/client && pnpm exec nuxt prepare` 成功
- [x] 14.2 `pnpm exec vue-tsc --noEmit -p .nuxt/tsconfig.json` 對新增 / 修改檔案無錯誤（Vite/Rollup pre-existing infra noise 可忽略）
- [x] 14.3 SPA dev 啟動，瀏覽器手測：
   - `/profile`：read-only，齒輪 → /settings、編輯 → /favorites、無 4 個假 menu icon
   - `/settings`：拖拉頭像→預覽→儲存生效；變更密碼→成功後被踢出登入；登出所有裝置→被踢出登入
   - `/favorites`：mobile tab、desktop two-section、移除按鈕 toast
   - Avatar dropdown：username header / 偏好設定 / 我的收藏 / 登出，無 Profile / My Orders 連結
   - Email read-only

## 15. Spec 驗證

- [x] 15.1 執行 `openspec validate add-user-settings --strict` 確認結構有效
- [x] 15.2 執行 `openspec validate image-upload --strict`、`user-auth --strict`、`user-profile --strict`、`client-layout --strict`、`client-pages --strict` 確認 main spec sync 後仍有效（archive 階段執行）
