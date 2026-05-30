# client-layout

## Purpose

定義 Client 前台全站 layout 契約：固定頂部 Header（含 Logo / 導航 / Dark mode toggle / Create CTA / User avatar dropdown）、Mobile bottom navigation、桌面 / 手機導航斷點切換、登入後個人選單與 logout 行為，作為所有 client 頁面共用的外殼。

## Requirements

### Requirement: Desktop Header

Layout SHALL 包含固定頂部 Header（h-16），內容包括：
- 左側：Logo "DRINK.UP"（font-display, bold italic），點擊導向首頁
- 中間：桌面版 nav links（Home、Create Group、My Orders、Profile）
- 右側：Dark mode toggle 按鈕、"+ Create" CTA 按鈕（桌面版限定）、使用者 avatar

#### Scenario: 當前頁面 nav 高亮
- **WHEN** 使用者在某頁面
- **THEN** 對應的 nav link 顯示 brand color + underline 樣式

#### Scenario: 點擊 Logo
- **WHEN** 使用者點擊 "DRINK.UP" logo
- **THEN** 導航到首頁（/）

### Requirement: User Avatar Dropdown

點擊 Header 右側的使用者 avatar SHALL 展開下拉選單。Dropdown 在已登入與未登入狀態下 MUST 共用相同結構（header + 偏好設定 / 我的收藏 + 末位按鈕），僅 header 文字與末位按鈕內容不同：

- **已登入**：header 為 `currentUser.name`（display only，不可點擊）；末位按鈕為「Logout」（紅色）；點擊 logout 後執行 `useAuthStore().logout()` + 導向 `/login`
- **未登入**：header 為「Guest」（display only）；末位按鈕為「登入」連結到 `/login`

中段的「偏好設定 → `/settings`」「我的收藏 → `/favorites`」 MUST 在兩種狀態下都顯示為 NuxtLink；guest 點擊時由 `auth.global` middleware 攔截並帶 `?redirect` 重導至 `/login`。

Avatar 圖片來源 MUST 使用 `https://api.dicebear.com/7.x/avataaars/svg?seed=${currentUser.email}`，未登入則使用預設 placeholder seed（如 `?seed=guest`）。

Dropdown MUST NOT 包含指向 `/profile`、`/my-orders` 的連結（main / bottom nav 已涵蓋），也 MUST NOT 包含「註冊」連結（仰賴 `/login` 頁底部的「建立新帳號」連結）。

#### Scenario: 開啟 dropdown

- **WHEN** 使用者點擊 avatar
- **THEN** dropdown 以 fade transition 出現

#### Scenario: 點擊外部關閉 dropdown

- **WHEN** dropdown 已開啟且使用者點擊外部區域
- **THEN** dropdown 關閉（使用 `onClickOutside`）

#### Scenario: 已登入顯示真實使用者名稱

- **WHEN** dropdown 開啟且 `useAuthStore().isLoggedIn === true`
- **THEN** dropdown header 顯示 `currentUser.name`

#### Scenario: 未登入顯示 Guest

- **WHEN** dropdown 開啟且 `useAuthStore().isLoggedIn === false`
- **THEN** dropdown header 顯示「Guest」

#### Scenario: 已登入點擊偏好設定

- **WHEN** 已登入使用者點擊 dropdown 中的「偏好設定」
- **THEN** 導向 `/settings`，dropdown 關閉

#### Scenario: 未登入點擊偏好設定 / 我的收藏 → 自動帶 redirect

- **WHEN** 未登入使用者點擊 dropdown 中的「偏好設定」或「我的收藏」
- **THEN** Vue Router 開始導航 → `auth.global` middleware 攔截 → `navigateTo('/login?redirect=%2Fsettings')`（或 `/login?redirect=%2Ffavorites`），dropdown 關閉

#### Scenario: 點擊 Logout

- **WHEN** 已登入使用者點擊 dropdown 末位 Logout 按鈕
- **THEN** 觸發 `useAuthStore().logout()`、dropdown 關閉、導向 `/login`

#### Scenario: 未登入點擊登入

- **WHEN** 未登入使用者點擊 dropdown 末位「登入」按鈕
- **THEN** 導向 `/login`，dropdown 關閉

### Requirement: Mobile Bottom Navigation

在 `md` 以下斷點 SHALL 顯示固定底部導航列（隱藏桌面版 nav），包含 4 個項目：
- Home（Home icon）
- Create（PlusCircle icon）
- Orders（List icon）
- Profile（User icon）

#### Scenario: 當前頁面 icon 高亮
- **WHEN** 使用者在某頁面
- **THEN** 對應的 mobile nav icon 放大 + 顯示 brand color + drop-shadow

#### Scenario: 桌面版隱藏
- **WHEN** 螢幕寬度 >= md 斷點
- **THEN** mobile bottom nav 隱藏

### Requirement: Desktop Footer Status Bar

在 `md` 以上斷點 SHALL 顯示固定底部黑色 status bar（h-10），顯示系統狀態、API 版本，以及 **`Current User` 區段**：已登入時顯示 `useAuthStore().currentUser.name`，未登入時顯示 `Guest`。純裝飾性質（不可互動）。

#### Scenario: Mobile 隱藏

- **WHEN** 螢幕寬度 < md 斷點
- **THEN** footer status bar 隱藏

#### Scenario: 已登入顯示真實使用者

- **WHEN** 桌面版且 `useAuthStore().isLoggedIn === true`
- **THEN** footer 顯示 `Current User: <currentUser.name>`

#### Scenario: 未登入顯示 Guest

- **WHEN** 桌面版且 `useAuthStore().isLoggedIn === false`
- **THEN** footer 顯示 `Current User: Guest`

