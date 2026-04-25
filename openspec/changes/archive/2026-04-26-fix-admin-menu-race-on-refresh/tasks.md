## 1. 新增 client plugin 載入 menu

- [x] 1.1 新增 `web/apps/admin/app/plugins/menu.client.ts`
- [x] 1.2 plugin 內注入 `useAuthStore` 與 `useMenuStore`
- [x] 1.3 當 `auth.isLoggedIn === true` 且 `menuStore.permissions.size === 0` 時 `await menuStore.fetchMenuData()`
- [x] 1.4 確認 plugin 為 `.client.ts` 命名（僅 client 端執行，避免 SSR self-signed cert 問題）

## 2. 移除 layout 內的 fetch 觸發

- [x] 2.1 修改 `web/apps/admin/app/layouts/default.vue`，移除 `if (import.meta.client && menuStore.menuData.length === 0) { menuStore.fetchMenuData() }` 區塊
- [x] 2.2 確認 layout 內仍有 `useMenuStore()` 注入（供 sidebar、loading 顯示等使用）

## 3. permission middleware 加上兜底

- [x] 3.1 修改 `web/apps/admin/app/middleware/permission.global.ts`，在判定 `findPermission` 前加入：若 `authStore.isLoggedIn` 為 true 且 `menuStore.permissions.size === 0`，則 `await menuStore.fetchMenuData()`
- [x] 3.2 確認 middleware 仍維持原本「找不到 pattern 就放行」、「找到 pattern 但無權限就重導 `/?forbidden=1`」的邏輯

## 4. 驗證

- [x] 4.1 啟動 admin（port 8081），登入後進入 `/shop/list`，按 Cmd+R 確認不再被重導至首頁
- [x] 4.2 對其他需要權限的頁面（`/admin-account/list`、`/drink-option/item/list`、`/order/list` 等）重複 4.1
- [x] 4.3 登出後重新登入，首次進入有權限頁面確認可正常載入
- [x] 4.4 直接輸入無權限的 URL（用權限受限的測試帳號），確認仍被正確重導至首頁並顯示無權限提示
- [x] 4.5 確認 sidebar menu 正常顯示、AppBreadcrumb 正常顯示
