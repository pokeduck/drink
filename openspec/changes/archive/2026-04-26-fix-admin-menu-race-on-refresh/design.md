## Context

後台 admin 為 Nuxt 4 預設模式（**SSR + Client Hydration**，未關閉 SSR）。重整 `/shop/list` 時 middleware 與 plugin 會被執行**兩次**：先在 Node server，再在 browser（hydration）。這是本問題的核心。

初步診斷以為只是「layout 比 middleware 晚」，但更深的根因是：

```
重整 /shop/list
  │
  ├─ Phase 1: SERVER (Node.js)
  │    auth.global       → 讀 cookie (存在) → 放行
  │    permission.global → 查 ShopList 權限
  │                        ❌ permissions.size === 0 (server 端 store 是空的)
  │                        → navigateTo('/?forbidden=1') → 回 302
  │    ※ plugins/menu.client.ts 不會在 server 執行（.client 命名）
  │    ※ 即使呼叫 fetchMenuData 也會壞（後端是 self-signed HTTPS）
  │
  └─ Phase 2: CLIENT (browser)  ← 已被 server 踢走，根本到不了這裡
```

關鍵不對稱：SSR 端讀得到 cookie（隨 HTTP header 帶來）所以 `authStore.isLoggedIn === true`，但 Pinia store 是 per-request 新的、`menuStore.permissions` 在 server 端永遠是空的。**「已登入 + 無權限資料」這個矛盾狀態讓 server 自信地 redirect**。

Nuxt 中 `.global` middleware 預設**兩端都跑**，沒有像 plugin 那樣的 `.client.ts` 命名慣例可以限定執行端，必須用 `import.meta.server` / `import.meta.client` 在程式碼裡 guard。

## Goals / Non-Goals

**Goals:**
- 重新整理任何需要權限的頁面，menu 權限應在 `permission.global` middleware 執行前已載入完成
- 集中 menu fetch 觸發點，避免「layout 內 race + 後續還要記得別重複 fetch」這類分散邏輯
- 維持 SSR 友善：fetch 行為僅在 client 端執行，避免 Node.js 端碰到 self-signed cert 等 SSR 限制

**Non-Goals:**
- 不調整 `permission.global.ts` 的判定邏輯（pattern 對應、CRUD action 檢查維持原樣）
- 不改變 menu API（`/api/admin/menus/me`）合約
- 不處理 menu API 失敗時的降級策略（已存在的 try/catch 維持原狀；本次只解決 race 問題）
- 不改動登入/登出流程

## Decisions

### Decision 1: 用 client-only plugin 而非 await 在 middleware

**選擇**：新增 `plugins/menu.client.ts`，在 plugin 內 `await menuStore.fetchMenuData()`。

**為什麼**：
- Plugin 在 Nuxt 啟動時執行**一次**（client 端），比 route middleware 早
- Plugin 可 `async` 並 await，後續 middleware 看到的 `permissions` 已是完整資料
- middleware 每次切路由都會跑，若把 fetch 放在 middleware 內並 await，雖然有 size 檢查也安全，但語意上「fetch 是 app boot 時的一次性動作」放 plugin 更清晰

**替代方案**：
- **A: 在 `permission.global.ts` 內 await fetch**
  - 缺點：把 boot 期一次性的事與每次路由判定混在一起，未來容易誤觸（例如有人加了 `clearMenu()` 後路由切換時又 fetch 一次）
- **B: middleware 看到 permissions 為空時直接放行**
  - 缺點：失去「重整時直接攔截無權限」的保護，會閃一下無權限頁
- **C: 把 fetch 維持在 layout，但改成 `await` 並用 Suspense**
  - 缺點：layout 仍晚於 middleware，本問題不會解，且 Suspense 影響範圍大

### Decision 2: plugin 內檢查 `isLoggedIn` 與 `permissions.size`

```ts
if (auth.isLoggedIn && menuStore.permissions.size === 0) {
  await menuStore.fetchMenuData()
}
```

- `isLoggedIn` 為假時不 fetch，避免登入頁載入 plugin 時對未登入使用者打 menu API
- `permissions.size === 0` 是 idempotent guard，若 store 已有資料（例如未來加上 HMR 或熱重載情境）不重複 fetch

### Decision 3: 移除 `default.vue` 內的 fetch 觸發

避免雙處觸發：plugin 已負責，layout 不再判斷。同時降低未來維護心智負擔。

### Decision 4: `auth.global.ts` 不需要等 menu

`auth.global.ts` 只看 cookie 中的 token，與 menu 無關，無需改動。

### Decision 5: `permission.global.ts` 在 SSR 端整個跳過判定（關鍵）

```ts
export default defineNuxtRouteMiddleware(async (to) => {
  // 權限判定僅在 client 執行
  if (import.meta.server) return
  // ...原本的判定邏輯
})
```

