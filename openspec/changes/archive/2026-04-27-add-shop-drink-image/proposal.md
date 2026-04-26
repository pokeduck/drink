# Proposal: 店家品項圖片管理（admin-shop-image）

## Why

前台 (Client) 使用者在揪團點餐時無法看到品項實品圖，影響選擇體驗。需要讓後台管理員能為每個 Shop × DrinkItem 上傳最多 10 張參考圖，並在前台點餐流程中呈現。

設計重點：
- DrinkItem 是全域主檔，但同品項在不同店家實際長相不同 → 圖片必須以 `(Shop, DrinkItem)` 為鍵綁定
- DrinkItem 可改名（既有功能），但圖片透過 `DrinkItemId` 綁定，改名不影響圖片連結
- 管理者既需要「在編品項時順手上傳」的快速動線，也需要「跨品項整批清理」的圖庫視角

## What Changes

新增獨立 capability `admin-shop-image`，包含：

- **新 Entity** `ShopImage`：`(ShopId, DrinkItemId nullable, Path, Hash, Sort, IsCover, OriginalFileName, FileSize, Width, Height, ...)`
- **新 API 表面** `/api/admin/shops/{shopId}/images`：上傳、列表、改綁、排序、設封面、移為孤兒、批量刪除、一鍵清孤兒
- **新前台頁面（Admin）** `/shop/[id]/images`：店家圖庫 (Pool) 視圖
  - 入口從 Shop list（每列加按鈕）與 Shop edit header 連結進入
  - 支援篩選（全部 / 已使用 / 孤兒 / 指定品項）
  - 縮圖點擊開 Drawer 顯示詳情，可改綁品項、設封面、移為孤兒
  - 多選 + 批量刪除（DB row 刪除）+ 一鍵清孤兒（含確認）
- **品項編輯動線擴充**：在 Shop edit 頁的 ShopMenuItem 表單區塊內，加入圖片管理子區塊（上傳 / 排序 / 設封面 / 從孤兒池挑既有圖 / 移除）
- **ShopMenuItem 軟刪行為擴充**：軟刪時，該 (ShopId, DrinkItemId) 的所有圖片自動孤兒化，IsCover 重置
- **沿用 image-upload pipeline**：上傳走 `/api/admin/upload`（已具備 magic bytes、SkiaSharp、長邊 4000 縮、WebP@85、SHA-256 去重），ShopImage 只儲存回傳的相對 path 與 metadata
- **不刪除實體檔案**：ShopImage 「刪除」操作只刪 DB row，不動 Upload.API 上的 webp 檔（image-upload pipeline 是 content-addressed 去重，刪檔可能誤傷其他引用）。日後排程清孤兒檔案另開 change

修改既有 capability `admin-shop-menu`：
- 「刪除品項」requirement 加上「同時觸發圖片孤兒化」的副作用 scenario

前台 (Client) 顯示先不在此 change 範圍。後續另開 change 處理 `client-pages` 的點餐流程圖片呈現。

## Capabilities

### New Capabilities
- `admin-shop-image`: ShopImage Entity、所有 `/api/admin/shops/{shopId}/images...` 端點、cascade 孤兒化邏輯、後台圖庫頁與品項區塊整合 UI

### Modified Capabilities
- `admin-shop-menu`: 「刪除/軟刪 ShopMenuItem」要 cascade 將同 (ShopId, DrinkItemId) 的 ShopImage 孤兒化

## Impact

**Affected specs:**
- `admin-shop-image` (ADDED — new capability)
- `admin-shop-menu` (MODIFIED — 刪除品項 cascade 行為)

**Affected code:**
- `api/Domain/Entities/ShopImage.cs`（新）
- `api/Infrastructure/Data/DrinkDbContext.OnModelCreating`（FK + partial unique index）
- `api/Application/Requests/Admin/ShopImage*.cs`（新）
- `api/Application/Responses/Admin/ShopImage*.cs`（新）
- `api/Application/Mappings/ShopImageMapper.cs`（新）
- `api/Application/Services/AdminShopImageService.cs`（新）
- `api/Admin.API/Controllers/ShopImagesController.cs`（新）
- `api/Application/Services/AdminShopMenuService.cs`（修改：軟刪 cascade orphan 邏輯）
- `web/apps/admin/app/pages/shop/[id]/images.vue`（新）
- `web/apps/admin/app/pages/shop/list.vue`（新增「圖庫」按鈕）
- `web/apps/admin/app/pages/shop/[id]/edit.vue`（菜單品項區塊整合圖片管理 UI）
- `web/apps/admin/app/components/`（新增 ShopImagePoolGrid、ShopImageDrawer、ShopImageItemSelector 等元件）
- `web/apps/admin/app/composable/useImageUploadQueue.ts`（既有，本 change 直接複用）
- `web/apps/admin/app/composable/useAssetHost.ts`（既有，本 change 直接複用）
- 新增 EF Core migration `AddShopImage`
- `pnpm generate` 後重新產生 admin api-types

**Permissions:** 沿用 `MenuConstants.ShopList`（與 admin-shop / admin-shop-menu 共用），不新增 AdminMenu 項目，動線從既有 Shop 管理頁進入。
