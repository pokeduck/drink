# admin-shop-image

## Purpose

定義後台店家圖片管理（ShopImage）契約：店家層級圖庫池（pool）、品項圖片綁定（每組 (ShopId, DrinkItemId) 至多 10 張、單一封面）、孤兒化（DrinkItemId=null）流程、圖片屬性更新（改綁 / 設封面 / 排序）、批量刪除與一鍵清孤兒、ShopMenuItem 軟刪時的 ShopImage 連帶孤兒化、image-upload pipeline 上傳代理與錯誤碼透傳，以及對應的 Admin 前台圖庫頁與品項編輯頁圖片管理 UI。

## Requirements

### Requirement: ShopImage 資料結構

系統 SHALL 維護 `ShopImage` Entity，欄位包含 `Id`、`ShopId`（FK Shop, NOT NULL）、`DrinkItemId`（FK DrinkItem, NULLABLE — null 表示孤兒）、`Path`（image-upload 回傳的相對路徑）、`Hash`（SHA-256，64 字元）、`Sort`、`IsCover`、`OriginalFileName`、`FileSize`、`Width`、`Height`、以及審計欄位（CreatedAt / Creator / UpdatedAt / Updater）。每組 `(ShopId, DrinkItemId)` 已綁定圖片數量 SHALL 不得超過 10 張；DB 層 SHALL 設置 partial unique index 確保每組 `(ShopId, DrinkItemId)` 至多一張 `IsCover = true`。`Hash` 欄位不做 unique（同 hash 可被多筆 ShopImage 引用，因 image-upload 是 content-addressed 去重設計）。

#### Scenario: 已綁定品項的圖片
- **WHEN** ShopImage 的 DrinkItemId 不為 null
- **THEN** Sort 與 IsCover 欄位有意義，可參與排序與封面選擇

#### Scenario: 孤兒圖片
- **WHEN** ShopImage 的 DrinkItemId 為 null
- **THEN** 該圖視為孤兒，Sort 值 SHALL 為 0、IsCover SHALL 為 false

#### Scenario: 兩家店引用同一張圖
- **WHEN** Shop=1 與 Shop=2 都上傳了內容相同的圖（Upload.API 去重後 hash 相同）
- **THEN** 系統 SHALL 建立兩筆獨立的 ShopImage，兩筆的 Path 與 Hash 完全相同，但各自有不同的 ShopId / DrinkItemId

### Requirement: 取得店家圖庫列表（Pool）

系統 SHALL 提供 `GET /api/admin/shops/{shopId}/images` 端點，支援分頁、`filter`（`all` / `orphan` / `assigned`）、`drink_item_id` 篩選、檔名 keyword 搜尋。回傳每張圖含 `id`、`path`、`hash`、`width`、`height`、`original_file_name`、`file_size`、`is_cover`、`sort`、`drink_item`（id + name 或 null）、`created_at`。預設排序：先依 DrinkItemId 群組（孤兒置最後），再依 Sort ASC、CreatedAt DESC。

#### Scenario: 列出全部圖片
- **WHEN** 管理員請求 `GET /api/admin/shops/1/images?filter=all&page=1&page_size=20`
- **THEN** 系統回傳該店所有 ShopImage（包含已綁與孤兒）

#### Scenario: 篩選孤兒圖片
- **WHEN** 管理員請求 `GET /api/admin/shops/1/images?filter=orphan`
- **THEN** 系統僅回傳 DrinkItemId IS NULL 的圖片

#### Scenario: 依品項篩選
- **WHEN** 管理員請求 `GET /api/admin/shops/1/images?drink_item_id=12`
- **THEN** 系統僅回傳該店綁定 DrinkItemId=12 的圖片，依 Sort ASC

#### Scenario: 店家不存在
- **WHEN** 管理員請求一個不存在的 shopId
- **THEN** 系統回傳 404

### Requirement: 取得單一品項的圖片列表

系統 SHALL 提供 `GET /api/admin/shops/{shopId}/drink-items/{drinkItemId}/images` 端點，回傳指定 (ShopId, DrinkItemId) 的所有圖片，依 Sort ASC 排序。封面（IsCover=true）的圖片 SHALL 在 response 中清楚標示。

