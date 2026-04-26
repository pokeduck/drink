# Tasks: refactor-image-upload-pipeline

## 1. 套件與設定

- [x] 1.1 在 `Drink.Application` (或 `Drink.Infrastructure`) csproj 加入 NuGet 套件 `SkiaSharp` 與 `SkiaSharp.NativeAssets.Linux`（部署用）
- [x] 1.2 `appsettings.json` (Upload.API) 移除 `Upload.AllowedExtensions` 中的 `.pdf`，新增 `Upload.PublicBaseUrl` 鍵（dev: `http://localhost:5103/assets`）
- [x] 1.3 `appsettings.json` (Admin.API) 在 `UploadApi` 區塊新增 `PublicBaseUrl=http://localhost:5103/assets`
- [x] 1.4 `appsettings.json` (User.API) 在 `UploadApi` 區塊新增 `PublicBaseUrl=http://localhost:5103/assets`
- [x] 1.5 `Drink.Application/Settings/UploadApiSettings.cs` 新增 `PublicBaseUrl` 屬性

## 2. Upload.API 核心 Pipeline

- [x] 2.1 重寫 `Drink.Application/Services/FileUploadService.cs`：
  - [x] 2.1.1 副檔名白名單驗證（jpg/jpeg/png/gif/webp）
  - [x] 2.1.2 Magic bytes 偵測真實格式（讀前 12 bytes 比對 PNG/JPEG/GIF/WebP signature）
  - [x] 2.1.3 大小驗證（≤ 10MB）
  - [x] 2.1.4 SkiaSharp 解碼（`SKBitmap.Decode`），失敗回 `INVALID_IMAGE`
  - [x] 2.1.5 動畫 GIF 取第一幀（`SKCodec` 走第 0 幀）
  - [x] 2.1.6 長邊 > 4000 → 等比縮圖（`SKBitmap.Resize`）
  - [x] 2.1.7 編碼為 WebP quality 85（`SKImage.Encode(SKEncodedImageFormat.Webp, 85)`）
  - [x] 2.1.8 SHA-256(處理後 bytes) 取得完整 hash
  - [x] 2.1.9 計算路徑 `images/{hash[0:6]}/{hash}.webp`
  - [x] 2.1.10 若檔案已存在 → 不寫檔，直接回傳 path
  - [x] 2.1.11 透過 `IFileStorageService` 寫入新檔
  - [x] 2.1.12 回傳 `FileUploadResponse { path, hash, size, width, height, mime_type }`
- [x] 2.2 修改 `FileUploadResponse.cs`：移除原 `url`、新增 `path / hash / size / width / height / mime_type`（`mime_type` 固定 `image/webp`）
- [x] 2.3 `FilesController.cs`：移除 `category` query 參數
- [x] 2.4 新增錯誤碼於 `ErrorCodes.cs`：`INVALID_FILE_TYPE` / `INVALID_IMAGE` / `FILE_TOO_LARGE`（依錯誤碼編碼規則 `4XXYY`，沿用 module 00 通用區段或開新模組編號）
- [x] 2.5 `LocalFileStorage.cs`（如已存在則修改、否則新增）：
  - [x] 2.5.1 寫檔時自動建立父目錄
  - [x] 2.5.2 提供 `Exists(path)` 方法供去重判斷
  - [x] 2.5.3 `WriteAsync` 收到已存在的 path 應 idempotent（不報錯也不重寫）

## 3. Upload.API Static File 設定

- [x] 3.1 `Program.cs` 修改 `UseAssetFileServer`：在 `StaticFileOptions.OnPrepareResponse` 加上：
  - [x] 3.1.1 `Cache-Control: public, max-age=31536000, immutable`
  - [x] 3.1.2 `ETag: "{filename without ext}"`（filename 即 hash）
- [x] 3.2 確認 CORS 設定維持（Allow `localhost:8081`、`localhost:8082` + 從 config 讀額外 origin）
- [x] 3.3 確認 `ApiKeyMiddleware` 仍跳過 `/assets`

## 4. Upload.API 清理舊資料

- [x] 4.1 清空 `api/Upload.API/Uploads/` 目錄（保留 `images/` 空目錄）
- [x] 4.2 確認 `.gitignore` 仍排除 `Uploads/` 內容物

## 5. Admin.API 與 User.API 整合

- [x] 5.1 `Admin.API/Controllers/UploadController.cs`：
  - [x] 5.1.1 `POST /api/admin/upload`：移除 `category` 參數，proxy 至 Upload.API
  - [x] 5.1.2 `GET /api/admin/upload/asset-host`：從 `UploadApiSettings.PublicBaseUrl` 讀，回 `{ base_url }`
