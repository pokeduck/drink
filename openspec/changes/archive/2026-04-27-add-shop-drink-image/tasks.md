# Tasks: add-shop-drink-image

## 1. Backend — Entity & DB

- [x] 1.1 建立 `ShopImage` Entity（`api/Domain/Entities/ShopImage.cs`），實作 `ICreateEntity` + `IUpdateEntity`，欄位：`Id / ShopId / DrinkItemId? / Path / Hash / Sort / IsCover / OriginalFileName / FileSize / Width / Height + audit`
- [x] 1.2 `DrinkDbContext.OnModelCreating` 加入 ShopImage 設定：FK to Shop / DrinkItem、partial unique on `(ShopId, DrinkItemId)` WHERE IsCover、index `(ShopId, DrinkItemId, Sort)`、index `(ShopId) WHERE DrinkItemId IS NULL`、index `(Hash)` 非 unique

## 2. Backend — DTO & Mapper

- [x] 2.1 Request DTO（`api/Application/Requests/Admin/`）：`ShopImageUploadRequest`（query `drink_item_id` 即可，無需 body DTO）、`ShopImageUpdateRequest`、`ShopImageBatchDeleteRequest`、`ShopImageSortRequest`、`ShopImageListQuery`
- [x] 2.2 Response DTO（`api/Application/Responses/Admin/`）：`ShopImageResponse`（含 `path / hash / width / height / file_size / is_cover / sort / drink_item / original_file_name / created_at`）、`ShopImageListResponse`、`ShopImageBatchDeleteResponse`
- [x] 2.3 `ShopImageMapper`（Mapperly）entity ↔ response

## 3. Backend — Service

- [x] 3.1 `AdminShopImageService`（繼承 BaseService）— 列表（filter all/orphan/assigned + 品項篩選 + 分頁）
- [x] 3.2 取得單品項圖列表
- [x] 3.3 上傳：HttpClient forward 至 image-upload (`POST /api/upload/files`) 直接打 Upload.API；驗證 10 張上限；自動補封面；image-upload 的錯誤碼原樣 forward 回前端
- [x] 3.4 改綁品項：驗證新品項是否同店家、是否已達 10 張、處理封面遷移；Path 不變
- [x] 3.5 設封面：transaction 內切換 IsCover
- [x] 3.6 排序：批次更新單品項內 Sort
- [x] 3.7 移為孤兒：DrinkItemId=null、Sort=0、IsCover=false；若是封面，自動補下一張
- [x] 3.8 批量刪除：**僅刪 DB row**，不呼叫 image-upload 刪檔；封面遞補；transaction
- [x] 3.9 一鍵清孤兒：批量刪 DrinkItemId IS NULL 的 DB row

## 4. Backend — admin-shop-menu cascade

- [x] 4.1 `AdminShopService` 修改：軟刪 ShopMenuItem 時，cascade 將 (ShopId, DrinkItemId) 的所有 ShopImage 孤兒化（同 transaction）
- [x] 4.2 軟刪分類連帶軟刪品項時，同樣 cascade orphan 每個受影響品項
- [x] 4.3 `AdminShopService` 確認：軟刪店家時**不**直接動 ShopImage（圖片狀態變化只會經由 ShopMenuItem 級聯觸發）— Shop soft delete 路徑沒去動 ShopMenuItem，所以也不會觸發 image cascade（行為符合 spec）

## 5. Backend — Controller & ErrorCodes

- [x] 5.1 `ShopImagesController`（`api/Admin.API/Controllers/`）：所有端點套 `[RequireRole(MenuConstants.ShopList, CrudAction.X)]`
- [x] 5.2 GET 列表 / GET 品項圖 / POST 上傳 / PATCH 修改 / PATCH 排序 / DELETE 批量 / DELETE 孤兒
- [x] 5.3 加入新 Error Code（`ShopImageNotFound` / `ImageLimitReached` / `ImageInvalidDrinkItem`，4-06-XX）

## 6. Backend — Migration

- [x] 6.1 EF Core migration `20260426061105_AddShopImage`
- [x] 6.2 跑 Migrator 套用 schema 完成

## 7. API 型別產出

- [x] 7.1 重啟 Admin.API (5101)（讓新 ShopImagesController 生效）
- [x] 7.2 從根目錄執行 `pnpm generate`
- [x] 7.3 確認 `web/internal/api-types/src/admin.d.ts` 含 3 個新 path（含 `/api/admin/shops/{shopId}/images`、`.../images/orphans`、`.../drink-items/{drinkItemId}/images/sort`）+ 12 個新 schema（ShopImageResponse / ShopImageListResponse / ShopImageUpdateRequest 等）

## 8. Admin Frontend — Pool 頁

