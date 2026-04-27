## ADDED Requirements

### Requirement: AdminMenu permission-only node
`AdminMenu` 實體 SHALL 新增 `IsPermissionOnly`（boolean，預設 false）欄位，標示此 menu row 僅作為權限載體。`IsPermissionOnly=true` 的 row 仍 SHALL 出現在 `/menus/me` 回應，供前端 route middleware 判定權限；但 SHALL 不渲染為 sidemenu entry。

`AdminMenuRoleSeeder` SHALL 將「葉節點」判定由 `Endpoint != null` 放寬為 `Endpoint != null || IsPermissionOnly`，使 permission-only node 在 fresh seed 時亦自動取得 system role 全 CRUD。

`menus/me` Response DTO SHALL 包含 `is_permission_only` 欄位（snake_case）。

#### Scenario: permission-only node 不在 sidemenu 顯示
- **WHEN** 前端 sidemenu 渲染，某 leaf menu `is_permission_only=true`
- **THEN** sidemenu SHALL 不渲染該 leaf（其父群組若無其他 leaf 也 SHALL 隱藏父群組）

#### Scenario: permission-only node 出現在 menus/me
- **WHEN** 前端呼叫 `/menus/me`
- **THEN** 回應 SHALL 包含所有使用者有權限的 menu row（含 is_permission_only=true 的 row）

#### Scenario: route middleware 套用權限
- **WHEN** 管理員直接輸入 `/shop/[id]/options` 但無 ShopOptions Read 權限
- **THEN** route middleware SHALL 透過 ShopOptions 對應的 menu row（IsPermissionOnly=true）進行權限判定，並在無權限時導回首頁

#### Scenario: System role 自動拿到 permission-only node 權限
- **WHEN** Migrator 在新環境執行 seed，某 menu `IsPermissionOnly=true`
- **THEN** `AdminMenuRoleSeeder` SHALL 將 system role 對該 menu 的 CanRead/Create/Update/Delete 全設為 true

### Requirement: ShopOptions 權限節點
系統 SHALL 在 `MenuConstants` 中新增 `ShopOptions` 節點（前後端常數同步），並在 `AdminMenu` seeder 中種入對應 menu row（Id=23, ParentId=9, Name="選項管理", Endpoint="/shop/[id]/options", IsPermissionOnly=true, Sort=3）。

系統 SHALL 在所有店家啟用設定相關的 Admin Controller endpoint 上掛 `[RequireRole(MenuConstants.ShopOptions, CrudAction.X)]`。

#### Scenario: 後端 Constants 與 Menu seed
- **WHEN** Migrator 執行完成
- **THEN** `MenuConstants.ShopOptions` 對應的 AdminMenu row SHALL 存在於資料庫且 IsPermissionOnly=true，system role SHALL 自動具備全 CRUD

#### Scenario: 前端常數同步
- **WHEN** 前端 `@app/core/menuConstants.ts` 載入
- **THEN** SHALL 包含 `ShopOptions` 常數，值與後端 `MenuConstants.ShopOptions` MenuId 相同

#### Scenario: 無 ShopOptions 權限的角色
- **WHEN** 角色無 ShopOptions 任何 CRUD 權限
- **THEN** 後端 SHALL 回傳 403；前端 route middleware SHALL 在使用者直接輸入 `/shop/[shopId]/options` 時導回首頁

### Requirement: Hub 子 route 對應 menuId
前端 route middleware 的 route → menuId 對應表 SHALL 包含：
- `/shop/[id]/edit` → `MenuConstants.ShopList`
- `/shop/[id]/images` → `MenuConstants.ShopList`
- `/shop/[id]/overrides` → `MenuConstants.ShopOverride`
- `/shop/[id]/options` → `MenuConstants.ShopOptions`

對應的 `AdminMenu` row 中含 `[id]` placeholder 的 Endpoint SHALL 視為 dynamic route pattern：route middleware 在比對時 SHALL 將實際 path 中的數值 segment 與 placeholder 對齊。

#### Scenario: 動態 route 對應權限節點
- **WHEN** 管理員導向 `/shop/42/options`
- **THEN** route middleware SHALL 對應到 `ShopOptions` 並執行對應角色的權限檢查
