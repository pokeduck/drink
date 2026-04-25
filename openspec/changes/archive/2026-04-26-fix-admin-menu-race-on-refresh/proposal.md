## Why

後台頁面在重新整理（Cmd+R）時，`permission.global` middleware 會在 menu 權限尚未載入前就判定路由權限，導致使用者被誤判為無權限並重導至首頁顯示「您沒有權限存取該頁面」。原因是 `fetchMenuData()` 目前寫在 `default.vue` layout 的 setup 內，layout 的執行時機比 route middleware 晚，因此重整時 `menuStore.permissions` 為空 Map，所有需要權限的頁面（如 `/shop/list`）都會被攔截。

## What Changes

- 新增 client-only Nuxt plugin `plugins/menu.client.ts`，在 middleware 執行前 `await menuStore.fetchMenuData()`（僅當已登入且 permissions 為空時）
- 移除 `layouts/default.vue` 中觸發 `fetchMenuData()` 的程式碼，集中在 plugin 處理
- `permission.global.ts` 行為不變，但因 plugin 已先載入 menu，重整時不會再有 race condition

## Capabilities

### New Capabilities
（無）

### Modified Capabilities
- `admin-permission`: 修改「路由級權限控制」需求，補上「重新整理頁面時 menu 權限必須先載入完畢才執行 middleware 判定」的行為要求

## Impact

- 影響檔案：
  - 新增 `web/apps/admin/app/plugins/menu.client.ts`
  - 修改 `web/apps/admin/app/layouts/default.vue`（移除 fetchMenuData 觸發點）
- 不影響後端 API 行為
- 不影響登入/登出流程（plugin 內檢查 `isLoggedIn` 才 fetch）
- 不影響 SSR（plugin 為 `.client.ts`，僅 client 端執行）
