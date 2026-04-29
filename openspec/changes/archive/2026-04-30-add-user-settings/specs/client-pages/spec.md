## ADDED Requirements

### Requirement: Settings 頁面 — 偏好設定

`/settings` SHALL 為唯一的可編輯偏好設定頁，包含三個區塊（Profile / Account / Preferences）。所有 client 端的編輯動作 MUST 集中於此，`/profile` 頁僅作展示用途。頁面 MUST 受 `auth.global` middleware 守護（未登入導 `/login`）。

#### Scenario: Profile section — 編輯顯示名稱

- **WHEN** 使用者於 Settings 內輸入新的顯示名稱並按「儲存」
- **THEN** Client 呼叫 `PUT /api/user/profile` 更新 `name`，成功後 `useAuthStore().currentUser` 同步更新

#### Scenario: Profile section — 拖拉上傳頭像

- **WHEN** 使用者拖拉 / 點擊選擇一張本地圖片
- **THEN** Client 即時 POST 至 `/api/user/upload/avatar` 取得壓縮後（max 長邊 512px）的相對路徑，**僅顯示預覽**，`User.Avatar` 尚未變更

#### Scenario: Profile section — 確認儲存頭像

- **WHEN** 預覽中按下「儲存」
- **THEN** Client 呼叫 `PUT /api/user/profile` 寫入 `avatar`；按「取消」則丟棄預覽 URL（檔案保留在 Upload.API 由 SHA-256 dedup 控制）

#### Scenario: Account section — Email 唯讀

- **WHEN** 使用者進入 Settings
- **THEN** Email 欄位 MUST 以 read-only 形式顯示（`disabled` input 或純文字），不可編輯

#### Scenario: Account section — 變更密碼成功

- **WHEN** 使用者填入正確舊密碼與合法新密碼，按「儲存」
- **THEN** Client 呼叫 `PUT /api/user/auth/password`，成功後 MUST 主動 `useAuthStore().clearTokens()` 並導向 `/login`，提示使用者用新密碼重新登入

#### Scenario: Account section — 變更密碼舊密碼錯

- **WHEN** 後端回傳 `400` + `INVALID_PASSWORD`
- **THEN** UI MUST 在 `old_password` 欄位旁顯示「舊密碼錯誤」，其他欄位保留輸入

#### Scenario: Account section — 登出所有裝置

- **WHEN** 使用者點擊「登出所有裝置」
- **THEN** Client 呼叫 `POST /api/user/auth/logout-all`，成功後 MUST `useAuthStore().clearTokens()` 並導向 `/login`

#### Scenario: Account section — Google 連結（Disabled）

- **WHEN** 使用者進入 Settings 且 `currentUser.is_google_connected === false`
- **THEN** UI 顯示「Google 連結」按鈕但 `disabled`，旁邊標示「即將推出」（Google SSO 是未來 change，本次不接通）

#### Scenario: Preferences section — 主題切換

- **WHEN** 使用者選擇 Light / Dark / System
- **THEN** Client 透過 `useColorMode().preference` 立即切換，並透過 cookie 持久化（@nuxtjs/color-mode 預設行為）

#### Scenario: Preferences section — 通知 channel

- **WHEN** 使用者切換 Email / WebPush 兩個 toggle
- **THEN** Client 呼叫 `PUT /api/user/profile` 更新 `notification_type`（None=0 / WebPush=1 / Email=2 / Both=3）

### Requirement: Favorites 頁面 — 我的收藏（Mockup）

`/favorites` SHALL 為使用者的收藏清單頁，分為「店家」「飲料」兩類。Mockup 階段顯示固定假資料；移除按鈕點擊時 MUST 顯示 toast「Mockup 階段尚未支援」並 NOT 修改任何資料。Mobile 版採 tab 切換，Desktop 版採並列 two-section。頁面底部 MUST 標示「真實功能將於店家瀏覽功能上線後啟用」。

#### Scenario: Mobile 顯示 tab

- **WHEN** 螢幕寬度 < md 斷點且使用者進入 `/favorites`
- **THEN** 上方顯示兩個 tab「店家 (5)」「飲料 (3)」，預設選中「店家」，點擊切換各自清單

#### Scenario: Desktop 顯示 two-section

- **WHEN** 螢幕寬度 ≥ md 斷點
- **THEN** 同時顯示「店家」與「飲料」兩個 section，並列或上下排列

#### Scenario: 假移除按鈕

- **WHEN** 使用者點擊任一項目的「移除」按鈕
- **THEN** UI 顯示 toast「Mockup 階段尚未支援」，列表 MUST NOT 變動

#### Scenario: 入口可達性

- **WHEN** 使用者位於任意頁
- **THEN** 必能透過 (a) Avatar dropdown「我的收藏」或 (b) `/profile` Favorites 區塊的「編輯」按鈕進入 `/favorites`

## MODIFIED Requirements

### Requirement: Profile 頁面 — 個人頁面

`/profile` SHALL 為唯讀展示頁（**不再提供任何編輯動作**），分為三個區塊：

1. **Identity Header**：大 avatar（mobile 40x40 / desktop 64x64）brutalist card；右側 ⚙ 齒輪 icon 連結到 `/settings`；使用者名稱（4xl-7xl responsive）；Email；加入日期（從 `currentUser.created_at` 格式化為「Joined Apr 2026」）；Email Verified badge；Google 連結 badge。
2. **Stats Mockup 區**：4 張 brutalist card 顯示 mockup 數據：本月花費（NT$ 480）、累計揪團次數（12）、最常去店家（五十嵐）、上次點的飲料（蜂蜜檸檬綠 半糖少冰）。卡片角落 MUST 標示「Mockup data — coming soon」字樣，避免使用者誤以為是真實資料。
3. **My Favorites 區**：橫向捲動兩列預覽（Shops × N、Drinks × M，皆為 mockup），標題右側「編輯」按鈕導向 `/favorites`。

頁面 MUST NOT 含有任何指向 Wallet / Notifications / Security / Settings 的假 menu icon 卡片（與舊版差異）。Logout 按鈕 MUST NOT 出現在 `/profile`（已移至 avatar dropdown 與 `/settings`）。

#### Scenario: 頁面 layout

- **WHEN** 頁面載入
- **THEN** 在桌面版顯示 12-column grid 或 single column，三個區塊由上至下排列

#### Scenario: 顯示真實使用者資訊

- **WHEN** 已登入使用者進入 `/profile`
- **THEN** Identity Header 內容（avatar / name / email / joined date / verified badge / google badge）皆來自 `useAuthStore().currentUser`

#### Scenario: 點擊齒輪 icon

- **WHEN** 使用者點擊 Identity Header 內的 ⚙ icon
- **THEN** 導向 `/settings`

#### Scenario: 點擊 Favorites 區的「編輯」

- **WHEN** 使用者點擊 My Favorites 區標題旁的「編輯」按鈕
- **THEN** 導向 `/favorites`

#### Scenario: Mockup 標示

- **WHEN** Stats / Favorites 區塊顯示
- **THEN** 區塊內 MUST 有「Mockup data — coming soon」或同等字樣，明確告知使用者此區資料為預告性質
