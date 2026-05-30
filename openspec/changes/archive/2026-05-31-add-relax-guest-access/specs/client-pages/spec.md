## MODIFIED Requirements

### Requirement: Group Detail 頁面 — 揪團詳情與點餐

揪團詳情頁 SHALL 為 **public 路徑**（由 `auth.global` middleware 的 `publicPrefixes: ['/group/']` 涵蓋），未登入訪客可瀏覽。頁面顯示：

1. **Header**：返回按鈕 + 店家 logo + 店名（大標題）+ 描述
2. **Host Controls**（雙重條件顯示）：當 `isHost === true` **且** `useAuthStore().isLoggedIn === true` 才顯示 "Management Panel" 按鈕連結到 `/admin/{id}`；guest（即使 mock data 計算出 `isHost === true`）也 MUST NOT 看到此區
3. **Menu Grid**：1/2/3 column grid，每項顯示飲料名稱、分類、價格（brand color）、"ORDER >" 文字。Menu 對 guest 與已登入使用者皆可瀏覽
4. **Order Modal**：點擊 menu item 開啟。Modal 內容（飲料名 / 價格 / size / sugar 選擇器）對 guest 與已登入使用者皆可瀏覽
5. **Place Order CTA**（Modal 內主按鈕）：依登入狀態切換 affordance：
   - **已登入**：按鈕標籤「ADD TO ORDER」（或同義詞），點擊執行 `handlePlaceOrder` 邏輯
   - **未登入**：按鈕替換為 `<NuxtLink>` 標籤「登入後下單」，`to` 為 `/login?redirect=` + `encodeURIComponent('/group/' + id)`，點擊直接 navigation 到登入頁；登入完成後使用者會被自動導回此揪團頁

#### Scenario: Guest 訪問揪團詳情可瀏覽

- **WHEN** 未登入使用者直接打開 `/group/abc123` URL
- **THEN** 頁面正常渲染，顯示 header / menu grid，**MUST NOT** 顯示 Host Controls 區

#### Scenario: 點擊飲料項目開啟點餐 Modal

- **WHEN** 任何使用者（含 guest）點擊 menu 中的飲料
- **THEN** 開啟點餐 Modal（mobile bottom sheet / desktop 置中），顯示飲料名 + 價格、Size / Sugar 選擇器

#### Scenario: 已登入送出訂單

- **WHEN** 已登入使用者點擊 Modal 內「ADD TO ORDER」
- **THEN** 按鈕變為 "PROCESSING..."（disabled），2 秒後導航到 `/my-orders`

#### Scenario: Guest 看到登入提示按鈕

- **WHEN** 未登入使用者開啟 Order Modal
- **THEN** Modal 內主按鈕顯示為「登入後下單」，且為 NuxtLink 連結到 `/login?redirect=%2Fgroup%2F<id>`

#### Scenario: Guest 點擊登入後下單

- **WHEN** 未登入使用者點擊 Modal 內「登入後下單」按鈕
- **THEN** 導航到 `/login?redirect=%2Fgroup%2F<id>`，登入成功後 `/login` 頁讀取 `redirect` query 並導回 `/group/<id>`

#### Scenario: Guest 模式下不顯示 Host Controls

- **WHEN** 未登入使用者訪問任意 `/group/[id]`
- **THEN** Host Controls 區塊 MUST 不渲染，與該使用者是否「在 mock data 中被認定為 host」無關
