## Why

Admin 後台的新增/編輯表單目前放在 Dialog 彈窗中，當 API 發生錯誤需要顯示提示時，會造成多層 UI 堆疊（頁面 → Dialog → 錯誤提示），導致多層灰色遮罩疊加變黑、錯誤提示被遮蓋等體驗問題。同時，成功/錯誤的回饋方式不統一（混用 Toast 和 Alert），按鈕 loading 和局部 loading 的做法也不一致，需要全面整理。

## What Changes

- **表單頁面獨立化**：將 drink-option 5 個模組（item / sugar / ice / size / topping）的新增/編輯從 Dialog 拆為獨立頁面（list + create + [id]/edit）
- **統一 API 回饋機制**：
  - 操作成功 → `ElNotification`（右上角，不遮擋內容）
  - 欄位驗證錯誤（400 + errors）→ 欄位下方 inline 紅字（維持現有，不額外彈框）
  - 通用錯誤（500、網路、權限）→ `ElMessageBox.alert`（有 Dialog 開啟時 `modal: false` 避免疊灰）
- **寫入操作全局 Loading**：add / update / delete API 呼叫統一使用 `v-loading.fullscreen.lock`，移除按鈕 `:loading` 和局部 `v-loading`
- **表單錯誤文字間距**：全局調整 `.el-form-item__error` 的 `margin-top`，讓紅字與輸入框有適當間距

## Capabilities

### New Capabilities
- `admin-feedback`: 統一 API 回饋機制（成功 Notification、錯誤 Alert、全局 Loading）的 composable 封裝與全局樣式

### Modified Capabilities
<!-- 無既有 spec 的 requirements 需要變更 -->
