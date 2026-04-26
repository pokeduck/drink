# image-upload

## ADDED Requirements

### Requirement: 圖片格式白名單與真實格式驗證
系統 SHALL 僅接受副檔名為 `jpg / jpeg / png / gif / webp` 的上傳檔案，且 MUST 透過 magic bytes 偵測真實格式以防止偽造副檔名。

#### Scenario: 副檔名與內容皆為合法圖片
- **WHEN** 呼叫端 POST 一張副檔名 `.png` 且 magic bytes 為 PNG 的檔案
- **THEN** 系統接受該檔並進入處理流程

#### Scenario: 副檔名不在白名單
- **WHEN** 呼叫端 POST 一張副檔名 `.pdf` 的檔案
- **THEN** 系統 SHALL 回傳 HTTP 400 與錯誤碼 `INVALID_FILE_TYPE`

#### Scenario: 副檔名與 magic bytes 不一致
- **WHEN** 呼叫端 POST 一個副檔名 `.jpg` 但 magic bytes 為 PDF 的檔案
- **THEN** 系統 SHALL 回傳 HTTP 400 與錯誤碼 `INVALID_IMAGE`

### Requirement: 單檔大小上限 10MB
系統 MUST 在收到上傳請求時拒絕大於 10,485,760 bytes（10MB）的檔案。

#### Scenario: 上傳超過 10MB 的檔案
- **WHEN** 呼叫端 POST 一張 12MB 的 JPEG
- **THEN** 系統 SHALL 回傳 HTTP 400 與錯誤碼 `FILE_TOO_LARGE`

### Requirement: 影像處理流程使用 SkiaSharp
系統 SHALL 使用 SkiaSharp 進行影像解碼、縮圖與重編碼，且 MUST 在處理過程中清除所有 EXIF metadata。

#### Scenario: 上傳含 EXIF GPS 資料的 JPEG
- **WHEN** 呼叫端上傳一張 EXIF 內含 GPS 座標的 JPEG
- **THEN** 處理後的 WebP 檔案 SHALL NOT 包含任何 EXIF metadata

#### Scenario: 上傳動畫 GIF
- **WHEN** 呼叫端上傳一張多幀 GIF
- **THEN** 系統 SHALL 只保留第一幀並編碼為靜態 WebP

#### Scenario: 解碼失敗（檔案損毀）
- **WHEN** SkiaSharp 解碼回傳 null（檔案損毀或非影像）
- **THEN** 系統 SHALL 回傳 HTTP 400 與錯誤碼 `INVALID_IMAGE`

### Requirement: 長邊超過 4000px 等比縮圖
系統 MUST 在解碼後檢查影像尺寸，若 `max(width, height) > 4000` 則 SHALL 等比縮圖至長邊 4000px，短邊依比例計算（向下取整）。

#### Scenario: 上傳 6000x4000 的圖
- **WHEN** 呼叫端上傳 6000x4000 的圖
- **THEN** 處理後的 WebP 尺寸 SHALL 為 4000x2666（長邊縮到 4000，短邊按比例）

#### Scenario: 上傳 3000x2000 的圖
- **WHEN** 呼叫端上傳 3000x2000 的圖
- **THEN** 處理後的 WebP 尺寸 SHALL 維持 3000x2000（不放大、不縮小）

### Requirement: 統一輸出 WebP 格式 quality 85
系統 MUST 將所有處理後的影像編碼為 WebP，quality 固定為 85，且 SHALL NOT 提供 quality 參數讓呼叫端調整。

#### Scenario: 上傳 PNG 並查看回傳路徑
- **WHEN** 呼叫端上傳一張 `.png`
- **THEN** 回傳路徑的副檔名 SHALL 為 `.webp`

#### Scenario: 上傳 WebP 並查看回傳路徑
- **WHEN** 呼叫端上傳一張 `.webp`
- **THEN** 系統仍 SHALL 重新解碼並重編碼為 WebP（清除 EXIF）

### Requirement: Content-addressed 路徑與 SHA-256 去重
系統 MUST 對「處理後的 WebP bytes」計算 SHA-256，路徑採 `images/{hash[0:6]}/{hash}.webp` 格式。若該 hash 對應的檔案已存在，系統 SHALL NOT 重複寫入，直接回傳該 path 給呼叫端。

#### Scenario: 第一次上傳新內容
- **WHEN** 呼叫端上傳的處理結果 hash 為 `abc123...`
- **AND** `images/abc123/abc123...webp` 不存在
- **THEN** 系統 SHALL 寫入該檔案
- **AND** 回傳 path = `/assets/images/abc123/abc123...webp`

#### Scenario: 兩次上傳同一張圖（不同呼叫端、不同 EXIF）
- **WHEN** 第一次上傳產生 hash `xyz789...` 並落檔
- **AND** 第二次上傳的處理結果同樣為 `xyz789...`
- **THEN** 系統 SHALL NOT 重複寫檔
- **AND** 兩次的回傳 path 完全相同

### Requirement: 上傳 response 包含相對路徑與 metadata
系統 MUST 回傳上傳結果，內容包含相對路徑（`/assets/...` 開頭、不含 host）、SHA-256 hash、處理後 byte 大小、處理後寬度、處理後高度、MIME type（固定 `image/webp`）。

