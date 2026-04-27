# admin-permission

## Purpose

定義 Admin 後台 menu / role 權限模型在前後端的契約：`/menus/me` 回傳每個葉節點的 CRUD 權限、前端以 `usePermission()` 與 route middleware 控制按鈕與路由級存取、後端 Controller 以 `[RequireRole]` 確保最終防線，並維持前後端 MenuConstants 一致。

## Requirements

### Requirement: Menu API 回傳 CRUD 權限
`/menus/me` API 回傳的每個葉節點 menu MUST 包含該使用者角色的 `can_read`、`can_create`、`can_update`、`can_delete` 權限。

#### Scenario: 角色有部分 CRUD 權限
- **WHEN** 使用者的角色對飲料品名 menu 設定為 `CanRead=true, CanCreate=false, CanUpdate=true, CanDelete=false`
- **THEN** `/menus/me` 回傳中該 menu 節點的 `can_read=true, can_create=false, can_update=true, can_delete=false`

#### Scenario: 系統管理員角色
- **WHEN** 使用者角色為系統角色（IsSystem=true）
- **THEN** 所有 menu 的 CRUD 權限 SHALL 全部為 true

### Requirement: 前端權限 composable
前端 MUST 提供 `usePermission()` composable，回傳 `can(menuId, action)` 方法，根據當前使用者的權限回傳 boolean。

#### Scenario: 查詢有權限的操作
- **WHEN** 呼叫 `can(MENU.DrinkItem, 'create')` 且使用者角色有飲料品名的 Create 權限
- **THEN** 回傳 `true`

#### Scenario: 查詢無權限的操作
- **WHEN** 呼叫 `can(MENU.DrinkItem, 'delete')` 且使用者角色無飲料品名的 Delete 權限
- **THEN** 回傳 `false`

#### Scenario: 權限尚未載入
- **WHEN** menu 資料尚未載入完成
- **THEN** `can()` SHALL 回傳 `false`（預設拒絕）

### Requirement: 按鈕級權限控制
所有管理頁面的操作按鈕 MUST 根據使用者權限控制顯示。無權限的按鈕 SHALL 不渲染。

#### Scenario: 無 Create 權限時隱藏新增按鈕
- **WHEN** 使用者角色對該 menu 無 Create 權限
- **THEN** 「新增」按鈕 SHALL 不顯示

#### Scenario: 無 Update 權限時隱藏編輯按鈕
- **WHEN** 使用者角色對該 menu 無 Update 權限
- **THEN** 「編輯」按鈕 SHALL 不顯示

#### Scenario: 無 Delete 權限時隱藏刪除按鈕
- **WHEN** 使用者角色對該 menu 無 Delete 權限
- **THEN** 「刪除」按鈕及「批次刪除」按鈕 SHALL 不顯示

### Requirement: 路由級權限控制
前端 MUST 透過 Nuxt route middleware 阻止使用者直接輸入 URL 進入無權限的頁面。Menu 權限資料 MUST 在 route middleware 執行權限判定前已載入完成，避免重新整理頁面時因權限未載入而誤判為無權限。

由於 menu store 僅能在 client 端載入（後端為 self-signed HTTPS、SSR 端 Pinia store 為 per-request 空狀態），permission middleware MUST 在 SSR 端跳過權限判定（直接 `return`），由 client 端完成所有權限檢查。

#### Scenario: 直接輸入無權限頁面 URL
- **WHEN** 使用者直接輸入 `/admin-account/create` 但無 AdminAccountList 的 Create 權限
- **THEN** SHALL 重導至首頁並顯示無權限提示

#### Scenario: 有權限的頁面正常進入
- **WHEN** 使用者直接輸入 `/drink-option/item` 且有 DrinkItem 的 Read 權限
- **THEN** SHALL 正常載入頁面

#### Scenario: 重新整理有權限的頁面
- **WHEN** 使用者已登入且在 `/shop/list` 頁面按下重新整理（Cmd+R 或 F5），其角色具有 ShopList 的 Read 權限
- **THEN** SHALL 在 permission middleware 執行前完成 menu 權限載入，使用者 SHALL 留在 `/shop/list` 而非被誤判為無權限重導至首頁

#### Scenario: 登入後首次導航
- **WHEN** 使用者剛登入完成、即將首次進入需要權限的頁面
- **THEN** permission middleware SHALL 在判定權限前確保 menu 資料已載入（若尚未載入則 await 載入完成），確保有權限的頁面能正常進入

#### Scenario: SSR 端執行 middleware
- **WHEN** Nuxt 在 server 端為已登入使用者重整需要權限的頁面執行 permission middleware
- **THEN** middleware SHALL 在 SSR 環境（`import.meta.server === true`）下直接 return 放行，不進行 redirect，避免使用 server 端尚未載入的 menu 權限做出錯誤判定

### Requirement: 後端 Controller 權限 attribute
所有 Admin.API Controller 的 endpoint MUST 掛上 `[RequireRole(menuId, action)]` attribute，確保後端也有權限檢查。

#### Scenario: 無 Create 權限呼叫 POST
- **WHEN** 使用者角色無該 menu 的 Create 權限，直接 POST 呼叫建立 API
- **THEN** 後端 SHALL 回傳 403 Forbidden

#### Scenario: 有 Read 權限呼叫 GET
- **WHEN** 使用者角色有該 menu 的 Read 權限，呼叫 GET 列表 API
- **THEN** 後端 SHALL 正常回傳資料

### Requirement: 前後端 MenuConstants 一致
前端 `@app/core` 的 menuConstants 與後端 `MenuConstants.cs` 的 MenuId 值 MUST 完全一致。

#### Scenario: 新增 menu 後雙邊同步
- **WHEN** 後端新增一個 menu 並定義 MenuConstants
- **THEN** 前端 `@app/core/menuConstants.ts` MUST 同步新增對應常數

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