#### Scenario: 成功取得單品項圖
- **WHEN** 管理員請求 `GET /api/admin/shops/1/drink-items/12/images`
- **THEN** 系統回傳該店該品項的所有圖（最多 10 張），依 Sort ASC

#### Scenario: 該品項目前無圖
- **WHEN** 該店該品項沒有任何圖
- **THEN** 系統回傳空陣列

### Requirement: 上傳圖片

系統 SHALL 提供 `POST /api/admin/shops/{shopId}/images` 端點接收 multipart 上傳，支援可選參數 `drink_item_id`：未提供則建立孤兒（DrinkItemId=null）；提供則綁定到指定品項。每次上傳一張檔案；前端如需多檔上傳 SHALL 多次呼叫此 API（透過 `useImageUploadQueue` 佇列）。

新圖建立時：
- 若綁定品項且該品項目前無封面，SHALL 自動設為封面（IsCover=true）
- 若綁定品項且該品項已有封面，新圖 IsCover=false
- Sort 預設為該 (ShopId, DrinkItemId) 群組現有最大 Sort + 1（孤兒則為 0）

實作 SHALL 透過內部 HttpClient 將檔案 forward 至 image-upload pipeline (`POST /api/admin/upload`)。檔案驗證（副檔名 / magic bytes / 大小 / SkiaSharp 解碼）由 image-upload pipeline 負責，本 endpoint SHALL 將其錯誤碼（`INVALID_FILE_TYPE` / `INVALID_IMAGE` / `FILE_TOO_LARGE`）原樣回傳前端。image-upload 回傳的 `path / hash / size / width / height` SHALL 寫入 ShopImage row。

#### Scenario: 上傳孤兒圖片
- **WHEN** 管理員上傳檔案但未提供 drink_item_id
- **THEN** 系統建立 ShopImage（DrinkItemId=null、Sort=0、IsCover=false）

#### Scenario: 上傳並綁定品項
- **WHEN** 管理員上傳檔案並提供有效的 drink_item_id（DrinkItem 存在於該 shop 的菜單中）
- **THEN** 系統建立 ShopImage 並綁定該品項，依規則處理封面與 Sort

#### Scenario: 達 10 張上限
- **WHEN** 管理員上傳到一個已有 10 張圖的 (ShopId, DrinkItemId)
- **THEN** 系統回傳 400 IMAGE_LIMIT_REACHED，errors 包含 `{ "drink_item_id": ["此品項圖片已達 10 張上限"] }`

#### Scenario: 檔案驗證失敗（副檔名）
- **WHEN** 上傳的檔案副檔名不在白名單
- **THEN** 系統 SHALL forward image-upload 的錯誤碼 `INVALID_FILE_TYPE`

#### Scenario: 檔案驗證失敗（magic bytes）
- **WHEN** 上傳的檔案副檔名與 magic bytes 不一致
- **THEN** 系統 SHALL forward image-upload 的錯誤碼 `INVALID_IMAGE`

#### Scenario: 檔案過大
- **WHEN** 上傳的檔案大於 image-upload 上限（10MB）
- **THEN** 系統 SHALL forward image-upload 的錯誤碼 `FILE_TOO_LARGE`

#### Scenario: 上傳同一張圖到不同店家
- **WHEN** 管理員上傳一張之前已被其他店家用過的圖（image-upload 去重後 hash 已存在）
- **THEN** 系統 SHALL 建立新的 ShopImage row（Path 與 Hash 與既有 row 相同），不報錯

### Requirement: 修改圖片屬性（改綁 / 設封面 / 排序）

系統 SHALL 提供 `PATCH /api/admin/shops/{shopId}/images/{imageId}` 端點，支援部分更新 `drink_item_id`、`is_cover`、`sort`。Path / Hash / Width / Height / FileSize 等檔案 metadata 屬性 SHALL NOT 可修改：

- **改綁品項**：當請求改變 `drink_item_id` 時：
  - 若新品項是 null → 同「移為孤兒」行為
  - 若新品項不為 null：必須屬於同一 ShopId 範圍下有效的 DrinkItem（`api/admin/shops/{shopId}/menu` 中存在），否則回 400 `IMAGE_INVALID_DRINK_ITEM`
  - 若新品項已達 10 張 → 回 400 `IMAGE_LIMIT_REACHED`
  - 改綁後：原品項若該圖為封面，自動補下一張為封面；新品項若無封面，新圖自動成為封面，否則 IsCover=false
  - **Path 不變**（image-upload 是 content-addressed，路徑不含 shop/drinkItem 資訊）
