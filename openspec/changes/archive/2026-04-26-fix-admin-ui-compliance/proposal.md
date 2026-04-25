## Why

後台 admin UI 在不同時期開發的頁面存在大量與 `.claude/rules/admin-ui.md` 規範不一致的問題。shop 系列頁面（最後製作）幾乎完全符合規範，但較早的 admin-account、member、drink-option、verification 系列頁面有系統性違規，造成 UI 行為與視覺不一致。

## What Changes

- 列表頁「建立時間」欄位統一使用 `formatDateTime()` 格式化，取代 `new Date().toLocaleString('zh-TW')`（12 處）
- 列表頁時間欄位寬度統一改為 `width="160"`，取代 `width="180"`（10 處）
- 所有 `el-input-number` 移除 `controls-position="right"`，改用預設左右兩側按鈕（17+ 處）
- 表單 `el-input-number` 寬度統一改為 `width: 180px; max-width: 100%`，取代 `width: 100%`（10+ 處）
- 列表頁表格內排序欄位 input-number 寬度改為 `120px`、欄位 `width="150"`（5 頁）
- 編輯頁面（admin-account、member）加入 `<AppTimestamp>` 至 Card header，取代 disabled input 顯示時間（2 頁）
- 所有編輯頁面加入 `useUnsavedGuard(form)` + `takeSnapshot()` 離開保護（8 頁）
- admin-account/role/index.vue 加入 `sortable="custom"` 及 `@sort-change`（1 頁）
- shop/override.vue 表格內 input-number 移除 `controls-position="right"` 並調整寬度為 `180px`

## Capabilities

### New Capabilities

（無新增功能，本次為現有頁面規範修正）

### Modified Capabilities

- `admin-ui-compliance`: 跨模組 UI 規範一致性修正，涵蓋所有已開發的後台頁面

## Impact

- **前端頁面**：`web/apps/admin/app/pages/` 下約 20 個 .vue 檔案需修改
- **共用元件**：無需修改（AppTimestamp、FormHint、AppPagination 已符合規範）
- **後端 API**：無影響
- **破壞性變更**：無，純 UI 層修正