- [x] 8.1 新頁面 `web/apps/admin/app/pages/shop/[id]/images.vue`
- [x] 8.2 麵包屑、card header（含返回按鈕回 shop list、店家名稱）
- [x] 8.3 篩選 bar：全部 / 已使用 / 孤兒 / 指定品項（下拉用 `#id name` 顯示）+ 檔名搜尋
- [x] 8.4 動作 bar：批量刪除（confirm dialog）、一鍵清孤兒（confirm dialog）、上傳按鈕
- [x] 8.5 縮圖網格：`<img :src="assetUrl(image.path)">`，每張顯示綁定品項 badge（封面 ★）、孤兒 badge「未使用」、勾選框
- [x] 8.6 點縮圖開 Drawer
- [x] 8.7 上傳：拖放區（用 `useImageUploadQueue`），多檔序列上傳，進度條，達上限提示（pool 上傳預設孤兒）
- [x] 8.8 v-loading 套用至卡片內容
- [x] 8.9 mounted 時呼叫 `useAssetHost.ensureBaseUrl()`

## 9. Admin Frontend — 共用元件

- [x] 9.1 `ShopImageDrawer.vue`：大圖預覽（assetUrl(path)）、metadata（檔名、大小、寬高、上傳時間）、改綁品項下拉 + 儲存、設封面 toggle（限已綁品項）、移為孤兒按鈕、刪除按鈕（confirm dialog）
- [x] 9.2 `ShopImagePoolGrid.vue` 共用網格元件（複用於 pool 頁與「從孤兒池挑」Modal）
- [x] 9.3 `ShopImageItemPicker.vue` 「從孤兒池挑」Modal（顯示該店孤兒，多選加入）

## 10. Admin Frontend — Shop list / edit 整合

- [x] 10.1 `shop/list.vue` 列表每筆加「圖庫」按鈕，跳 `/shop/{id}/images`
- [x] 10.2 `shop/[id]/edit.vue` header 區加「圖庫管理」連結進 `/shop/{id}/images`
- [x] 10.3 在每個 ShopMenuItem 編輯 dialog 內加圖片管理子區塊（新元件 `ShopMenuItemImageStrip.vue`；只在 edit 模式 + 有 drink_item_id 時顯示，避免新增時 item 還沒 ID 就要塞圖的時序問題）：
  - [x] 10.3.1 橫向縮圖列（已綁本品項的圖，無拖拉排序）
  - [x] 10.3.2 上傳：選檔上傳，使用 `useImageUploadQueue`、達 10 張禁用、自動超量警告
  - [x] 10.3.3 設封面（每張縮圖下 ★ 按鈕）
  - [x] 10.3.4 移為孤兒（保留檔案）
  - [x] 10.3.5 徹底刪除（DB row）
  - [x] 10.3.6 「從孤兒池挑」按鈕 → 開 ShopImageItemPicker
  - 注：拖拉排序刻意不做，留待後續若有需求再開

> **後續 change backlog（`add-client-shop-drink-image`）**：Client 前台點餐流程接 ShopImage API；當品項回空陣列時，前端自動顯示 placeholder 圖（範例圖放 `web/apps/client/public/`）。本 change 不處理。

## 11. 雜項

- [x] 11.1 圖片相關欄位錯誤對應 — 沿用 `useApiFeedback.handleError`（自動把 errors 字典設到 serverErrors / 顯示 alert）
- [x] 11.2 Placeholder 圖片 — 不需要（Pool 頁有 empty state）
- [x] 11.3 AdminMenu 不新增 — 沿用 `MenuConstants.ShopList`（已驗證存在）

## 12. 驗收（用戶手動驗證通過）

- [x] 12.1 跑通端到端：上傳到 pool（孤兒）→ 在 Drawer 改綁品項 → 改封面 → 軟刪 ShopMenuItem 確認自動孤兒 → 一鍵清孤兒
- [x] 12.2 邊界測試：第 11 張上傳被擋、改綁到已滿品項被擋、刪除最後一張封面後沒有自動補（確認預期行為）、店家軟刪後圖仍存在且狀態不變
- [x] 12.3 去重驗證：兩家店上傳同一張圖 → 兩筆 ShopImage 但磁碟只一份；其中一家刪除 ShopImage 後，另一家仍能正常顯示
- [x] 12.4 權限測試:角色無 ShopList Update 不能 PATCH，無 Delete 不能批刪
- [x] 12.5 forward 錯誤碼：上傳 PDF / 12MB 檔 / 改副檔名的 PDF → 確認前端拿到 INVALID_FILE_TYPE / FILE_TOO_LARGE / INVALID_IMAGE

## 13. Build / Validate

- [x] 13.1 `dotnet build` 全綠
- [x] 13.2 Admin Nuxt build 全綠
- [x] 13.3 `openspec validate add-shop-drink-image --strict` 通過
