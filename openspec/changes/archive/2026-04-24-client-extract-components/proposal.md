## Why

前台 client app 剛完成 React → Nuxt UI 翻譯，6 個頁面中存在大量重複的 UI pattern（返回按鈕 10+、區塊標題 4、空狀態 3、狀態 badge 3、Modal 骨架 2）。抽取共用元件可以減少重複、統一視覺、降低未來維護成本。

## What Changes

- **新增 `BackButton.vue`**：統一返回/關閉按鈕（ChevronLeft / X icon），替換 4 個頁面中 10+ 處 inline 寫法
- **新增 `SectionHeader.vue`**：統一區塊標題（h2 + border-b-4 + 可選右側文字），替換 3 個頁面中 4 處
- **新增 `EmptyState.vue`**：統一空狀態區塊（icon + 標題 + 副標），替換 3 個頁面中 3 處
- **新增 `StatusBadge.vue`**：統一狀態 badge（色碼 mapping + label），替換 GroupCard + MyOrders 中的重複邏輯
- **新增 `BrutalistModal.vue`**：統一 Modal 骨架（Teleport + backdrop + 響應式定位 + transition），替換 create + group detail 中 2 處
- **重構現有頁面**：將 inline pattern 替換為新元件呼叫

## Capabilities

### New Capabilities
- `client-shared-components`: 前台共用 UI 元件（BackButton、SectionHeader、EmptyState、StatusBadge、BrutalistModal）

### Modified Capabilities
_無_

## Impact

- **`web/apps/client/app/components/`**：新增 5 個元件檔案
- **`web/apps/client/app/pages/`**：所有 6 個頁面 + layout 會被重構以使用新元件
- **不影響**：CSS 設計系統、mock 資料、後端 API、admin app
- **零視覺變化**：純重構，UI 外觀完全不變
