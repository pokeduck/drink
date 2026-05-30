## 1. Middleware — publicPaths / publicPrefixes / redirect query

- [x] 1.1 修改 `web/apps/client/app/middleware/auth.global.ts`：
  - `publicPaths` 加入 `'/'`
  - 新增 `publicPrefixes = ['/group/']`，使用 `Array.some + startsWith` 判斷
  - 受保護頁面 + 未登入 → `navigateTo('/login?redirect=' + encodeURIComponent(to.fullPath))`
  - 已登入 + `/login` 或 `/register` → `navigateTo('/')`（行為不變）
- [x] 1.2 加入 redirect 安全驗證 helper：`isSafeRedirect(value: string): boolean` — 回傳 `value.startsWith('/') && !value.startsWith('//')`（middleware 內可備用，或統一交給 `/login` 頁驗證）

## 2. /login 頁讀取 redirect

- [x] 2.1 修改 `web/apps/client/app/pages/login.vue`：
  - import `useRoute`
  - `handleSubmit` 成功後改為 `await navigateTo(safeRedirect ?? '/')`，其中 `safeRedirect = isSafeRedirect(route.query.redirect) ? route.query.redirect : null`
  - 維持「沒有帳號？建立新帳號」連結到 `/register`（已存在）

## 3. Avatar Dropdown 重構（Guest 結構對齊 logged-in）

- [x] 3.1 修改 `web/apps/client/app/layouts/default.vue` 的 dropdown 區塊：
  - 已登入版本：維持現狀（username header / 偏好設定 / 我的收藏 / Logout）
  - 未登入版本改為：「Guest」header / 偏好設定 (NuxtLink → /settings) / 我的收藏 (NuxtLink → /favorites) / 登入 (NuxtLink → /login)
  - 移除原本的「註冊」連結
- [x] 3.2 確認 `currentUserName.value` 為 `Guest` 時 dropdown 顯示一致；avatar dicebear seed fallback 為 `'guest'`（已存在）
- [x] 3.3 確認 dropdownItems 陣列在兩種狀態下都被使用（將「偏好設定 / 我的收藏」項目從 `v-if="authStore.isLoggedIn"` 區塊提到外面）

## 4. /group/[id] guest gating

- [x] 4.1 修改 `web/apps/client/app/pages/group/[id].vue`：
  - import `useAuthStore`
  - Host Controls 區塊條件改為 `v-if="isHost && authStore.isLoggedIn"`
- [x] 4.2 Order Modal 內 Place Order 按鈕替換為條件式 affordance：
  - `v-if="authStore.isLoggedIn"`：原 `<button @click="handlePlaceOrder">...</button>`
  - `v-else`：`<NuxtLink :to="\`/login?redirect=${encodeURIComponent('/group/' + route.params.id)}\`" class="brutalist-button brutalist-button-primary py-4 text-lg flex-1">登入後下單</NuxtLink>`

## 5. Type check & SPA 測試

- [x] 5.1 `cd web/apps/client && pnpm exec nuxt prepare` 成功
- [x] 5.2 `pnpm exec vue-tsc --noEmit -p .nuxt/tsconfig.json` 對 middleware / login.vue / default.vue / group/[id].vue 零錯誤
- [x] 5.3 SPA 啟動瀏覽器手測：
  - 無 cookie 訪問 `/` → 看到 Active Groups（不被導 login）
  - 無 cookie 訪問 `/group/abc` → 看到 menu，Host Controls 不顯示，Place Order 按鈕為「登入後下單」
  - 無 cookie 訪問 `/settings` → 自動到 `/login?redirect=%2Fsettings`，登入完成後回到 `/settings`
  - 無 cookie 訪問 `/login?redirect=https://evil.com` → 登入完成後仍導到 `/`
  - 已登入點 dropdown → username + 偏好設定 / 我的收藏 / 登出
  - 未登入點 dropdown → Guest + 偏好設定 / 我的收藏 / 登入
  - 未登入點 dropdown 內「偏好設定」 → `/login?redirect=%2Fsettings`

## 6. Spec validate

- [x] 6.1 `openspec validate add-relax-guest-access --strict` 通過
