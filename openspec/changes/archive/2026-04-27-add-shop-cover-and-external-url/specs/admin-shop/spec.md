## ADDED Requirements

### Requirement: 店家 Cover 圖片與外部連結欄位
Shop Entity SHALL 包含 `CoverImagePath`（string?, max 500, nullable）與 `ExternalUrl`（string?, max 500, nullable）兩個欄位。`CoverImagePath` 儲存 image-upload pipeline 回傳的相對路徑；`ExternalUrl` 儲存店家的外部連結（例如 Google Map URL），僅接受 http 或 https scheme 的合法 URL。

#### Scenario: 新店家預設無 cover 與 external url
- **WHEN** 管理員透過 `POST /api/admin/shops` 建立店家但未提供 `cover_image_path` 與 `external_url`
- **THEN** 系統建立店家，CoverImagePath 與 ExternalUrl 皆為 null

#### Scenario: 取得單一店家回傳新欄位
- **WHEN** 管理員請求 `GET /api/admin/shops/{shopId}`
- **THEN** Response 包含 `cover_image_path` 與 `external_url`，未設定時為 null

#### Scenario: 列表 API 不回傳 cover 與 external url
- **WHEN** 管理員請求 `GET /api/admin/shops`
- **THEN** ShopListResponse 不包含 `cover_image_path` 與 `external_url` 欄位

### Requirement: External URL 格式驗證
系統 SHALL 對 `external_url` 進行格式驗證：值必須能被 `Uri.TryCreate(value, UriKind.Absolute, ...)` 解析為絕對 URI，且 `Scheme` 必須為 `http` 或 `https`。當值為 null 或空字串時 SHALL 視為未設定，不觸發驗證錯誤。驗證失敗時系統 SHALL 回傳 400 VALIDATION_ERROR，errors 包含 `{ "external_url": ["請輸入有效的連結"] }`。

#### Scenario: 接受 https URL
- **WHEN** 管理員送出 `external_url=https://maps.google.com/?q=...`
- **THEN** 系統接受並儲存

#### Scenario: 接受 http URL
- **WHEN** 管理員送出 `external_url=http://example.com`
- **THEN** 系統接受並儲存

#### Scenario: 拒絕非 http(s) scheme
- **WHEN** 管理員送出 `external_url=ftp://example.com` 或 `external_url=javascript:alert(1)` 或 `external_url=mailto:foo@bar.com`
- **THEN** 系統回傳 400 VALIDATION_ERROR，errors 為 `{ "external_url": ["請輸入有效的連結"] }`

#### Scenario: 拒絕非 URL 字串
- **WHEN** 管理員送出 `external_url=not a url`
- **THEN** 系統回傳 400 VALIDATION_ERROR，errors 為 `{ "external_url": ["請輸入有效的連結"] }`

#### Scenario: 接受 null 或空字串
- **WHEN** 管理員送出 `external_url=null` 或 `external_url=""`
- **THEN** 系統視為未設定，ExternalUrl 儲存為 null

### Requirement: 上傳店家 Cover 圖片
系統 SHALL 提供 `POST /api/admin/shops/{shopId}/cover-image` 端點，接收 multipart 檔案上傳。實作 SHALL 透過內部 HttpClient 將檔案 forward 至 image-upload pipeline (`POST /api/admin/upload`)。檔案驗證（副檔名 / magic bytes / 大小 / SkiaSharp 解碼）由 image-upload pipeline 負責，本 endpoint SHALL 將其錯誤碼（`INVALID_FILE_TYPE` / `INVALID_IMAGE` / `FILE_TOO_LARGE`）原樣回傳前端。image-upload 回傳的 `path` 成功後 SHALL 寫入 `Shop.CoverImagePath`。若該店原本已有 CoverImagePath，SHALL 直接覆蓋為新 path，舊 path 對應的 image-upload 檔案 SHALL NOT 被刪除（content-addressed，可能仍被其他 entity 引用）。

#### Scenario: 成功上傳 cover
- **WHEN** 管理員 POST 一個合法圖片檔到 `/api/admin/shops/{shopId}/cover-image`
- **THEN** 系統 forward 至 image-upload pipeline，取得 path 後寫入 Shop.CoverImagePath，回傳 200 與 `{ cover_image_path }`

#### Scenario: 替換既有 cover
- **WHEN** 管理員對已有 cover 的店家上傳新圖
- **THEN** 系統將 Shop.CoverImagePath 更新為新 path；舊 path 對應的 image-upload 檔案 SHALL NOT 被刪除

