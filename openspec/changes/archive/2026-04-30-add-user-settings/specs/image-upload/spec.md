## ADDED Requirements

### Requirement: Avatar 專用上傳 endpoint

Upload.API SHALL 提供 `POST /api/upload/avatar` endpoint（需 X-Api-Key），沿用 `/api/upload` 既有的格式驗證、magic bytes 偵測、單檔 10MB 上限、SkiaSharp 處理（去 EXIF）、WebP quality 85、SHA-256 dedup 與相對路徑回傳行為，唯一差別為**長邊上限改為 512px**（適配前端最大 256px @2x retina 的顯示需求，與 admin shop image 走的 4000px pipeline 區隔）。User.API MUST 提供對應的 `POST /api/user/upload/avatar` proxy（需 `[Authorize]`）。

#### Scenario: 上傳一張 1024x1024 的頭像

- **WHEN** 已認證使用者透過 `/api/user/upload/avatar` 上傳一張 1024x1024 的 PNG
- **THEN** Upload.API 經 SkiaSharp 解碼、清除 EXIF、resize 至 512x512，編碼為 WebP quality 85，計算 SHA-256 寫入 `images/{xx}/{xxx...}.webp`，回傳含相對路徑與 metadata 的 `FileUploadResponse`

#### Scenario: 上傳一張 256x256 的頭像

- **WHEN** 已認證使用者上傳一張 256x256 的 JPG
- **THEN** Upload.API 不放大尺寸，直接以原 256x256 編碼為 WebP（不再 upscale）

#### Scenario: 拒絕的檔案

- **WHEN** 使用者上傳超過 10MB / 副檔名不符 / magic bytes 不符的檔案
- **THEN** 系統 SHALL 回傳對應錯誤碼（`FILE_TOO_LARGE` / `INVALID_FILE_TYPE` / `INVALID_IMAGE`），與 `/api/upload` 行為一致

#### Scenario: User.API proxy

- **WHEN** 已認證 client 呼叫 `POST /api/user/upload/avatar`
- **THEN** User.API MUST 透過 HttpClient 帶 `X-Api-Key` 將請求轉發到 Upload.API 的 `/api/upload/avatar`，並原樣回傳 response

## MODIFIED Requirements

### Requirement: 長邊超過 4000px 等比縮圖

`FileUploadService` 的 resize 邏輯 SHALL 接受 `int maxDimension` 參數（不再 hardcode），由各 endpoint 自行決定上限：`/api/upload` 傳 `4000`、`/api/upload/avatar` 傳 `512`。系統 MUST 在解碼後檢查影像尺寸，若 `max(width, height) > maxDimension` 則 SHALL 等比縮圖至長邊 `maxDimension`，短邊依比例計算（向下取整）；若已小於等於 `maxDimension` 則維持原尺寸。

#### Scenario: 上傳 6000x4000 的圖到 /api/upload

- **WHEN** 呼叫端透過 `/api/upload` 上傳 6000x4000 的圖
- **THEN** 處理後的 WebP 尺寸 SHALL 為 4000x2666（長邊縮到 4000，短邊按比例）

#### Scenario: 上傳 1024x1024 的圖到 /api/upload/avatar

- **WHEN** 呼叫端透過 `/api/upload/avatar` 上傳 1024x1024 的圖
- **THEN** 處理後的 WebP 尺寸 SHALL 為 512x512

#### Scenario: 上傳 3000x2000 的圖到 /api/upload

- **WHEN** 呼叫端透過 `/api/upload` 上傳 3000x2000 的圖
- **THEN** 處理後的 WebP 尺寸 SHALL 維持 3000x2000（不放大、不縮小）