### Requirement: 頁面轉場動畫

Layout SHALL 透過 Nuxt `pageTransition` 為所有頁面切換提供 fade 動畫。

#### Scenario: 頁面切換
- **WHEN** 使用者從一個頁面導航到另一個頁面
- **THEN** 舊頁面 fade-out，新頁面 fade-in

### Requirement: 全域認證守衛

Client SHALL 實作 `middleware/auth.global.ts` 守衛全站路由：

**Public 路徑**（未登入可訪問）：
- `publicPaths`：`['/', '/login', '/register', '/verify-email']`
- `publicPrefixes`：`['/group/']`（涵蓋所有 `/group/[id]` 動態路由）

**Auth-required 行為**：
- 未登入使用者訪問任何**非** public 的路徑 MUST 被導向 `/login?redirect=<encoded path>`，其中 `<encoded path>` 為 `encodeURIComponent(to.fullPath)`
- 已登入使用者訪問 `/login` 或 `/register` MUST 被導向 `/`

**安全約束**：
- `redirect` query value MUST 為相對路徑（`/` 開頭且 NOT 以 `//` 開頭），否則 `/login` 頁登入成功後 MUST fallback 到 `/`，避免 open redirect 攻擊

#### Scenario: Guest 訪問公開頁面通行

- **WHEN** 未登入使用者造訪 `/`、`/login`、`/register`、`/verify-email?token=...`、或 `/group/abc123` 之類符合 publicPrefixes 的路徑
- **THEN** middleware 不攔截，正常進入頁面

#### Scenario: Guest 訪問受保護頁面帶 redirect 重導

- **WHEN** 未登入使用者直接造訪 `/create`、`/profile`、`/my-orders`、`/settings`、`/favorites` 或 `/admin/[id]`
- **THEN** middleware MUST `navigateTo('/login?redirect=' + encodeURIComponent(to.fullPath))`，例如造訪 `/settings` 會被導去 `/login?redirect=%2Fsettings`

#### Scenario: 已登入使用者訪問 /login 或 /register

- **WHEN** 已登入使用者造訪 `/login` 或 `/register`
- **THEN** middleware MUST `navigateTo('/')`

#### Scenario: redirect 為絕對 URL fallback

- **WHEN** `/login` 頁讀到 `?redirect=https://evil.com` 或 `?redirect=//evil.com`
- **THEN** 登入成功後 MUST `navigateTo('/')`，不使用 `redirect` query 的值

### Requirement: Pinia 狀態管理

Client SHALL 安裝 `pinia` 與 `@pinia/nuxt`，並在 `nuxt.config.ts` 的 `modules` 註冊 `@pinia/nuxt`。所有跨頁面的 state（auth、profile）MUST 透過 `defineStore` 管理，不再使用 module-level singleton 或 `useState`。

#### Scenario: Pinia 模組已啟用

- **WHEN** Nuxt 啟動 client app
- **THEN** `@pinia/nuxt` module 已載入，Pinia 全域 instance 可用

#### Scenario: 跨元件存取 auth state

- **WHEN** 任何 component 呼叫 `useAuthStore()`
- **THEN** 取得同一份 store instance，state 在不同元件間同步

### Requirement: Auth Store 行為

Client SHALL 提供 `useAuthStore`（Pinia store），透過 `useCookie('auth_token')` 與 `useCookie('refresh_token')` 持久化 token，並暴露 `login`、`register`、`verifyEmail`、`refresh`、`logout`、`fetchProfile` actions 與 `currentUser`、`isLoggedIn` getters。`logout` 動作 MUST 同時呼叫後端撤銷 refresh_token 並清除本地 token。

#### Scenario: 登入成功

- **WHEN** `login({ email, password })` 後端回傳 `access_token` + `refresh_token`
- **THEN** Store 將兩個 token 寫入 cookie、設定 `isLoggedIn=true`、自動執行 `fetchProfile()` 取得 `currentUser`

#### Scenario: 登出

- **WHEN** 已登入使用者呼叫 `logout()`
- **THEN** Store 呼叫 `POST /api/user/auth/logout`、清除 cookie、`currentUser` 設為 null、`isLoggedIn=false`

#### Scenario: SPA reload 後保持登入

- **WHEN** 使用者重新整理頁面，cookie 中仍有有效 `auth_token`
- **THEN** Store 從 cookie 還原 token、`isLoggedIn=true`，並在第一次需要 user 資料時觸發 `fetchProfile()`

### Requirement: 401 自動 Refresh 與重試

Client SHALL 提供 `useUserApi` composable（封裝 `openapi-fetch` client），透過 middleware 自動為每個請求注入 `Authorization: Bearer <access_token>` header，並在收到 `401` 時 MUST 嘗試以 refresh_token 取新 access_token、用新 token 重試原始請求；refresh 失敗 MUST 清除 token 並導向 `/login`。並發 refresh 必須以單一 in-flight promise 防止重複呼叫。

#### Scenario: Access token 過期自動 refresh

- **WHEN** 已登入使用者呼叫 API 收到 `401` 且本地有 refresh_token
- **THEN** Client 呼叫 `/api/user/auth/refresh`、寫入新 token、用新 token 重試原始請求

#### Scenario: Refresh 失敗

- **WHEN** Refresh 端點回傳 `400` 或無 refresh_token
- **THEN** Client 呼叫 `clearTokens()` 並 `navigateTo('/login')`

#### Scenario: 並發 401 共用 refresh

- **WHEN** 使用者同時觸發多個 API 同時收到 `401`
- **THEN** 僅有一個 refresh 請求被送出，其他請求等待同一個 refresh promise，成功後皆以新 token 重試