#### Scenario: 店家不存在
- **WHEN** 管理員 POST 到一個不存在的 shopId
- **THEN** 系統回傳 404

#### Scenario: 檔案驗證失敗（副檔名）
- **WHEN** 上傳的檔案副檔名不在白名單
- **THEN** 系統 SHALL forward image-upload 的錯誤碼 `INVALID_FILE_TYPE`

#### Scenario: 檔案驗證失敗（magic bytes）
- **WHEN** 上傳的檔案副檔名與 magic bytes 不一致
- **THEN** 系統 SHALL forward image-upload 的錯誤碼 `INVALID_IMAGE`

#### Scenario: 檔案過大
- **WHEN** 上傳的檔案大於 image-upload 上限
- **THEN** 系統 SHALL forward image-upload 的錯誤碼 `FILE_TOO_LARGE`

### Requirement: 透過 PUT 移除或更新 Cover 與 External URL
`PUT /api/admin/shops/{shopId}` 端點 SHALL 接受 `cover_image_path` 與 `external_url` 兩個 nullable 欄位。當送入 null 時系統 SHALL 將對應欄位清為 null（不另開 DELETE endpoint 處理移除 cover）。`cover_image_path` 直接作為字串接受（前端通常透過 cover 上傳 endpoint 取得 path 後再透過 PUT 套用，但允許直接傳遞 path 以保持 PUT 行為一致）。

#### Scenario: 透過 PUT 移除 cover
- **WHEN** 管理員 PUT shop 並把 `cover_image_path` 設為 null
- **THEN** 系統將 Shop.CoverImagePath 設為 null；image-upload 檔案 SHALL NOT 被刪除

#### Scenario: 透過 PUT 更新 external url
- **WHEN** 管理員 PUT shop 並提供有效的 `external_url`
- **THEN** 系統更新 Shop.ExternalUrl

#### Scenario: 透過 PUT 移除 external url
- **WHEN** 管理員 PUT shop 並把 `external_url` 設為 null 或空字串
- **THEN** 系統將 Shop.ExternalUrl 設為 null

### Requirement: Cover 上傳權限控制
`POST /api/admin/shops/{shopId}/cover-image` 端點 SHALL 透過 `[RequireRole(MenuConstants.ShopList, CrudAction.Update)]` 控制存取（替換 cover 屬於 update 行為）。

#### Scenario: 無權限上傳 cover
- **WHEN** 管理員角色無 ShopList Update 權限
- **THEN** 系統回傳 403

### Requirement: Admin 前台 Cover 與外部連結 UI
Admin 前台 SHALL 在 `/shop/create` 與 `/shop/[id]/edit` 兩頁的店家基本資訊表單內加入：

- **External URL 欄位**：`el-input` 文字框，placeholder 提示填寫 https URL，欄位錯誤訊息對應 `errors.external_url`，create 與 edit 頁皆可填寫
- **Cover 圖片區塊**：edit 頁顯示縮圖（若 `cover_image_path` 不為 null，透過 `useAssetHost.assetUrl(path)` 拼接）、上傳按鈕、移除按鈕；create 頁顯示提示「店家建立後可上傳 cover 圖片」（建立流程不上傳，建立完成後跳轉到 edit 頁再操作）

Cover 上傳 SHALL 使用既有 `useImageUploadQueue` composable（簡化為單張），呼叫 `POST /api/admin/shops/{shopId}/cover-image`。移除 cover SHALL 將表單 `form.cover_image_path` 設為 null，等使用者按「儲存」時透過 `PUT /api/admin/shops/{shopId}` 一起送出。

#### Scenario: edit 頁上傳新 cover
- **WHEN** 管理員在 `/shop/[id]/edit` 點擊上傳並選擇圖片
- **THEN** 前端呼叫 `POST /api/admin/shops/{shopId}/cover-image`，成功後縮圖立即顯示

#### Scenario: edit 頁移除 cover
- **WHEN** 管理員點擊「移除 cover」並按儲存
- **THEN** 前端透過 `PUT /api/admin/shops/{shopId}` 將 `cover_image_path` 設為 null

#### Scenario: create 頁不顯示上傳區
- **WHEN** 管理員進入 `/shop/create`
- **THEN** Cover 區塊顯示提示「店家建立後可上傳 cover 圖片」，不顯示上傳按鈕

#### Scenario: 兩頁都可填寫 external url
- **WHEN** 管理員在 create 或 edit 頁填寫 `external_url`
- **THEN** 提交時送出該欄位；無效格式時表單顯示後端回傳的錯誤訊息「請輸入有效的連結」
