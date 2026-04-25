## Context

後台 admin UI 規範定義在 `.claude/rules/admin-ui.md`，shop 系列頁面已完全符合，可作為參考標竿。其餘頁面（admin-account、member、drink-option、verification）為較早開發，存在系統性不一致。本次修改為純前端 UI 層調整，不涉及 API 或資料邏輯。

## Goals / Non-Goals

**Goals:**
- 所有已開發的後台列表頁、編輯頁、建立頁 100% 符合 admin-ui.md 規範
- 修改後行為與 shop 系列頁面一致

**Non-Goals:**
- 不修改尚未開發的頁面（order/list、notification/list、notification/by-group、system/setting）
- 不新增功能或改變業務邏輯
- 不重構元件結構或提取新的共用元件

## Decisions

### 1. 時間格式統一使用 `formatDateTime()`

各列表頁的 `new Date(row.created_at).toLocaleString('zh-TW')` 統一替換為 `formatDateTime(row.created_at)`，並在 script 區塊 import `formatDateTime` from `~/utils/format`。verification 頁面的 sent_at / expires_at 也一併套用。

### 2. 編輯頁 AppTimestamp 取代 disabled input

admin-account/[id]/edit.vue 與 member/[id]/edit.vue 將建立時間 / 更新時間的 disabled input 移除，改在 el-card header 中使用 `<AppTimestamp>`，傳入 API 回傳的原始日期字串（不再手動 `toLocaleString`）。

### 3. useUnsavedGuard 加入所有編輯頁

8 個缺少離開保護的編輯頁面統一加入 `useUnsavedGuard(form)`。資料載入完成後呼叫 `takeSnapshot()`，儲存成功後再次呼叫 `takeSnapshot()`。

shop/[id]/edit.vue 目前使用 `markSaved()` 而非二次 `takeSnapshot()`，確認 `useUnsavedGuard` composable 中 `markSaved` 等同 `takeSnapshot` 的語意，若是則保持現狀。

### 4. el-input-number 預設控制位置 + 固定寬度

- 表單：移除 `controls-position="right"`，寬度改為 `style="width: 180px; max-width: 100%"`
- 表格內排序欄位：移除 `controls-position="right"`，寬度改為 `120px`，el-table-column `width="150"`
- 表格內非排序欄位（如 shop/override 覆寫價格/排序）：移除 `controls-position="right"`，寬度改為 `180px`

### 5. role/index.vue 排序支援

加入 `@sort-change` handler，ID 與建立時間欄位加上 `sortable="custom"`。此頁面目前無分頁（角色數量少），排序為前端 UI 一致性考量。但因後端 `/api/admin/roles` 可能不支援 sort_by / sort_order 參數，需確認 — 若不支援則僅加上 sortable UI 但不實際呼叫後端排序（或暫時跳過此項，標記為 open question）。

## Risks / Trade-offs

- **role/index.vue 排序**：後端可能不支援排序參數，需確認 API → 若不支援，暫不加排序功能，避免前後端不一致
- **useUnsavedGuard 對小單元頁面**：drink-option 的 edit 頁面欄位極少（1-3 個），加離開保護略顯過度但符合規範統一性
- **markSaved vs takeSnapshot**：需確認 composable 實作是否語意等價，若不等價需統一
