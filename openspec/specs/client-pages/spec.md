# client-pages

## Purpose

定義 Client 前台主要頁面契約：Home 揪團列表（Active / History 雙區塊）、Create Group 建立揪團、Group Detail 揪團詳情與點餐、My Orders 個人訂單、Profile 個人頁面、Admin Panel 團主管理，以及共用的 GroupCard 元件視覺與互動規範。

## Requirements

### Requirement: Home 頁面 — 活動揪團列表

首頁 SHALL 顯示兩個區塊：
1. **Active Groups**：標題 + 數量 badge，grid 排列（1/2/3 columns responsive），每筆為 `GroupCard` 元件。空狀態顯示 dashed border + "NO ACTIVE BUYS" 訊息。
2. **History**：僅在有歷史資料時顯示，grid 排列（1/2/4 columns），套用 opacity-60 + grayscale 效果。

#### Scenario: 有活動揪團
- **WHEN** 頁面載入且有 OPEN/ORDERED/ARRIVING/READY 狀態的揪團
- **THEN** 在 Active Groups 區塊以 grid 顯示 GroupCard，帶入場動畫

#### Scenario: 無活動揪團
- **WHEN** 頁面載入且無活動揪團
- **THEN** 顯示 dashed border 空狀態區塊 "NO ACTIVE BUYS"

#### Scenario: 有歷史揪團
- **WHEN** 有 COMPLETED/CANCELLED 狀態的揪團
- **THEN** 在 History 區塊顯示，套用灰階 + 半透明效果

### Requirement: GroupCard 元件

GroupCard SHALL 為可點擊的揪團卡片，連結到 `/group/{id}`，顯示：
- 右上角狀態 badge（依狀態顯示不同顏色）
- 店家 logo（14x14 方形 + border）+ 店名 + 截止時間
- 描述文字（italic, muted）
- Footer：成員數 + "Join Now" CTA + 箭頭 icon

#### Scenario: 狀態顏色對應
- **WHEN** 揪團狀態為 OPEN
- **THEN** badge 為 green-400
- **WHEN** 揪團狀態為 ORDERED
- **THEN** badge 為 blue-400
- **WHEN** 揪團狀態為 READY
- **THEN** badge 為 orange-400
- **WHEN** 揪團狀態為 COMPLETED
- **THEN** badge 為 slate-300 + opacity-60

#### Scenario: 點擊卡片
- **WHEN** 使用者點擊 GroupCard
- **THEN** 導航到 `/group/{group.id}`

### Requirement: Create Group 頁面 — 建立揪團

建立揪團頁 SHALL 包含 3 步驟表單：
1. **選擇店家**：點擊開啟 Modal（mobile 全螢幕 / desktop 置中 700px），支援搜尋過濾，選中後顯示店家卡片 + "Change Shop" 連結
2. **截止時間**：`<input type="time">`，brutalist shadow 樣式
3. **備註**：`<textarea>` 描述欄位
4. **送出按鈕**："Launch Group" + Send icon

步驟 2、3 在未選擇店家前 SHALL 為 disabled 狀態（opacity-30 + pointer-events-none）。

#### Scenario: 店家選擇 Modal 搜尋
- **WHEN** 使用者在 Modal 搜尋欄輸入文字
- **THEN** 即時過濾店家列表

#### Scenario: 選擇店家
- **WHEN** 使用者點擊某店家
- **THEN** 該店家高亮（brand border + checkmark），Modal 可關閉，步驟 2/3 解鎖

#### Scenario: 搜尋無結果
- **WHEN** 搜尋文字無匹配店家
- **THEN** 顯示空狀態訊息

### Requirement: Group Detail 頁面 — 揪團詳情與點餐

揪團詳情頁 SHALL 顯示：
1. **Header**：返回按鈕 + 店家 logo + 店名（大標題）+ 描述
2. **Host Controls**（條件顯示）：若當前用戶為團主，顯示 "Management Panel" 按鈕連結到 `/admin/{id}`
3. **Menu Grid**：1/2/3 column grid，每項顯示飲料名稱、分類、價格（brand color）、"ORDER >" 文字

#### Scenario: 點擊飲料項目開啟點餐 Modal
- **WHEN** 使用者點擊 menu 中的飲料
- **THEN** 開啟點餐 Modal（mobile bottom sheet / desktop 置中）

#### Scenario: 點餐 Modal 內容
- **WHEN** Modal 開啟
- **THEN** 顯示飲料名 + 價格、Size 選擇器（2 column: Large/Medium）、Sugar 選擇器（4-5 column: 100%/75%/50%/25%/0%）、"ADD TO ORDER" 按鈕

#### Scenario: 送出訂單
- **WHEN** 使用者點擊 "ADD TO ORDER"
- **THEN** 按鈕變為 "PROCESSING..."（disabled），2 秒後導航到 `/my-orders`

#### Scenario: 團主看到管理入口
- **WHEN** 當前用戶為該揪團的 hostId
- **THEN** 顯示 Host Controls 區塊

### Requirement: My Orders 頁面 — 我的訂單

我的訂單頁 SHALL 顯示當前用戶的所有訂單，每筆卡片包含：
- 店家 logo + 名稱 + 日期
- 狀態 badge："READY"（brand bg）或 "WAITING"（muted）
- 飲料 icon + 名稱 + 規格 + 價格
- 付款狀態：綠點（已付）或紅點 + pulse 動畫（未付）+ "PAY NOW" 按鈕

#### Scenario: 無訂單
- **WHEN** 使用者無任何訂單
- **THEN** 顯示 ShoppingBag icon + "NO ORDERS YET" 空狀態

#### Scenario: 未付款訂單
- **WHEN** 訂單 paid 為 false
- **THEN** 顯示紅色 pulse 點 + "PAY NOW" 按鈕

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

### Requirement: Admin Panel 頁面 — 團主管理

團主管理頁 SHALL 顯示：
1. **返回按鈕 + "Management" 標題**
2. **Progress Tracker**：3 步驟水平 stepper（Collecting → Ordered → Ready），每步為可點擊按鈕，active 步驟有 brand bg + shadow + scale-up，步驟間有進度線動畫
3. **Financial Summary**：雙邊框區塊顯示 Total vs Paid 金額
4. **Order List**：1/2/3 column grid，每張卡片顯示使用者 avatar、名稱、飲料詳情、價格、已付/未付 toggle checkbox（綠色為已付）
5. **Broadcast Status 按鈕**：全寬 primary 按鈕

#### Scenario: 切換訂單狀態
- **WHEN** 團主點擊 Progress Tracker 中的步驟
- **THEN** 揪團狀態更新，active 步驟樣式變化，進度線動畫

#### Scenario: Toggle 付款狀態
- **WHEN** 團主點擊某訂單的 paid checkbox
- **THEN** 該訂單 paid 狀態切換，checkbox 顏色更新
