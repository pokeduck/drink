# Proposal: 重構 Upload.API 為 Image Pipeline

## Why

目前 Upload.API 只做檔案落地與靜態服務，沒有：去重、EXIF 清除、格式統一、尺寸正規化、CDN-friendly cache header。隨著 shop 圖片功能即將上線（同一張圖可能被多店家/多品項引用），現行設計會產生重複檔案、暴露 EXIF 隱私（GPS / 拍攝裝置）、且無法享受 CDN 邊緣快取。

此外，未來會把資產放到 Linode + Cloudflare Free Plan 提供 CDN，需要把 asset host 抽成設定 + 路徑使用 content-addressed 結構（hash 為主），讓 CDN cache 可以設 `immutable` 永不失效。

## What Changes

### Phase 1（本次實作）

- **Image pipeline 改寫** Upload.API：
  - 收檔 → magic bytes 偵測格式 → SkiaSharp 解碼（GIF 取第一幀） → 長邊 > 4000 等比縮到 4000 → 重編碼為 WebP（quality=85，順帶清除 EXIF） → SHA-256(處理後 bytes) → 寫入 `images/{hash[0:6]}/{hash}.webp` → 已存在則去重，回傳同 path
  - 接受副檔名：`jpg / jpeg / png / gif / webp`，magic bytes 必須對得上
  - 單檔上限 10MB（處理前）
- **路徑改 content-addressed**：`/assets/images/{hash[0:6]}/{hash}.webp`，移除 `category` query 參數（路徑固定 `images/`）
- **CDN-friendly headers**：`/assets/*` 回應加上 `Cache-Control: public, max-age=31536000, immutable` 與 `ETag: "{hash}"`
- **新增 asset-host endpoint**：
  - `GET /api/admin/upload/asset-host`
  - `GET /api/user/upload/asset-host`
  - 各自從自己的 `appsettings.json` 讀 `Upload.PublicBaseUrl`（dev: `http://localhost:5103/assets`，prod: `https://cdn.example.com/assets`）
- **DB 路徑規範**：上傳 response 與 DB 儲存值統一相對路徑（不含 host），由前端用 asset-host 拼接
- **前端佇列上傳**（不切片、不 resumable）：批次上傳時，前端 composable `useImageUploadQueue` 對 `File[]` 一張一張呼叫單檔 POST，提供總進度、單張狀態、retry。Server 端永遠只處理單檔
- **BREAKING — 移除 `category` query 參數**：所有檔案落在 `images/`，呼叫端不再帶 `category`
- **BREAKING — 移除 PDF 支援**：白名單只保留圖片格式
- **BREAKING — Response shape 變動**：`FileUploadResponse` 加入 `hash / width / height`，`url` 欄位改為相對路徑（不含 host）

### Phase 2（不在本 change 實作，僅文件記載於 design.md）

- Cloudflare Free Plan 接上 Linode 的步驟、DNS、Cache Rule、SSL 模式、Linode 防火牆鎖 CF IP 的操作清單
- 切換時的 config 修改清單
- `IFileStorageService` 未來換 S3 / R2 / MinIO 的接縫位置

## Capabilities

### New Capabilities

- `image-upload`: Upload.API 的圖片處理流程（白名單、magic bytes、SkiaSharp pipeline、WebP 統一、SHA-256 去重、content-addressed 路徑、CDN-friendly headers、asset-host endpoint）

### Modified Capabilities

無（既有 capability 都不變更需求）。

## Impact

**Affected code:**

- `api/Upload.API/Program.cs`（pipeline 註冊、static file headers）
- `api/Upload.API/Controllers/FilesController.cs`（移除 `category`、單檔處理流程）
- `api/Upload.API/Middleware/ApiKeyMiddleware.cs`（不變動）
- `api/Application/Services/FileUploadService.cs`（重寫：magic bytes + SkiaSharp + hash + 去重）
- `api/Application/Responses/FileUploadResponse.cs`（新增 `hash / width / height`、`url` 改相對路徑）
- `api/Application/Settings/UploadApiSettings.cs`（新增 `PublicBaseUrl`）
- `api/Admin.API/Controllers/UploadController.cs`（移除 `category`、新增 `asset-host`）
- `api/User.API/Controllers/UploadController.cs`（同上）
- `api/Admin.API/appsettings*.json` / `api/User.API/appsettings*.json` / `api/Upload.API/appsettings*.json`（`Upload.PublicBaseUrl`）
- `web/internal/api-types/src/{admin,user,upload}.d.ts`（`pnpm generate` 後重新產生）

**Affected dependencies:**

- 新增 NuGet：`SkiaSharp` + `SkiaSharp.NativeAssets.Linux`（部署用）

**Affected data:**

- `api/Upload.API/Uploads/`：清空舊測試檔案（1 張 jpg、1 個 pdf；目前無前端引用）
- 既有路徑結構 `Uploads/{guid}.{ext}` 改為 `Uploads/images/{hash[0:6]}/{hash}.webp`

**Permissions:**

- 不新增 AdminMenu，沿用既有 Admin/User 認證
- `asset-host` endpoint 維持 `[Authorize]`（前端拿 token 後才能查）

**Frontend:**

- 新增 composable `web/apps/admin/app/composables/useImageUploadQueue.ts`
- 新增 composable `web/apps/client/app/composables/useImageUploadQueue.ts`
- Admin/Client 啟動時拉一次 `asset-host`，存於 Pinia store（或 useState），渲染圖片時用 `${baseUrl}${path}` 拼接

**Out of scope:**

- 多尺寸（thumb / medium）— 上 Cloudflare Image Resizing 後在 URL 動態處理
- 防盜連、配額、rate limit
- 排程清理孤兒檔案（DB 沒參照的檔）— 未來另開 change
- 舊檔案搬遷（直接清空，目前無人引用）
