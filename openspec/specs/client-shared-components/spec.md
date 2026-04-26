# client-shared-components

## Purpose

定義 Client 前台跨頁面共用的 Brutalist 元件契約：BackButton（返回導航）、SectionHeader（區塊標題）、EmptyState（空狀態）、StatusBadge（狀態徽章）、BrutalistModal（彈窗），統一視覺、props 與互動行為，避免各頁面各自實作。

## Requirements

### Requirement: BackButton 元件

系統 SHALL 提供 `BackButton.vue` 元件，支援返回和關閉兩種模式。預設點擊執行 `router.back()`，可透過 `@click` 覆蓋行為。

#### Scenario: 預設返回按鈕
- **WHEN** 使用 `<BackButton />`
- **THEN** 顯示 ChevronLeft icon，點擊執行 router.back()

#### Scenario: 關閉按鈕模式
- **WHEN** 使用 `<BackButton icon="close" />`
- **THEN** 顯示 X icon

#### Scenario: 自訂點擊行為
- **WHEN** 使用 `<BackButton @click="handler" />`
- **THEN** 點擊執行 handler，不執行 router.back()

### Requirement: SectionHeader 元件

系統 SHALL 提供 `SectionHeader.vue` 元件，顯示區塊標題 + border-b-4 分隔線，支援右側 slot。

#### Scenario: 基本標題
- **WHEN** 使用 `<SectionHeader title="Menu" />`
- **THEN** 顯示 h2 標題 + border-b-4 border-black dark:border-white

#### Scenario: 帶右側內容
- **WHEN** 使用 `<SectionHeader title="Active">` 並填入 `#right` slot
- **THEN** 標題左側、slot 內容右側，flex justify-between

#### Scenario: 小型標題（History 風格）
- **WHEN** 使用 `<SectionHeader title="History" muted />`
- **THEN** 標題和分隔線套用 opacity-40 效果

### Requirement: EmptyState 元件

系統 SHALL 提供 `EmptyState.vue` 元件，顯示空狀態區塊（dashed border + icon + 標題 + 副標）。

#### Scenario: 帶 icon 的空狀態
- **WHEN** 使用 `<EmptyState title="NO ORDERS YET" subtitle="Go find a group buy!">` 並在 default slot 放入 icon
- **THEN** 顯示 dashed border 區塊，icon 置中、標題大寫粗體、副標半透明

#### Scenario: 無 icon 的空狀態
- **WHEN** 使用 `<EmptyState title="NO ACTIVE BUYS" />` 不填 slot
- **THEN** 只顯示標題和副標，無 icon

### Requirement: StatusBadge 元件

系統 SHALL 提供 `StatusBadge.vue` 元件，根據 OrderStatus enum 顯示對應顏色和 label 的 badge。

#### Scenario: OPEN 狀態
- **WHEN** 使用 `<StatusBadge :status="OrderStatus.OPEN" />`
- **THEN** 顯示 green-400 底色 + "Active" 文字

#### Scenario: READY 狀態
- **WHEN** 使用 `<StatusBadge :status="OrderStatus.READY" />`
- **THEN** 顯示 orange-400 底色 + "Ready" 文字

#### Scenario: 簡化模式（MyOrders 用）
- **WHEN** 使用 `<StatusBadge :status="status" variant="simple" />`
- **THEN** READY 顯示 brand 底白字，其他顯示 muted 樣式 + "WAITING"

### Requirement: BrutalistModal 元件

系統 SHALL 提供 `BrutalistModal.vue` 元件，統一 Modal 骨架（Teleport + backdrop + transition + 響應式定位）。

#### Scenario: 開啟 Modal
- **WHEN** `v-model` 為 true
- **THEN** 以 fade transition 顯示 backdrop + modal 內容

#### Scenario: 點擊 backdrop 關閉
- **WHEN** 使用者點擊 Modal 外部 backdrop
- **THEN** `v-model` 設為 false，Modal 關閉

#### Scenario: Mobile 全螢幕
- **WHEN** 螢幕寬度 < md 斷點
- **THEN** Modal 佔滿全螢幕（fixed inset-0）

#### Scenario: Desktop 置中
- **WHEN** 螢幕寬度 >= md 斷點
- **THEN** Modal 置中顯示，帶 brutalist 邊框和陰影

#### Scenario: Slots 填充
- **WHEN** 填入 `#header`、`#default`、`#footer` slot
- **THEN** 分別渲染在 Modal 的頭部、內容區、底部