- [x] 5.2 `User.API/Controllers/UploadController.cs`：同上（admin → user）

## 6. API 型別產出

- [x] 6.1 啟動 Admin.API (5101) / User.API (5102) / Upload.API (5103)
- [x] 6.2 從根目錄執行 `pnpm generate`
- [x] 6.3 確認 `web/internal/api-types/src/{admin,user,upload}.d.ts` 含新欄位（`path / hash / width / height`）與新 endpoint（`asset-host`）— 經 swagger.json 確認 FileUploadResponse 含新欄位、`/api/upload/files` 已無 `category` 參數

## 7. Admin Frontend Composable

- [x] 7.1 新增 `web/apps/admin/app/composable/useImageUploadQueue.ts`：
  - [x] 7.1.1 接受 `File[]`，回傳 `uploads: Ref<UploadItem[]>`、`progress`、`enqueue`、`retry`
  - [x] 7.1.2 內部用 Promise queue 限制 `concurrency = 1`
  - [x] 7.1.3 每張用 XHR + multipart 呼叫 `POST /api/admin/upload`，附上 token 與 progress 追蹤
  - [x] 7.1.4 失敗的 item 標記 `error`，提供 retry 方法
- [x] 7.2 新增 `web/apps/admin/app/composable/useAssetHost.ts`：
  - [x] 7.2.1 第一次呼叫時打 `GET /api/admin/upload/asset-host` 一次
  - [x] 7.2.2 cache 在 module-level 變數
  - [x] 7.2.3 提供 `assetUrl(path: string): string` helper

## 8. Client Frontend Composable

- [x] 8.1 新增 `web/apps/client/app/composables/useImageUploadQueue.ts`（同 7.1，呼叫 `/api/user/upload`）
- [x] 8.2 新增 `web/apps/client/app/composables/useAssetHost.ts`（同 7.2，呼叫 `/api/user/upload/asset-host`）

## 9. 驗收

- [x] 9.1 上傳一張 5MB JPEG with EXIF GPS → 確認回傳 path 為 `.webp`、實際檔案大小變小、用 EXIF reader 確認無 GPS metadata（518KB→205KB；grep "Exif"/"GPS" 在輸出 webp 都找不到）
- [x] 9.2 上傳同一張圖兩次 → 兩次回傳 path 完全相同、磁碟只有一份檔案
- [x] 9.3 上傳 12MB JPEG → 回 400 + `FILE_TOO_LARGE`（用 19MB 檔測試）
- [x] 9.4 上傳 `.pdf` → 回 400 + `INVALID_FILE_TYPE`
- [x] 9.5 上傳改成 `.jpg` 副檔名的 PDF → 回 400 + `INVALID_IMAGE`
- [x] 9.6 上傳動畫 GIF → 確認結果為靜態（只有第一幀；輸出 webp 為 VP8 編碼，無 ANMF chunk）
- [x] 9.7 上傳 6000x4000 JPEG → 確認結果為 4000x2666 WebP（用 7728x5152 測試 → 4000x2666）
- [x] 9.8 GET `/assets/images/...` → 確認 response header 含 `Cache-Control: ... immutable` 與 `ETag: "..."`
- [x] 9.9 GET `/api/admin/upload/asset-host` 帶 token → 回 `{ base_url: "http://localhost:5103/assets" }`（admin 端驗證；user 端邏輯相同未測，無測試帳號）
- [x] 9.10 GET `/api/admin/upload/asset-host` 不帶 token → 401（admin 與 user 兩邊都 401）
- [ ] 9.11 前端佇列上傳 5 張 → 一張一張依序傳完，總進度條從 0/5 → 5/5（**需瀏覽器手動驗收**）

## 10. Lint / Build / Validate

- [x] 10.1 `dotnet build` 全綠（5 個既有警告，全與本 change 無關）
- [x] 10.2 Admin Nuxt build 全綠（lint script 不存在，build 已驗證 type）
- [x] 10.3 Client Nuxt build 全綠（lint script 不存在，build 已驗證 type）
- [x] 10.4 `openspec validate refactor-image-upload-pipeline --strict` 通過

## 11. Phase 2 文件（不實作）

- [x] 11.1 確認 design.md 內 Phase 2 章節已包含 CF Free 設定步驟、Linode 防火牆鎖 IP、切換時 config 修改清單