**為什麼**：
- SSR 端 Pinia store 是 per-request 新建的，`menuStore.permissions` 永遠是空 Map
- Server 端不能 `fetchMenuData()`：Node.js 對 `https://localhost:5101` self-signed cert 會拒絕
- 即使能 fetch，也得處理 cookie 轉發、token refresh 等 SSR-only 細節，scope 過大
- Admin 後台是純內部工具，SSR 對它沒實際好處（沒 SEO 需求、無匿名訪問）

**結論**：權限判定本來就是 client 端 SPA 行為，讓 server 直接 render HTML（不做 redirect），把判定整個交給 client，搭配 `plugins/menu.client.ts` 預先 await menu，以及 middleware 自身的兜底，三層協作確保不會誤判。

**替代方案**：
- **A: 整個 app 關閉 SSR（`ssr: false`）**
  - 缺點：影響範圍過大；其他 `.global` middleware 也會失去 SSR 預檢的機會；屬於工程性配置變更，不該夾在 bug fix 裡
- **B: SSR 端做完整 menu fetch（含 cookie 轉發 + cert 處理）**
  - 缺點：scope 爆炸；要處理 self-signed cert、refresh token、SSR fetch 重試；對純內部工具沒收益
- **C: 維持 SSR 判定但讓 permissions 為空時放行**
  - 缺點：等同關閉 SSR 端的權限保護（無權限頁會先 render 再被 client redirect），閃一下、體感差

**重要原則記錄下來**：Nuxt `.global` middleware 兩端都跑。寫 middleware 時必問「我依賴的資料在 server 端拿得到嗎？」。拿不到（client-only store、localStorage、瀏覽器 API）就**必須 `if (import.meta.server) return`**，否則 server 會用「資料未就緒」的狀態做出錯誤決定。`.client.ts` 命名只能限定 plugin，不會限定 middleware。

## Risks / Trade-offs

- **Risk**: plugin 中 `await fetchMenuData()` 會延遲 client 端首次進站時間（約一個 API round-trip）
  - **Mitigation**: 影響僅在 cold start / 重整時發生一次；既有 layout 寫法本來也要 fetch，只是時機提前。整體 UX 無顯著退化，反而避免錯誤重導。

- **Risk**: 若 menu API 失敗（500/網路斷）導致 permissions 仍為空，使用者會被當成無權限重導
  - **Mitigation**: 本次不擴大 scope；既有 `fetchMenuData` 的 try/catch 會 console.error，後續若需「menu 載入失敗時的友善頁面」再開新 change

- **Risk**: 登入後 plugin 已執行過、不會再跑；登入流程要確保 fetch 一次
  - **Mitigation**: 現行登入流程登入成功後會 `navigateTo('/')`，重新進入 layout；目前 `default.vue` 的 fetch 移除後，登入後需要其他觸發點。**處理方式**：登入成功後在 `auth.store.login()` 結尾呼叫 `useMenuStore().fetchMenuData()`，或在 `permission.global.ts` 內保留「permissions 空 + isLoggedIn 為真就 await fetch」的兜底（採後者較簡單，且能涵蓋 plugin 後才登入的場景）。

  → 採**兜底方案**：`permission.global.ts` 在判定前加上「if isLoggedIn && permissions.size === 0 → await fetchMenuData()」。Plugin 已處理重整情境，middleware 兜底處理登入後首次導航情境。兩者不重複 fetch（size 檢查）。

## Migration Plan

純前端變更，無資料庫/API 異動：

1. 新增 plugin（client-only 預載 menu）
2. 修改 layout（移除 fetch）
3. 修改 permission middleware（加上 SSR 跳過 + client 端兜底 fetch）
4. 手動驗證：登入 → 進入任一需要權限的頁面 → Cmd+R → 預期不被重導
5. 手動驗證：登出 → 重新登入 → 不會看到「無權限」誤判

回滾策略：直接 revert 該 commit；不影響資料。

## 三層防護總結

修法不是單點 fix，而是三層協作：

| 層 | 何時生效 | 作用 |
|----|---------|------|
| `permission.global` 開頭 `if (server) return` | SSR 端每次重整 | 防止 server 用空 store 做錯誤 redirect |
| `plugins/menu.client.ts` (await) | Client hydration 一次性 | 在 client middleware 跑之前把 menu 預載好 |
| `permission.global` 內 `size === 0` 兜底 fetch | Client 端登入後首次導航 | plugin 已跑過、無法再觸發時的補強 |

任何一層獨立都不夠：少了第一層 server 會搶先 redirect、少了第二層重整時 client middleware 第一次跑就空、少了第三層登入後首次進入有權限頁可能被誤判。
