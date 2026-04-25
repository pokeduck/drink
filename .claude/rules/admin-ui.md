# Admin 後台 UI 規範

## 頁面結構

- 所有 list / edit / create 頁面必須包含 `<AppBreadcrumb />`（麵包屑）
- 所有 list / edit / create 頁面的 `el-card` 必須有 `<template #header>` 顯示單元名稱
  - list 頁面：顯示列表名稱（如「帳號列表」、「冰塊列表」）
  - edit / create 頁面：card header 左側放返回按鈕 + 標題，edit 右側放 `<AppTimestamp>`（若有）
- 非 Dialog 的 edit / create 頁面，表單只放「儲存」或「建立」按鈕，不放「取消」按鈕
- 返回上一頁：不使用 `el-page-header`，改用 card header 內的 `el-button text` + `ArrowLeft` icon
  - 與標題放在同一行，用 `gap: 8px` 間隔
  - edit 頁面（有 AppTimestamp）結構：
    ```html
    <template #header>
      <div style="display: flex; justify-content: space-between; align-items: center">
        <div style="display: flex; align-items: center; gap: 8px">
          <el-button text @click="router.push('/xxx/list')"><el-icon><ArrowLeft /></el-icon>返回</el-button>
          <span>編輯 XXX</span>
        </div>
        <AppTimestamp v-if="createdAt" :created-at="createdAt" :updated-at="updatedAt" />
      </div>
    </template>
    ```
  - create 頁面（無 AppTimestamp）結構：
    ```html
    <template #header>
      <div style="display: flex; align-items: center; gap: 8px">
        <el-button text @click="router.push('/xxx/list')"><el-icon><ArrowLeft /></el-icon>返回</el-button>
        <span>新增 XXX</span>
      </div>
    </template>
    ```

## 表單欄位群組（Field Group）

每個表單欄位視為一個 group，由上到下依序為：

1. **欄位名稱**（label）
2. **輸入框**（input / select / radio 等）
3. **輸入提示字**（hint）— 僅在該欄位有提示時才顯示，無提示則不預留空間
4. **錯誤訊息**（error message）— 不預留空間，出現時自然往下推（業界主流做法）

### 間距規則

- group 之間：使用 Element Plus `el-form-item` 預設的 `margin-bottom` 控制間距
- 錯誤訊息：全域 CSS（`app.vue`）覆蓋 Element Plus 預設的 `position: absolute`，改為 `position: relative; top: auto; left: auto; padding-top: 0; margin-top: 4px`，出現時自然往下推
- 提示字：使用 `<FormHint>` 元件（`~/components/FormHint.vue`），樣式已封裝

### 數字輸入框（el-input-number）

- 只允許整數（可負數），不允許小數點、千分位
- 所有 `el-input-number` 必須加上 `:precision="0"`
- 清空或輸入非數字時，自動補回 `0`（全域 plugin `input-number-defaults.client.ts` 處理）
- 加減按鈕使用預設位置（左右兩側），不使用 `controls-position="right"`
- 固定寬度 `width: 180px; max-width: 100%`（可容納 4 位數）
- 表格內的數字輸入框同樣使用預設位置，寬度 `180px`
- 列表排序欄位：輸入框寬度 `120px`，欄位寬度 `width="150"`，兩位數即可
- 表格內有 placeholder 的 input-number（如覆寫頁面）：價格寬度 `240px`/column `280`，排序寬度 `180px`/column `220`

### Checkbox 群組（全選 / 取消全選）

- 全選 / 取消全選按鈕放在 content 側第一行，與 label 同一行高度
- 按鈕使用 `size="small"`，外層 div 加 `margin-top: 4px` 對齊 label
- checkbox 列表在按鈕下方

## 編輯頁面時間戳記

- 所有非 Dialog 的編輯頁面需顯示「建立時間」與「最後更新時間」
- 位置：Card header 右側，與左側的返回按鈕 + 標題同一行
- 使用 `<AppTimestamp>` 元件（`~/components/AppTimestamp.vue`），傳入原始日期字串
- 格式同全站：`yyyy/MM/dd HH:mm:ss`，內部使用 `formatDateTime()` 格式化
- 範例：`建立：2026/04/25 14:30:00　｜　更新：2026/04/25 15:00:00`

## 列表頁建立時間欄位

- 格式固定：`yyyy/MM/dd HH:mm:ss`
- 欄位寬度固定：`width="160"`
- 全站統一，使用共用 `formatDateTime()`（`~/utils/format.ts`）格式化

## 編輯頁面離開保護

- 所有編輯頁面使用 `useUnsavedGuard(form)`（`~/composable/useUnsavedGuard.ts`）
- 資料載入後呼叫 `takeSnapshot()` 記錄初始狀態
- 儲存成功後再次呼叫 `takeSnapshot()` 更新快照
- 表單有修改未儲存時，路由離開或關閉瀏覽器會跳確認提示
- 純瀏覽未修改不會攔截

## Scroll to Top 按鈕

- 全站共用，放在 `default.vue` layout 的 `.admin-main` 內
- 使用 `el-backtop`，`target=".admin-main"`，常駐顯示（`:visibility-height="0"`）
- 位置：右下角 `right: 40, bottom: 40`
- 透明度：預設 `opacity: 0.7`，hover 時 `opacity: 1`

## UI 層級（由上到下）

1. **全站 Loading**（fullscreen lock）
2. **Alert / MessageBox**
3. **頁面 Loading**（`.admin-main` v-loading）
4. **Dialog**
5. **Scroll to Top 按鈕**
6. **頁面主體**（body）

## 列表頁分頁

- 使用 `AppPagination` 元件
- layout：`共 X 筆`（靠左）→ `每頁 N 筆/頁`（靠右）→ 上下頁 → `跳至 X 頁`
- 預設每頁 20 筆，可選 10 / 20 / 50 / 100
- 分頁區塊 `margin-top: 16px`
