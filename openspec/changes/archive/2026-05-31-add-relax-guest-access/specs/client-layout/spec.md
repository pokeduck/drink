## MODIFIED Requirements

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
