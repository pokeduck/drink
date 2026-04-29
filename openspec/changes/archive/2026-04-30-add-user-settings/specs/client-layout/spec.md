## MODIFIED Requirements

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
