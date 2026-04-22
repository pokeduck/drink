## ADDED Requirements

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
前端 MUST 透過 Nuxt route middleware 阻止使用者直接輸入 URL 進入無權限的頁面。

#### Scenario: 直接輸入無權限頁面 URL
- **WHEN** 使用者直接輸入 `/admin-account/create` 但無 AdminAccountList 的 Create 權限
- **THEN** SHALL 重導至首頁並顯示無權限提示

#### Scenario: 有權限的頁面正常進入
- **WHEN** 使用者直接輸入 `/drink-option/item` 且有 DrinkItem 的 Read 權限
- **THEN** SHALL 正常載入頁面

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
