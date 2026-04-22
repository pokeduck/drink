## Why

後端已有完整的 CRUD 權限系統（AdminMenuRole），但前端完全沒有使用。任何登入的管理員都能看到所有按鈕、進入所有頁面，只靠後端 403 擋住。需要在前端實作權限控制，讓 UI 反映實際權限：隱藏無權限的按鈕、阻止無權限的頁面存取。同時後端 Controller 也需要掛上 `[RequireRole]` attribute，確保前後端雙重保護。

## What Changes

- `/menus/me` API 回傳擴充：每個 menu 節點帶上 `can_read` / `can_create` / `can_update` / `can_delete` 權限
- 新增前端 `usePermission()` composable，提供 `can(menuId, action)` 方法
- 前端 menu store 儲存權限資訊
- 所有頁面的「新增」「編輯」「刪除」按鈕根據權限 `v-if` 控制
- Nuxt route middleware 擋住直接 URL 進入無權限頁面
- 後端所有 Controller endpoint 掛上 `[RequireRole(menuId, action)]` attribute

## Capabilities

### New Capabilities
- `admin-permission`: 前後端完整 CRUD 權限控制（API 權限回傳、前端 composable、按鈕/路由控制、Controller attribute）

### Modified Capabilities

（無）

## Impact

- **後端 API**：`/menus/me` response 增加 CRUD 權限欄位
- **後端 Controller**：所有 Admin.API Controller 方法加上 `[RequireRole]`
- **前端 store**：menu store 擴充儲存權限 map
- **前端 composable**：新增 `usePermission()`
- **前端頁面**：所有列表/表單頁面的操作按鈕加 `v-if` 權限判斷
- **前端 middleware**：新增 permission route middleware
