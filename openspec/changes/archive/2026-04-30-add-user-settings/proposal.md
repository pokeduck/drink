## Why

完成會員認證後，client 進入「真實使用者體驗」階段，但目前 `/profile` 頁的內容（4 個假 menu icon、hardcode 12 / $350 stats）只是 brutalist 風格示範，沒有實質功能；而 avatar dropdown 也存在重複入口（Profile / My Orders / Settings 三個都指向 `/profile`）。同時，會員缺少最基本的「變更密碼」「登出所有裝置」自助管理操作；頭像目前只能透過 PUT `/profile` 直接覆寫 URL，沒有適當的上傳體驗。本次將：(a) 釐清 profile / settings / favorites 三個區域的職責邊界、(b) 建立完整的偏好設定頁、(c) 預留收藏功能的版面（mockup），讓未來真實 favorites entity 上線時 UI 已經就位。

## What Changes

### 入口架構清理

- `/profile` 改為 read-only 展示頁（identity + stats mockup + favorites mockup），所有編輯動作搬到 `/settings`
- `/profile` 上的齒輪 icon → `navigateTo('/settings')`
- Header avatar dropdown 重構：從目前的「Profile / My Orders / Settings / Logout」改為「username header / 偏好設定 / 我的收藏 / 登出」
- 解決現有「4 個入口指向同一頁」的重複問題

### 新頁面

- 新增 `/settings`：唯一的可編輯介面（Profile editor / Account / Preferences）
- 新增 `/favorites`：mockup 階段的我的收藏清單（Mobile tab / Desktop two-section）

### 後端新增 endpoints

- `PUT /api/user/auth/password` — 變更密碼；與 admin 端不同，**不會主動撤銷任何 refresh token**（避免「在自己機器上換密碼還被踢出」的常見痛點），需要踢出其他裝置請另行使用 logout-all
- `POST /api/user/auth/logout-all` — 登出所有裝置，撤銷該 user 所有未撤銷的 refresh token
- `POST /api/upload/avatar`（Upload.API）— avatar 專用上傳，max 長邊 512px，沿用既有驗證 / EXIF 清除 / WebP / SHA-256 dedup
- `POST /api/user/upload/avatar`（User.API proxy）— 對應前端的呼叫窗口
- `UserProfileResponse` 補欄位 `created_at`（讓 profile 顯示「Joined Apr 2026」）

### 頭像上傳 UX

- 設定頁採「拖拉 / 點擊選擇檔案」單一模式（不做外部 URL 模式，避免外部圖失效 / 不經壓縮的問題）
- 拖拉後**先預覽，使用者按「儲存」才真正寫入** `User.Avatar`（避免錯放後又要重來）
- 舊頭像檔案保留不刪（行為已符合：`User.Avatar` 只存 URL，覆寫不影響 Upload.API 的檔案；SHA-256 dedup 自動處理重覆）

### 明確 Out of Scope

- 真實 favorites entity / API / 在店家詳情頁加 ⭐ 的 UI 入口（依賴未實作的「店家瀏覽」capability，留 future change）
- 真實 stats 數據（依賴未實作的「揪團 / 訂單」capability，留 future change）
- Google SSO 真實接通（獨立 change）
- Email 變更、刪除帳號、飲料預設、avatar 歷史清單

## Capabilities

### New Capabilities

無。本次所有變更均屬既有 capability 的擴充。

### Modified Capabilities

- `image-upload`: 新增 avatar-specific 上傳 endpoint 與更小的 max 長邊（512px）
- `user-auth`: 新增變更密碼、登出所有裝置兩個自助操作
- `user-profile`: `UserProfileResponse` 加 `created_at` 欄位
- `client-layout`: avatar dropdown 結構變更（移除 Profile / My Orders 連結，新增「我的收藏」）
- `client-pages`: Profile 頁重構為 read-only 展示；新增 Settings 頁（唯一可編輯入口）；新增 Favorites mockup 頁

## Impact

### Affected code

- `api/Application/Services/`：`UserAuthService` 加 `ChangePassword` / `LogoutAll`；`FileUploadService` 重構 `Upload()` 接收 `maxDimension` 參數
- `api/Application/Requests/User/`：`ChangePasswordRequest`
- `api/Application/Mappings/UserProfileMapper.cs`：補 `CreatedAt → created_at` 映射
- `api/Application/Responses/User/UserProfileResponse.cs`：加 `CreatedAt` 屬性
- `api/User.API/Controllers/AuthController.cs`：加 `change-password` / `logout-all` endpoints
- `api/User.API/Controllers/UploadController.cs`：加 `/avatar` proxy endpoint
- `api/Upload.API/Controllers/FilesController.cs`：加 `/avatar` endpoint
- `web/internal/api-types/src/user.d.ts`、`upload.d.ts`：codegen 重生
- `web/apps/client/app/`：
  - `layouts/default.vue`：dropdown 重構
  - `pages/profile.vue`：重寫成 read-only
  - `pages/settings.vue`：新建
  - `pages/favorites.vue`：新建
  - `stores/auth.ts`：補 `changePassword` / `logoutAll` actions
  - `composables/`：avatar 上傳專用 helper（沿用 useImageUploadQueue 邏輯）

### Dependencies

- 後端：不新增 NuGet（SkiaSharp 已存在）
- 前端：不新增 npm 套件

### Database

- **不需要 migration**：所有變更都是 endpoint / response 層

### Configuration

- 不需新增 appsettings key
