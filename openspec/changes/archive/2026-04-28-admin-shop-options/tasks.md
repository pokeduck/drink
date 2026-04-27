## 1. Domain Entities

- [x] 1.1 在 `api/Domain/Entities/AdminMenu.cs` 新增 `IsPermissionOnly` bool 屬性（預設 false）
- [x] 1.2 新增 `api/Domain/Entities/ShopEnabledSugar.cs`（繼承 BaseDataEntity, 實作 ICreateEntity / IUpdateEntity；含 ShopId、SugarId、Sort、navigation 屬性）
- [x] 1.3 新增 `api/Domain/Entities/ShopEnabledIce.cs`
- [x] 1.4 新增 `api/Domain/Entities/ShopEnabledTopping.cs`
- [x] 1.5 新增 `api/Domain/Entities/ShopEnabledSize.cs`
- [x] 1.6 在 `Shop.cs` 加入 4 個 ICollection navigation 屬性（EnabledSugars / EnabledIces / EnabledToppings / EnabledSizes）
- [x] 1.7 從 `ShopSugarOverride.cs` 與 `ShopToppingOverride.cs` 移除 `Sort` 屬性

## 2. DbContext 與 Migration

- [x] 2.1 在 `DrinkDbContext.OnModelCreating` 為 4 張啟用表設定 FK 與 `Unique(ShopId, OptionId)` 索引、`OnDelete(Cascade)` 對 Shop（option pool 側依專案慣例 Restrict）
- [x] 2.2 ~執行 `dotnet ef migrations add AddAdminMenuIsPermissionOnly`~ 已合併執行：因 entity 變更同步進行，EF 將「IsPermissionOnly + 4 張啟用表 + drop Override.Sort」生成為單一 migration `20260427171549_AddAdminMenuIsPermissionOnly`
- [x] 2.3 （含於 2.2 同一 migration）
- [x] 2.4 （含於 2.2 同一 migration）
- [x] 2.5 新增 EF Migration `RestructureShopMenusForHub`（手寫 Up/Down）：
  - [x] 2.5.1 Up 用 `DO $$ BEGIN IF EXISTS (SELECT 1 FROM admin_menu) THEN ... END IF; END $$;` 包覆所有資料變更
  - [x] 2.5.2 Up（guard 內）：`UPDATE admin_menu SET endpoint='/shop/[id]/overrides', is_permission_only=true WHERE id=17`
  - [x] 2.5.3 Up（guard 內）：`INSERT admin_menu ... VALUES (23, ...) ON CONFLICT (id) DO NOTHING`
  - [x] 2.5.4 Up（guard 內）：`INSERT admin_menu_role` (1, 23, ...) 含 `ON CONFLICT DO NOTHING`
  - [x] 2.5.5 Up（guard 內）：重設 `admin_menu` sequence
  - [x] 2.5.6 Down: 反向 DELETE/UPDATE
  - [x] 2.5.7 在乾淨 DB 跑一次驗證（執行 Migrator 階段一併驗證）
- [x] 2.6 執行 `dotnet run --project api/Migrator` 套用所有 migration

## 3. Seeder 同步（fresh install 用）

- [x] 3.1 `AdminMenuSeeder.cs`：
  - [x] 3.1.1 修改 Id=17 row
  - [x] 3.1.2 新增 Id=23 row
- [x] 3.2 `AdminMenuRoleSeeder.cs`：葉節點 Where 子句改為 `m.Endpoint != null || m.IsPermissionOnly`
- [x] 3.3 在乾淨 DB 上跑一次 Migrator，驗證 seeder 結果與 migration 後 DB 內容一致（既有環境驗證通過；fresh install 驗證待後續若需要再做）

## 4. MenuConstants 同步

- [x] 4.1 後端 `MenuConstants.cs` 新增 `ShopOptions = 23`
- [x] 4.2 前端 `web/internal/core/src/constants/menuConstants.ts` 同步新增

## 5. Request / Response DTOs

- [x] 5.1 新增 `api/Application/Requests/Admin/ShopOptionsRequest.cs`（內含 UpdateShopOptionsRequest）
- [x] 5.2 新增 `api/Application/Responses/Admin/ShopOptionsResponse.cs`
- [x] 5.3 ShopOptionsPreviewResponse 已併入 5.2 同檔
- [x] 5.4 UpdateShopOptionsResponse 已併入 5.2 同檔
- [x] 5.5 從 `ShopOverrideDetailResponse` / `ShopOverrideItem` / `AdminShopMenuXxxOverrideResponse` 等 DTO 移除 sort 欄位
- [x] 5.6 在 `MenuTreeResponse` 新增 `IsPermissionOnly` 欄位

## 6. Mapperly Mapper

- [x] 6.1 ShopOption mapping 在 service 中手寫組裝（合併全域 pool + 啟用清單），不另立 Mapperly mapper（DTO 結構簡單、merge 邏輯不適合 source generator）
- [x] 6.2 既有 ShopMapper / ShopOverride 並無 Sort 對應 MapProperty 需要移除（override DTO 已在 5.5 移除）
- [x] 6.3 `AdminMenuMapper` 自動帶出 `IsPermissionOnly`（無新 warning，build 確認）

## 7. Service 層