- **設封面**：當請求 `is_cover=true`：transaction 內先把該 (ShopId, DrinkItemId) 既有 IsCover=true 設為 false，再設目標圖 IsCover=true。對孤兒圖片設 is_cover=true SHALL 回 400
- **改 Sort**：直接更新該圖 Sort 值

#### Scenario: 改綁有效品項
- **WHEN** 管理員 PATCH 圖片，改 drink_item_id 從 12 → 15，且 15 在同 shop 且未滿
- **THEN** 系統更新 DrinkItemId=15、Sort 變新群組末位 + 1、依規則處理封面，原 12 群組若該圖為封面則自動補

#### Scenario: 改綁不影響 Path
- **WHEN** 管理員 PATCH 改 drink_item_id
- **THEN** ShopImage.Path 與檔案實際位置 SHALL NOT 變動

#### Scenario: 改綁到不同店家或不存在的品項
- **WHEN** 管理員 PATCH 改 drink_item_id 到一個不存在或不在該 shop 的 DrinkItem
- **THEN** 系統回傳 400 IMAGE_INVALID_DRINK_ITEM

#### Scenario: 改綁到已滿的品項
- **WHEN** 管理員 PATCH 改 drink_item_id 到一個已有 10 張圖的品項
- **THEN** 系統回傳 400 IMAGE_LIMIT_REACHED

#### Scenario: 設封面
- **WHEN** 管理員 PATCH 把某圖 is_cover=true，該圖目前有綁品項
- **THEN** 系統 transaction 把同群組舊封面改為 false、目標圖改為 true

#### Scenario: 對孤兒設封面
- **WHEN** 管理員 PATCH 把孤兒圖（DrinkItemId IS NULL）的 is_cover 改為 true
- **THEN** 系統回傳 400 VALIDATION_ERROR，errors 包含 `{ "is_cover": ["孤兒圖不可設為封面"] }`

### Requirement: 批次更新單品項圖片排序

系統 SHALL 提供 `PATCH /api/admin/shops/{shopId}/drink-items/{drinkItemId}/images/sort` 端點，接受 `items: [{ id, sort }]` 陣列，批次更新該品項圖片的 Sort 值。輸入清單 SHALL 涵蓋該品項目前所有圖片，否則回 400。

#### Scenario: 成功批次排序
- **WHEN** 管理員送出該品項所有圖的新 sort 值
- **THEN** 系統更新對應圖片的 Sort 值

#### Scenario: 排序清單缺漏
- **WHEN** 管理員送出的 items 數量與該品項目前圖片數不一致或包含他品項的 id
- **THEN** 系統回傳 400 VALIDATION_ERROR

### Requirement: 從品項移除圖片（孤兒化）

當管理員在品項管理 UI 點擊「移除」單張圖時，前端 SHALL 呼叫 `PATCH /api/admin/shops/{shopId}/images/{imageId}` 將 `drink_item_id` 設為 null。系統 SHALL 將該圖片改為孤兒：DrinkItemId=null、Sort=0、IsCover=false。若該圖原為該品項封面，系統 SHALL 自動把同品項剩餘圖中 Sort 最小的一張設為新封面（若無剩餘則該品項無封面）。

#### Scenario: 移除非封面圖
- **WHEN** 管理員把 DrinkItemId=12 的非封面圖改為孤兒
- **THEN** 該圖 DrinkItemId=null、Sort=0、IsCover=false；其他圖不變

#### Scenario: 移除封面圖且還有剩餘
- **WHEN** 管理員把 DrinkItemId=12 的封面圖改為孤兒，群組還有 3 張
- **THEN** 該圖 DrinkItemId=null、Sort=0、IsCover=false；剩餘圖中 Sort 最小者自動 IsCover=true

#### Scenario: 移除最後一張封面圖
- **WHEN** 管理員把 DrinkItemId=12 唯一的圖改為孤兒
- **THEN** 該圖變孤兒；該品項變成無封面