#### Scenario: 上傳成功
- **WHEN** 呼叫端 POST 一張合法圖片
- **THEN** Response body data SHALL 包含欄位 `path / hash / size / width / height / mime_type`
- **AND** `path` SHALL 以 `/assets/images/` 開頭
- **AND** `mime_type` SHALL 為 `image/webp`
- **AND** `path` SHALL NOT 包含 host

### Requirement: Static 服務回應 CDN-friendly headers
系統 MUST 在 `/assets/*` 的所有回應中加上下列 header：
- `Cache-Control: public, max-age=31536000, immutable`
- `ETag` 為 hash 字串包雙引號

#### Scenario: 讀取靜態圖片
- **WHEN** 用戶端 GET `/assets/images/abc/abc123...webp`
- **THEN** 回應 header SHALL 包含 `Cache-Control: public, max-age=31536000, immutable`
- **AND** 回應 header SHALL 包含 `ETag: "abc123..."`（雙引號內為完整 hash）

### Requirement: API Key 鑑權僅針對上傳，不影響靜態檔讀取
系統 MUST 對 `POST /api/upload/*` 強制 `X-Api-Key` 驗證，但 SHALL 對 `/assets/*` 跳過 API Key 檢查。

#### Scenario: 無 API Key 上傳
- **WHEN** 呼叫端 POST `/api/upload/files` 但未帶 `X-Api-Key`
- **THEN** 系統 SHALL 回傳 HTTP 401 與錯誤碼 `UNAUTHORIZED`

#### Scenario: 無 API Key 讀靜態檔
- **WHEN** 用戶端 GET `/assets/images/...` 不帶任何 header
- **THEN** 系統 SHALL 直接回傳檔案（並附 cache headers）

### Requirement: Asset host endpoint 由各 API 提供
系統 MUST 在 Admin.API 與 User.API 各提供一個 endpoint，回傳前端拼接圖片 URL 用的 base URL：
- `GET /api/admin/upload/asset-host`
- `GET /api/user/upload/asset-host`

值 SHALL 從各 API 自己 `appsettings.json` 的 `Upload.PublicBaseUrl` 讀取。Endpoint MUST 維持 `[Authorize]` 鑑權。

#### Scenario: 已認證使用者查詢 asset host
- **WHEN** 已認證的 admin 使用者 GET `/api/admin/upload/asset-host`
- **THEN** 系統 SHALL 回傳 `{ base_url: "<config 裡的 Upload.PublicBaseUrl 值>" }`

#### Scenario: 未認證查詢 asset host
- **WHEN** 未認證的呼叫端 GET `/api/admin/upload/asset-host`
- **THEN** 系統 SHALL 回傳 HTTP 401

### Requirement: 上傳代理流程（Admin.API / User.API → Upload.API）
Admin.API 與 User.API SHALL 各提供一個上傳 endpoint，內部以 `HttpClient` proxy 至 Upload.API，並自動帶上 `X-Api-Key` header。
- `POST /api/admin/upload`
- `POST /api/user/upload`

呼叫端 SHALL NOT 直接訪問 Upload.API 的上傳 endpoint。

#### Scenario: 前端透過 Admin.API 上傳
- **WHEN** 已認證 admin 透過 Admin.API POST 一張圖
- **THEN** Admin.API SHALL 將 multipart 內容 forward 至 Upload.API
- **AND** SHALL 在轉發請求中加入 `X-Api-Key` header
- **AND** SHALL 將 Upload.API 的 response 原樣回傳

#### Scenario: 未認證使用者透過 Admin.API 上傳
- **WHEN** 未認證呼叫端 POST `/api/admin/upload`
- **THEN** 系統 SHALL 回傳 HTTP 401（在 Admin.API 層擋下，不會打到 Upload.API）

### Requirement: 路徑固定 images/，移除 category 參數
系統 MUST NOT 接受 `category` query 參數。所有上傳一律寫入 `images/` 路徑底下。

#### Scenario: 呼叫端帶 category 參數
- **WHEN** 呼叫端 POST `/api/upload/files?category=avatars`
- **THEN** 系統 SHALL 忽略該參數
- **AND** 寫入路徑 SHALL 仍為 `images/{hash[0:6]}/{hash}.webp`

### Requirement: 新增錯誤碼
系統 SHALL 新增下列錯誤碼於 `ErrorCodes`：
- `INVALID_FILE_TYPE`：副檔名不在白名單
- `INVALID_IMAGE`：magic bytes 對不上、或 SkiaSharp 解碼失敗
- `FILE_TOO_LARGE`：超過 10MB

#### Scenario: 上傳 PDF
- **WHEN** 呼叫端上傳 `.pdf`
- **THEN** Response body SHALL 為 `{ code: <INVALID_FILE_TYPE 對應碼>, error: "INVALID_FILE_TYPE", ... }`

#### Scenario: 上傳超大檔
- **WHEN** 呼叫端上傳 15MB 的 JPEG
- **THEN** Response body SHALL 為 `{ code: <FILE_TOO_LARGE 對應碼>, error: "FILE_TOO_LARGE", ... }`

#### Scenario: 上傳損毀檔
- **WHEN** 呼叫端上傳一個假裝是 JPG 但 SkiaSharp 解碼失敗的檔
- **THEN** Response body SHALL 為 `{ code: <INVALID_IMAGE 對應碼>, error: "INVALID_IMAGE", ... }`
