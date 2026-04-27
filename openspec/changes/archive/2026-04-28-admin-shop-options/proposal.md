## Why

兩個耦合在一起的痛點：

1. **品項編輯被全域 pool 噴爆** — 全域飲料選項（Sugar / Ice / Topping / Size）pool 會隨時間長到每種上百種，店家在編輯品項時被迫從整個全域 pool 挑選，UI 完全無法工程化處理。需要在店家層級先做一輪「啟用過濾」，讓品項管理只看到該店家用得到的子集。
2. **店家層級設定的入口分裂** — 目前 `/shop/[id]/edit`、`/shop/[id]/images` 走「店家列表行內按鈕」入口，而 `/shop/override` 卻是 sidemenu + 內建店家下拉。再多塞一個「選項啟用」會把入口模式撕成 3 種。趁目前只有 1 家店、零遷移成本的時間點，把所有店家層級設定整合成 hub-and-spoke layout（業界 Shopify / Stripe / Linear 同套）：店家有自己的 hub 頁，內含 sub-tabs。

順手把店家內排序統一收到啟用表，避免 sort 散落在 `ShopXxxOverride` 與品項層；override 表職責收窄為「Price 覆寫」。

## What Changes

### 新功能：店家層級啟用過濾
- 新增 4 張啟用表 `ShopEnabledSugar` / `ShopEnabledIce` / `ShopEnabledTopping` / `ShopEnabledSize`：有 row = 啟用、無 row = 停用，預設 default disable，沿用 `BaseDataEntity` 慣例（Id PK + `Unique(ShopId, OptionId)` + `Sort` 欄位）。
- 新增端點：
  - `GET /api/admin/shops/{shopId}/options`：四種選項全域 pool + 啟用狀態 + 店家內排序。
  - `POST /api/admin/shops/{shopId}/options/preview`：dry-run 模擬本次儲存後新停用的選項與受影響品項清單。
  - `PUT /api/admin/shops/{shopId}/options`：整批 delete-then-insert 啟用設定，並在同 transaction 內 cascade 刪除新停用選項對應的 `ShopMenuItemSugar / Ice / Topping / Size` row。
- 新增權限節點 `MenuConstants.ShopOptions`，獨立於 `ShopList` 與 `ShopOverride`。

### 重構：Shop Hub Pattern
- **BREAKING** 將 `/shop/[id]/edit` 升級為 hub layout，內含 sub-tabs：
  - 基本資訊 (`/shop/[id]/edit`)
  - 圖片管理 (`/shop/[id]/images` — 既有頁面整合進 hub layout，URL 不變)
  - 選項啟用 (`/shop/[id]/options` — 新)
  - 覆寫設定 (`/shop/[id]/overrides` — **route 從 `/shop/override` 移到 `/shop/[id]/overrides`**，撤離 sidemenu)
- 店家列表行內按鈕仍只有「編輯」一顆（即進入 hub 的入口）；hub 內透過 sub-tabs 切換。
- AdminMenu 新增欄位 `IsPermissionOnly`（bool, default false）：標示此 menu 不渲染 sidemenu，只作為權限載體。`AdminMenuRoleSeeder` 葉節點判斷由 `Endpoint != null` 放寬為 `Endpoint != null || IsPermissionOnly`。
- AdminMenu seed 變更：
  - **MODIFY** Id=17 (覆寫設定)：`Endpoint` 從 `/shop/override` → `/shop/[id]/overrides`，`IsPermissionOnly = true`。
  - **ADD** Id=23 (選項管理)：`Endpoint = /shop/[id]/options`, `IsPermissionOnly = true`。
- 前端 sidemenu 渲染：過濾掉 `is_permission_only=true` 的葉節點。
- 前端 permission middleware 的 route → menuId 對應表新增 `/shop/[id]/overrides` 與 `/shop/[id]/options`。

### 修改：admin-shop-menu
- **BREAKING** `POST/PUT items` 驗證 `sugar_ids / ice_ids / topping_ids / size_ids` 必須在該店家啟用清單內，否則回 `OPTION_NOT_ENABLED_FOR_SHOP`。
- `GET /api/admin/shops/{shopId}/menu` 回傳的「可選池」由全域改為店家啟用子集。

