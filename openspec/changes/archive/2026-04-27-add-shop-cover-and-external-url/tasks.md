## 1. Domain & Validator

- [x] 1.1 在 `Shop.cs` 加入 `CoverImagePath`（`string?`, `[StringLength(500)]`）與 `ExternalUrl`（`string?`, `[StringLength(500)]`）兩個 nullable property
- [x] 1.2 新增 `api/Application/Attributes/HttpUrlAttribute.cs`（與 `RequireRoleAttribute` 同 namespace），實作 `ValidationAttribute`：null/空字串視為 valid；其他用 `Uri.TryCreate(value, UriKind.Absolute, out var uri)` + `uri.Scheme is "http" or "https"` 判斷；錯誤訊息「請輸入有效的連結」

## 2. Migration

- [x] 2.1 執行 `dotnet ef migrations add AddShopCoverAndExternalUrl --project api/Infrastructure --startup-project api/Migrator --output-dir Migrations`
- [x] 2.2 檢查產生的 migration：兩個 nullable column，無需資料遷移
- [x] 2.3 執行 `dotnet run --project api/Migrator` 套用

## 3. DTO & Mapper

- [x] 3.1 `CreateShopRequest`：加入 `CoverImagePath`（`string?`, `[StringLength(500)]`）與 `ExternalUrl`（`string?`, `[StringLength(500)]`, `[HttpUrl]`）
- [x] 3.2 `UpdateShopRequest`：同步加入兩個欄位
- [x] 3.3 `ShopDetailResponse`：加入 `CoverImagePath` 與 `ExternalUrl`
- [x] 3.4 確認 `ShopListResponse` 不加新欄位（列表不需 cover）
- [x] 3.5 `ShopMapper.cs`：Mapperly 自動處理新欄位（依 RequiredMappingStrategy.Target 預設行為），確認編譯通過

## 4. Service & Controller

- [x] 4.1 `AdminShopService`：Create / Update 帶上 `CoverImagePath` 與 `ExternalUrl`（含 `NormalizeExternalUrl` 把空白字串視為 null）
- [x] 4.2 新增 `UploadCoverImage(int shopId, IFormFile file, CancellationToken)`：透過 `AdminShopImageService.ForwardToImageUpload`（提升為 internal 共用）forward 到 image-upload pipeline；成功後寫入 `Shop.CoverImagePath`；錯誤碼透傳
- [x] 4.3 `ShopsController` 新增 `[HttpPost("{shopId}/cover-image")]` action，標註 `[RequireRole(MenuConstants.ShopList, CrudAction.Update)]`，接收 `IFormFile`，回傳 `ShopCoverImageUploadResponse`（含 `cover_image_path`）
- [x] 4.4 既有 `POST /api/admin/shops` 與 `PUT /api/admin/shops/{shopId}` 已接收新欄位並正確處理 null

## 5. API 型別生成

- [x] 5.1 啟動 Admin.API（5101），確認 swagger 含新 endpoint 與欄位
- [x] 5.2 從根目錄執行 `pnpm generate`
- [x] 5.3 確認 `web/internal/api-types/src/admin.d.ts` 含 `cover_image_path` / `external_url` / cover-image endpoint

## 6. 前端 — Edit 頁

- [x] 6.1 `web/apps/admin/app/pages/shop/[id]/edit.vue` 的 `form` reactive 加入 `cover_image_path: null as string | null` 與 `external_url: ''`
- [x] 6.2 `fetchShop` 帶入 `item.cover_image_path` 與 `item.external_url`
- [x] 6.3 `handleSubmitShop` body 帶上 `cover_image_path: form.cover_image_path` 與 `external_url: form.external_url || null`
- [x] 6.4 表單加入 cover 區塊：縮圖預覽（用 `useAssetHost.assetUrl(form.cover_image_path)`）、上傳按鈕（用 `useImageUploadQueue<{ cover_image_path: string }>`，POST `/admin/shops/{shopId}/cover-image`）、移除按鈕（清 `form.cover_image_path`）
- [x] 6.5 表單加入 `external_url` 欄位：`el-input`，`prop="external_url"`，`:error="serverErrors.external_url"`
- [x] 6.6 `useUnsavedGuard` 自動涵蓋新欄位（form 是同一個 reactive，無需修改）

## 7. 前端 — Create 頁

- [x] 7.1 `web/apps/admin/app/pages/shop/create.vue` 的 form 加入 `external_url: ''`
- [x] 7.2 表單加入 `external_url` 欄位
- [x] 7.3 表單加入 cover 提示區塊（不上傳，提示「店家建立後可在編輯頁上傳封面圖片」）
- [x] 7.4 提交 body 帶上 `external_url: form.external_url || null`，不傳 `cover_image_path`
- [x] 7.5 建立成功後改為跳轉到新店家的 `/shop/{id}/edit`（取代原本的 `/shop/list`），方便使用者立即上傳 cover

## 8. Spec Archive 準備

- [x] 8.1 確認 `openspec validate add-shop-cover-and-external-url --strict` 通過
- [x] 8.2 手動測試：edit 頁上傳 cover、替換 cover、移除 cover、external_url 各種有效/無效輸入（並修正 ModelState 自動驗證 errors key 為 snake_case 的全域行為）
- [x] 8.3 手動測試：create 頁提交 external_url、無 cover 上傳區
- [x] 8.4 全部驗證通過後執行 `/opsx:archive` 將 change 套用至 `openspec/specs/admin-shop/spec.md`
