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

點擊 Header 右側的使用者 avatar SHALL 展開下拉選單。**未登入時**選單 MUST 提供「登入」與「註冊」兩個入口；**已登入時** MUST 顯示 username header（使用者名稱，display only 不可點擊）+ 兩個動詞連結「偏好設定」（→ `/settings`）「我的收藏」（→ `/favorites`）+ Logout 按鈕（紅色）。Avatar 的圖片來源 MUST 使用 `https://api.dicebear.com/7.x/avataaars/svg?seed=${currentUser.email}`，未登入則使用預設 placeholder seed。Dropdown MUST NOT 包含指向 `/profile` 或 `/my-orders` 的連結（main nav 與 mobile bottom nav 已提供這兩個頁面的入口，避免重複）。

#### Scenario: 開啟 dropdown

- **WHEN** 使用者點擊 avatar
- **THEN** dropdown 以 fade transition 出現

#### Scenario: 點擊外部關閉 dropdown

- **WHEN** dropdown 已開啟且使用者點擊外部區域
- **THEN** dropdown 關閉（使用 `onClickOutside`）

#### Scenario: 已登入顯示真實使用者名稱

- **WHEN** dropdown 開啟且 `useAuthStore().isLoggedIn === true`
- **THEN** dropdown header 顯示 `currentUser.name`（display only，不是連結）

#### Scenario: 點擊偏好設定

- **WHEN** 已登入使用者點擊 dropdown 中的「偏好設定」
- **THEN** 導向 `/settings`，dropdown 關閉

#### Scenario: 點擊我的收藏

- **WHEN** 已登入使用者點擊 dropdown 中的「我的收藏」
- **THEN** 導向 `/favorites`，dropdown 關閉

#### Scenario: 點擊 Logout

- **WHEN** 已登入使用者點擊 dropdown 中的 Logout 按鈕
- **THEN** 觸發 `useAuthStore().logout()`、dropdown 關閉、導向 `/login`

#### Scenario: 未登入顯示登入入口

- **WHEN** dropdown 開啟且 `useAuthStore().isLoggedIn === false`
- **THEN** dropdown 顯示「登入」與「註冊」兩個 NuxtLink，分別導向 `/login`、`/register`，不顯示其他項目

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

Client SHALL 實作 `middleware/auth.global.ts` 守衛全站路由：未登入存取任何受保護頁面 MUST 導向 `/login`；已登入訪問 `/login` 或 `/register` MUST 導向 `/`。`/login`、`/register`、`/verify-email` 為 public 路徑，未登入仍可進入。

#### Scenario: 未登入存取受保護頁面

- **WHEN** 未登入使用者直接造訪 `/`、`/create`、`/profile`、`/my-orders`、`/group/[id]` 或 `/admin/[id]`
- **THEN** middleware 阻止導航並 `navigateTo('/login')`

#### Scenario: 已登入存取登入頁

- **WHEN** 已登入使用者造訪 `/login` 或 `/register`
- **THEN** middleware 阻止導航並 `navigateTo('/')`

#### Scenario: 公開頁面通行

- **WHEN** 未登入使用者造訪 `/login`、`/register` 或 `/verify-email?token=...`
- **THEN** middleware 不攔截，正常進入頁面

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
