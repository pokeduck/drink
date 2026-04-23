## ADDED Requirements

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

點擊 Header 右側的使用者 avatar SHALL 展開下拉選單，包含：
- "Signed in as" 標題 + 使用者名稱
- 選單項目：Profile、My Orders、Settings
- Logout 按鈕（紅色）

#### Scenario: 開啟 dropdown
- **WHEN** 使用者點擊 avatar
- **THEN** dropdown 以 fade transition 出現

#### Scenario: 點擊外部關閉 dropdown
- **WHEN** dropdown 已開啟且使用者點擊外部區域
- **THEN** dropdown 關閉（使用 `onClickOutside`）

#### Scenario: 點擊選單項目
- **WHEN** 使用者點擊 dropdown 中的 Profile / My Orders
- **THEN** 導航到對應頁面，dropdown 關閉

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

在 `md` 以上斷點 SHALL 顯示固定底部黑色 status bar（h-10），顯示系統狀態、使用者資訊、API 版本。純裝飾性質。

#### Scenario: Mobile 隱藏
- **WHEN** 螢幕寬度 < md 斷點
- **THEN** footer status bar 隱藏

### Requirement: 頁面轉場動畫

Layout SHALL 透過 Nuxt `pageTransition` 為所有頁面切換提供 fade 動畫。

#### Scenario: 頁面切換
- **WHEN** 使用者從一個頁面導航到另一個頁面
- **THEN** 舊頁面 fade-out，新頁面 fade-in