### Requirement: 批量刪除 ShopImage row

系統 SHALL 提供 `DELETE /api/admin/shops/{shopId}/images` 端點，接受 `ids: int[]`，批量刪除 ShopImage row。刪除 SHALL **僅刪 DB row、不刪 Upload.API 上的 webp 檔案**（因 image-upload 是 content-addressed 去重設計，可能有其他 entity 引用同一檔案）。對於每個刪除的圖：
- 若該圖為某品項封面，SHALL 自動把同品項剩餘圖中 Sort 最小者升為新封面
- 整批操作以單一 DB transaction 進行

#### Scenario: 批量刪除混合圖片
- **WHEN** 管理員送出包含已綁與孤兒的混合 ids 陣列
- **THEN** 系統刪除所有指定的 ShopImage row，封面自動遞補

#### Scenario: 刪除不影響 Upload.API 檔案
- **WHEN** 管理員刪除一張 ShopImage
- **THEN** Upload.API 上對應的 webp 檔案 SHALL NOT 被刪除（其他 ShopImage 或未來 entity 可能仍引用同 hash）

#### Scenario: 部分 id 不存在或不屬於該店
- **WHEN** 管理員送出包含不存在或屬於他店的 id
- **THEN** 系統回傳 400 並指出無效的 ids（VALIDATION_ERROR）

### Requirement: 一鍵清孤兒

系統 SHALL 提供 `DELETE /api/admin/shops/{shopId}/images/orphans` 端點，刪除該店所有 DrinkItemId IS NULL 的 ShopImage row（同樣不刪 Upload.API 檔案）。前端 UI SHALL 在執行前彈出確認 dialog，列出將被刪除的張數。

#### Scenario: 一鍵清孤兒
- **WHEN** 管理員確認執行清孤兒
- **THEN** 系統刪除該店所有孤兒 ShopImage row，回傳被刪除的張數

#### Scenario: 沒有孤兒圖
- **WHEN** 該店目前無孤兒圖
- **THEN** 系統回傳 200 與「刪除 0 張」訊息

### Requirement: ShopMenuItem 軟刪時圖片孤兒化

當 ShopMenuItem 被軟刪除時（直接刪除、分類軟刪連帶、店家軟刪連帶），系統 SHALL 在同一 transaction 內將該 (ShopId, DrinkItemId) 所有 ShopImage 孤兒化（DrinkItemId=null、Sort=0、IsCover=false）。Shop 軟刪除本身 SHALL **不**直接動 ShopImage（圖片狀態變化僅來自 ShopMenuItem 級聯）。

#### Scenario: 軟刪 ShopMenuItem
- **WHEN** 管理員軟刪 ShopId=1, DrinkItemId=12 的 ShopMenuItem
- **THEN** (ShopId=1, DrinkItemId=12) 所有 ShopImage DrinkItemId=null、Sort=0、IsCover=false

#### Scenario: 軟刪分類連帶軟刪品項
- **WHEN** 管理員軟刪某分類，底下含 DrinkItemId=12 與 DrinkItemId=15 兩個 ShopMenuItem
- **THEN** (ShopId, 12) 與 (ShopId, 15) 所有圖一併孤兒化

#### Scenario: 軟刪店家
- **WHEN** 管理員軟刪一個店家（連帶軟刪所有底下 ShopMenuItem）
- **THEN** 受影響的 (ShopId, DrinkItemId) 圖透過 ShopMenuItem 級聯孤兒化（行為由 ShopMenuItem 級觸發，非 Shop 級直接觸發）

### Requirement: 圖片管理權限控制

所有 `/api/admin/shops/{shopId}/images...` 端點 SHALL 透過 `[RequireRole(MenuConstants.ShopList, CrudAction.X)]` 控制存取（與 admin-shop / admin-shop-menu 共用同一權限項目）。GET 對應 Read、POST 對應 Create、PATCH 對應 Update、DELETE 對應 Delete。

#### Scenario: 無 Read 權限存取列表
- **WHEN** 管理員角色無 ShopList Read 權限
- **THEN** GET 端點回傳 403

#### Scenario: 無 Update 權限改綁品項
- **WHEN** 管理員角色無 ShopList Update 權限
- **THEN** PATCH 端點回傳 403