### 修改：admin-shop-override
- **BREAKING** `ShopSugarOverride.Sort` / `ShopToppingOverride.Sort` 欄位 DROP（DB + DTO 移除）。店家內排序統一由啟用表負責。
- **BREAKING** 前端入口從 sidemenu `/shop/override`（內建店家下拉）改為 hub 內 `/shop/[id]/overrides` tab；shopId 由 route param 取得，不再有店家下拉。

### 修改：admin-permission
- 新增 `AdminMenu.IsPermissionOnly` 欄位與相關 menus/me Response DTO 變更。
- 新增 `MenuConstants.ShopOptions` 與對應 AdminMenu seed。
- `AdminMenuRoleSeeder` 葉節點判斷放寬。

### Migration
- 4 張啟用表上線即全空；現有 1 家店上線後手動到 hub 內 `/shop/[id]/options` tab 補設。
- AdminMenu schema 加 `is_permission_only` 欄；Id=17 改路徑 + 設旗標、Id=23 INSERT；對應 admin_menu_role 補 system role 全 CRUD。

## Capabilities

### New Capabilities
- `admin-shop-options`: 店家層級飲料選項啟用過濾，含啟用清單、店家內排序、dry-run preview、cascade 刪除既有品項對應 row 的整批更新流程。
- `admin-shop-hub`: 店家層級設定的 hub-and-spoke UI 模式：基本資訊 / 圖片 / 選項 / 覆寫等子設定整合至 `/shop/[id]/*` 子 tab，店家列表行內按鈕為唯一進入點。

### Modified Capabilities
- `admin-shop-menu`: 品項可選池由全域改為店家啟用子集；新增/更新品項時驗證選項是否在啟用清單內。
- `admin-shop-override`: 移除 Sort 欄位（職責移交啟用表）；前端 route 從 sidemenu `/shop/override` 移至 hub `/shop/[id]/overrides`。
- `admin-permission`: 新增 `AdminMenu.IsPermissionOnly` 欄位與 sidemenu 過濾規則；新增 `ShopOptions` 權限節點。

## Impact

- **資料庫**：
  - 新增 4 張表（`ShopEnabledSugar` / `ShopEnabledIce` / `ShopEnabledTopping` / `ShopEnabledSize`）。
  - 移除 `ShopSugarOverride.Sort`、`ShopToppingOverride.Sort` 兩欄。
  - `AdminMenu` 新增 `IsPermissionOnly` 欄。
  - 修改 1 筆 AdminMenu (Id=17) + 新增 1 筆 (Id=23)，並補 system role admin_menu_role row。
- **後端**：
  - 新增 `AdminShopOptionService` 與 `ShopOptionsController`。
  - 修改 `AdminShopMenuService`（驗證 + GET 過濾）、`AdminShopOverrideService`（拿掉 Sort）。
  - `AdminMenuRoleSeeder` 葉節點判斷邏輯放寬。
  - menus/me Response DTO 新增 `is_permission_only`。
- **前端**：
  - 新增 hub layout `/shop/[id]/_layout.vue`（或等效機制：useShopHubTabs）。
  - 新增 `web/apps/admin/app/pages/shop/[id]/options.vue`、`overrides.vue`。
  - 既有 `web/apps/admin/app/pages/shop/[id]/edit.vue` 與 `images.vue` 包進 hub layout。
  - 移除 `web/apps/admin/app/pages/shop/override.vue` 或留 redirect 至 `/shop/list`。
  - sidemenu 渲染加 `is_permission_only` 過濾。
  - permission middleware route 對應表擴充。
  - menuConstants.ts 新增 `ShopOptions`。
- **API 型別**：`pnpm generate` 重生 `web/internal/api-types/src/admin.d.ts`。
- **權限**：既有 system role (id=1) 透過 migration 自動補 ShopOptions 全 CRUD；其他角色需手動賦予。
- **上線**：因啟用表全空、品項 GET 回傳空可選池，現存品項在新 UI 看不到子選項，需於上線視窗手動到 `/shop/[id]/options` 為唯一一家既有店家補設。
