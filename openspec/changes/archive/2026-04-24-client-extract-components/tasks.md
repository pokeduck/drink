## 1. 建立共用元件

- [x] 1.1 建立 `components/BackButton.vue`（icon prop: back/close、預設 router.back、@click 覆蓋）
- [x] 1.2 建立 `components/SectionHeader.vue`（title prop、muted prop、#right slot）
- [x] 1.3 建立 `components/EmptyState.vue`（title/subtitle props、default slot for icon）
- [x] 1.4 建立 `components/StatusBadge.vue`（status prop、variant prop: default/simple、色碼 mapping）
- [x] 1.5 建立 `components/BrutalistModal.vue`（v-model、size prop、#header/#default/#footer slots、backdrop click close）

## 2. 重構頁面 — 套用新元件

- [x] 2.1 重構 `layouts/default.vue`：無需替換（BackButton 不在 layout 中）
- [x] 2.2 重構 `pages/create.vue`：BackButton、BrutalistModal（店家選擇）、EmptyState（搜尋無結果）
- [x] 2.3 重構 `pages/group/[id].vue`：BackButton、SectionHeader（Menu）、BrutalistModal（點餐）
- [x] 2.4 重構 `pages/index.vue`：SectionHeader（Active Groups / History）、EmptyState（NO ACTIVE BUYS）
- [x] 2.5 重構 `pages/my-orders.vue`：StatusBadge（simple variant）、EmptyState（NO ORDERS YET）
- [x] 2.6 重構 `pages/admin/[id].vue`：BackButton、SectionHeader（Order List）
- [x] 2.7 重構 `components/GroupCard.vue`：StatusBadge

## 3. 驗證

- [x] 3.1 啟動 dev server，確認所有頁面視覺與重構前完全一致