#### Scenario: 無 Delete 權限批量刪除
- **WHEN** 管理員角色無 ShopList Delete 權限
- **THEN** DELETE 端點回傳 403

### Requirement: 後台圖庫頁

Admin 前台 SHALL 提供 `/shop/{id}/images` 頁面作為店家圖庫管理介面，入口從 `/shop/list` 列表每筆的「圖庫」按鈕進入，並在 `/shop/{id}/edit` 頁 header 提供連結。頁面 SHALL 包含：

- 篩選列（全部 / 已使用 / 孤兒 / 指定品項；下拉以 `#id name` 顯示品項）+ 檔名搜尋
- 動作列（全選、批量刪除、一鍵清孤兒、上傳）
- 縮圖網格（圖片 src 透過 `useAssetHost.assetUrl(image.path)` 拼接），每張顯示綁定品項名稱（封面以 ★ 標示）或「未使用」badge、勾選框
- 點擊縮圖開啟右側 Drawer 顯示詳情、改綁品項、設封面、移為孤兒、刪除

頁面 v-loading 套用至卡片內容。Pool 上傳 SHALL 預設建立孤兒圖（不要求選擇品項）。上傳 UI SHALL 使用既有 composable `useImageUploadQueue`，串接到 `POST /api/admin/shops/{shopId}/images`。

#### Scenario: 從 shop list 進入圖庫
- **WHEN** 管理員在 `/shop/list` 點擊某店「圖庫」按鈕
- **THEN** 導航到 `/shop/{id}/images`，顯示該店所有圖片

#### Scenario: 篩選孤兒
- **WHEN** 管理員選擇篩選「孤兒」
- **THEN** 網格僅顯示 DrinkItemId IS NULL 的圖

#### Scenario: 點圖開 Drawer
- **WHEN** 管理員點擊縮圖
- **THEN** 右側 Drawer 滑出，顯示大圖預覽、metadata（含 width × height）、改綁下拉、設封面 toggle、移為孤兒按鈕、刪除按鈕

#### Scenario: 一鍵清孤兒前確認
- **WHEN** 管理員點擊「一鍵清孤兒」
- **THEN** 系統先彈出確認 dialog，顯示將刪除張數，使用者確認後才呼叫 API

### Requirement: 後台品項編輯頁圖片管理

Admin 前台在 `/shop/{id}/edit` 頁的每個 ShopMenuItem 編輯區塊內 SHALL 加入圖片管理子區塊，包含：

- 縮圖網格（已綁定本品項的圖片，依 Sort ASC），每張支援拖拉排序、點 ★ 設封面、點 X 移為孤兒
- 拖放上傳區，支援多檔序列上傳（達 10 張禁用並顯示 `n/10` 提示）— 使用 `useImageUploadQueue`
- 「從孤兒池挑」按鈕，點擊開 Modal 顯示該店孤兒圖（多選），確認後將選中圖改綁本品項

排序變更 SHALL 在使用者確認儲存時送出 PATCH 排序 API（不立即送出）。封面切換、移為孤兒、上傳 SHALL 即時送出 API（單次操作）。

#### Scenario: 在品項區上傳新圖
- **WHEN** 管理員在某 ShopMenuItem 的圖片區拖入檔案
- **THEN** 前端透過 `useImageUploadQueue` 一張一張呼叫 POST 上傳 API（drink_item_id=本品項 id），上傳完縮圖加入網格

#### Scenario: 達 10 張禁止上傳
- **WHEN** 該品項已有 10 張圖
- **THEN** 上傳區呈 disabled 狀態，顯示「已達 10 張上限」提示

#### Scenario: 從孤兒池挑既有圖
- **WHEN** 管理員點「從孤兒池挑」並選中數張孤兒圖
- **THEN** 前端逐一呼叫 PATCH API 將選中圖改綁本品項，受 10 張上限限制（超過則僅綁定到滿並提示其餘未綁定）

#### Scenario: 移除一張圖
- **WHEN** 管理員點某縮圖的 X 按鈕
- **THEN** 前端 PATCH drink_item_id=null，UI 從本品項網格移除該圖（移到 pool 的孤兒區）
