## ADDED Requirements

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

個人頁面 SHALL 顯示：
1. **Profile Header**：大 avatar（mobile 40x40 / desktop 64x64）brutalist card + settings icon overlay。使用者名稱（5xl-9xl responsive）。部門 + 會員 badge。
2. **Stats Sidebar**（md:col-span-4）：兩張統計卡片（Orders Placed、Savings），帶裝飾性旋轉方塊 hover 動畫。桌面版 logout 按鈕。
3. **Account Menu**（md:col-span-8）：4 個選單項目（Wallet、Notifications、Security、Settings），各有彩色 icon、label、sublabel、chevron。Mobile 版 logout 按鈕在底部。

#### Scenario: 頁面 layout
- **WHEN** 頁面載入
- **THEN** 在桌面版顯示 12-column grid（4/8 split），mobile 為單欄

#### Scenario: 選單項目互動
- **WHEN** 使用者點擊 Account Menu 項目
- **THEN** 有 tap scale 效果（CSS active:scale-95）

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
