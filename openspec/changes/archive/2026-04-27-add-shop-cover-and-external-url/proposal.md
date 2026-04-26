## Why

店家在 Admin 後台目前無法上傳 cover 圖片或登錄外部連結（例如 Google Map URL），管理員需要這兩個欄位來呈現店家識別圖與導引訊息。

## What Changes

- Shop Entity 新增兩個 nullable 欄位：`CoverImagePath`（圖片路徑，max 500）與 `ExternalUrl`（外部連結，max 500，僅接受 http / https URL）
- 新增獨立 endpoint `POST /api/admin/shops/{shopId}/cover-image`：multipart 上傳，內部 forward 至 image-upload pipeline，成功後寫入 `Shop.CoverImagePath`
- `POST /api/admin/shops` 與 `PUT /api/admin/shops/{shopId}` 接受 `cover_image_path` 與 `external_url`；移除 cover 透過 `PUT` 把 `cover_image_path` 設為 null（不另開 DELETE endpoint）
- `GET /api/admin/shops/{shopId}` 回傳 `cover_image_path` 與 `external_url`
- 自製 `HttpUrlAttribute`：用 `Uri.TryCreate` + 限制 scheme 為 http / https，驗證失敗訊息為「請輸入有效的連結」
- Admin 前台 `/shop/create` 與 `/shop/[id]/edit` 兩頁加入 cover 圖片上傳區塊（用既有 `useImageUploadQueue` 簡化為單張）與外部連結欄位
- 換 cover 時舊圖檔保留於 image-upload server（content-addressed，不刪檔）

## Capabilities

### New Capabilities
（無）

### Modified Capabilities
- `admin-shop`：Shop entity 新增 cover 圖片與外部連結欄位、新增 cover 上傳 endpoint、CRUD 與詳情 API 增加對應 request / response 欄位

## Impact

- Entity：`api/Domain/Entities/Shop.cs`
- Migration：新建一個 EF Core migration（兩個 nullable 欄位，無資料遷移負擔）
- Validator：新建 `HttpUrlAttribute`
- DTO：`ShopRequest.cs`（CreateShopRequest / UpdateShopRequest）、`ShopResponse.cs`（ShopDetailResponse；ShopListResponse 不加，列表不需 cover）
- Mapper：`ShopMapper.cs` 自動映射新欄位
- Service / Controller：`ShopsController` 新增 cover 上傳 action；既有 CRUD action 帶上新欄位
- 前端：`web/apps/admin/app/pages/shop/create.vue`、`web/apps/admin/app/pages/shop/[id]/edit.vue`
- 不影響：`ShopImage` entity / `admin-shop-image` 既有契約與其他 capability