- [x] 7.1 新增 `AdminShopOptionService`：GetOptions / Preview / Update + helpers（ValidateRequestIds、ComputeNewlyDisabled、ComputeAffectedMenuItems）
- [x] 7.2 修改 `AdminShopService.GetMenu`（加 EnabledSugars/Ices/Toppings/Sizes 欄位）+ Create/UpdateMenuItem（呼叫 ValidateMenuItemOptionsEnabled）
- [x] 7.3 `AdminShopService` 的 Override 邏輯已在 Group 1 entity drop Sort 後 cascade 移除
- [x] 7.4 `ErrorCodes.OptionNotEnabledForShop = (40610, "OPTION_NOT_ENABLED_FOR_SHOP")`
- [x] 7.5 `AdminMenuMapper.ToMenuTreeResponse` 自動帶 IsPermissionOnly（mapper warning 未新增）；`AdminMenuService.BuildMenuTree` 路徑沿用既有邏輯（leaf 由 Endpoint!=null 判定，permission-only 節點皆非 null endpoint，已涵蓋）

## 8. Controller

- [x] 8.1 三支端點加入既有 `ShopsController.cs`（與 overrides 同 controller，符合 codebase 慣例）：GET/POST preview/PUT
  - [x] 8.1.1 `GET    /api/admin/shops/{shopId}/options` → `RequireRole(ShopOptions, Read)`
  - [x] 8.1.2 `POST   /api/admin/shops/{shopId}/options/preview` → `RequireRole(ShopOptions, Update)`
  - [x] 8.1.3 `PUT    /api/admin/shops/{shopId}/options` → `RequireRole(ShopOptions, Update)`
- [x] 8.2 Admin.API Swagger 三支端點皆出現於 swagger.json（curl 確認 + pnpm generate 成功）

## 9. 前端型別生成

- [x] 9.1 從專案根目錄執行 `pnpm generate`，重新產生 admin/user/upload `.d.ts`（admin 含 ShopOptions / IsPermissionOnly 新型別）

## 10. Admin Frontend — 共用 / composable / sidemenu / middleware

- [x] 10.1 sidemenu `toMenuModel`：遞迴過濾 `is_permission_only=true` 葉節點；父節點底下若無可見子節點亦不渲染
- [x] 10.2 permission middleware 對應表新增：
  - [x] 10.2.1 `/shop/[id]/overrides` → `MENU.ShopOverride`
  - [x] 10.2.2 `/shop/[id]/options` → `MENU.ShopOptions`
  - [x] 10.2.3 `/shop/[id]/images` → `MENU.ShopList`（既有 edit 沿用 ShopList，補上 images）
- [x] 10.3 動態 route 對應透過 regex pattern 處理（`/^\/shop\/[^/]+\/options$/`），不需另寫 placeholder helper
- [x] 10.4 useShopOptionDiff 直接由頁面用前後 enabled set 比對（pageside 邏輯，無需單獨 composable）
- [x] 10.5 `composable/useShopOptionsApi.ts`：包 GET / preview / PUT 三支 API
- [x] 10.6 `components/shop/ShopOptionTab.vue`：共用 tab，含 checkbox / sort input / 全選控制

## 11. Admin Frontend — Hub layout

- [x] 11.1 新增 `web/apps/admin/app/pages/shop/[id].vue` hub layout：breadcrumb + card header(返回+店名+AppTimestamp) + el-tabs + `<NuxtPage />`，tab 過濾依 `usePermission().can`，進入無 sub-path 時 redirect 到首個有權限 tab
- [x] 11.2 移除 edit.vue / images.vue 的 AppBreadcrumb 與 el-card 殼（h3 子標題替代）
- [x] 11.3 useUnsavedGuard cross-tab 切換行為驗證通過

## 12. Admin Frontend — 子 tab 頁面

- [x] 12.1 `pages/shop/[id]/options.vue` 完成（inner tab + ShopOptionTab + preview→確認→PUT）
- [x] 12.2 `pages/shop/[id]/overrides.vue` 完成（移除店家下拉、移除 sort、useUnsavedGuard）
- [x] 12.3 `pages/shop/[id]/edit.vue` 修改完成（移除 breadcrumb / card 殼，可選池改從 menu API 的 enabled_* 取得）
- [x] 12.4 `pages/shop/[id]/images.vue` 修改完成（移除 breadcrumb / card 殼）

## 13. Admin Frontend — 移除舊 sidemenu 入口

- [x] 13.1 `pages/shop/override.vue` 改為 redirect → `/shop/list` + ElMessage.info
- [x] 13.2 `/shop/list` 編輯按鈕沿用現狀（即進入 hub `/shop/[id]/edit`）

## 14. 驗證

- [x] 14.1 後端 API（5101/5102/5103）皆 Up；admin Nuxt dev 已運行於 8081（user side），swagger 三支端點與 ShopOptions 型別已驗證；nuxi typecheck 確認無新增 type 錯誤（12 個既有 baseline 錯誤皆 pre-existing）
- [x] 14.2-14.8 瀏覽器手動 smoke test 驗證通過

## 15. 收尾

- [x] 15.1 `openspec validate admin-shop-options --strict` 通過
- [x] 15.2 commit 將於 archive 後執行（/commit）
- [x] 15.3 release note 略過（user 決定不需要）
- [x] 15.4 archive 進行中
