## 1. 後端：MenuTreeResponse 擴充 + API 修改

- [x] 1.1 `MenuTreeResponse` 加上 `CanRead`、`CanCreate`、`CanUpdate`、`CanDelete` 欄位
- [x] 1.2 `AdminMenuService.GetMyMenus()` 查詢 `AdminMenuRole` 並填入權限到 response
- [x] 1.3 系統角色（IsSystem）自動給全部 CRUD 為 true
- [x] 1.4 更新 Swagger codegen（`pnpm run gen:api`）確認前端型別更新

## 2. 後端：Controller 掛 RequireRole attribute

- [x] 2.1 `UsersController` 所有 endpoint 掛上 `[RequireRole]`
- [x] 2.2 `RolesController` 所有 endpoint 掛上 `[RequireRole]`
- [x] 2.3 `MembersController` 所有 endpoint 掛上 `[RequireRole]`
- [x] 2.4 `DrinkItemsController` 所有 endpoint 掛上 `[RequireRole]`
- [x] 2.5 `SugarsController` 所有 endpoint 掛上 `[RequireRole]`
- [x] 2.6 `IcesController` 所有 endpoint 掛上 `[RequireRole]`
- [x] 2.7 `ToppingsController` 所有 endpoint 掛上 `[RequireRole]`
- [x] 2.8 `SizesController` 所有 endpoint 掛上 `[RequireRole]`
- [x] 2.9 `VerificationsController` 所有 endpoint 掛上 `[RequireRole]`

## 3. 前端：權限基礎建設

- [x] 3.1 `@app/core` 新增 `menuConstants.ts`，與後端 `MenuConstants.cs` 對應
- [x] 3.2 menu store 擴充：從 menu tree 提取 permissions map
- [x] 3.3 新增 `usePermission()` composable（`can(menuId, action)` 方法）
- [x] 3.4 新增 `permission.global.ts` route middleware（路由→menuId 對應表 + 權限檢查 + 無權限重導）

## 4. 前端：頁面按鈕權限控制

- [x] 4.1 `admin-account/list.vue`：新增/編輯/刪除/重設密碼按鈕加 `v-if`
- [x] 4.2 `admin-account/role/index.vue`：新增/編輯/刪除按鈕加 `v-if`
- [x] 4.3 `drink-option/item.vue`：新增/編輯/刪除/批次刪除按鈕加 `v-if`
- [x] 4.4 `drink-option/sugar.vue`：同上
- [x] 4.5 `drink-option/ice.vue`：同上
- [x] 4.6 `drink-option/topping.vue`：同上
- [x] 4.7 `drink-option/size.vue`：同上
- [x] 4.8 `member/list.vue`：新增/重設密碼按鈕加 `v-if`
- [x] 4.9 `member/verification/register.vue`：重發按鈕加 `v-if`

## 5. 驗證

- [x] 5.1 建立一個測試用角色，設定部分 CRUD 權限
- [x] 5.2 登入測試帳號，確認按鈕依權限顯示/隱藏
- [x] 5.3 直接輸入無權限頁面 URL，確認被重導
- [x] 5.4 用 API 直接呼叫無權限 endpoint，確認回傳 403
