## Context

Admin 前端目前的 API 回饋機制有幾個問題：

1. **錯誤處理重複觸發**：`useAdminApi.ts` 的 error middleware 用 `ElMessage.error` 顯示 Toast，而頁面層的 `useApiError.ts` 又用 `ElMessageBox.alert` 顯示 Alert，兩者會同時觸發
2. **成功提示**：各頁面散落的 `ElMessage.success()` 直接呼叫，位置與 header 重疊
3. **Loading 方式不統一**：有按鈕 `:loading`、有局部 `v-loading`、有些沒有 loading
4. **Dialog 內表單 + 錯誤提示**：多層 UI 堆疊問題

### 現有架構

```
useAdminApi.ts (全局 middleware)
├── onResponse: 非 401 錯誤 → ElMessage.error()     ← Toast
└── onError: 網路錯誤 → ElMessage.error()            ← Toast

useApiError.ts (頁面級 composable)
├── 有 errors → serverErrors（inline 紅字）
└── 無 errors → ElMessageBox.alert()                 ← Alert（跟上面重複！）

useLoading.ts (帶最低 1 秒的 loading)
└── 各頁面自行決定 loading 綁定方式

各頁面直接呼叫
└── ElMessage.success('xxx')                          ← 散落各處
```

## Goals / Non-Goals

**Goals:**
- 統一 API 回饋為單一觸發點，消除重複提示
- 成功 → ElNotification（右上角）、通用錯誤 → ElMessageBox.alert（智慧 modal）、欄位錯誤 → inline
- 寫入操作（CUD）統一全頁 fullscreen loading
- drink-option 5 模組表單拆獨立頁面
- 表單錯誤紅字增加間距

**Non-Goals:**
- 不改列表頁的讀取 loading（維持現有 v-loading 蓋 el-card 的方式）
- 不改確認型 Dialog（刪除確認、重設密碼確認維持 ElMessageBox.confirm）
- 不改 client 前台

## Decisions

### 1. 錯誤處理：移除 middleware 層，統一由頁面層控制

**選擇**：移除 `useAdminApi.ts` 的 error middleware 中的 `ElMessage.error`，讓錯誤由 `useApiError.ts` 統一處理

**替代方案**：保留 middleware 層改用 Alert → 但 middleware 無法知道頁面是否有 Dialog 開啟，無法做 modal 判斷

**原因**：頁面層才有上下文（是否有欄位錯誤、是否在 Dialog 中），適合做回饋決策

```
改後架構：

useAdminApi.ts (全局 middleware)
├── onResponse: 非 401 錯誤 → 不做任何提示（交給頁面處理）
└── onError: 網路錯誤 → ElMessageBox.alert()   ← 只處理 fetch 層級錯誤

useApiError.ts (統一回饋 composable，改名 useApiFeedback)
├── handleError():
│   ├── 有 errors → serverErrors（inline 紅字）
│   └── 無 errors → ElMessageBox.alert（auto modal 判斷）
├── showSuccess(): → ElNotification（右上角）
├── startLoading(): → ElLoading.service fullscreen
└── stopLoading(): → 關閉 fullscreen loading（保留最低 1 秒）
```

### 2. 全頁 Loading：使用 ElLoading.service 而非 v-loading directive

**選擇**：`ElLoading.service({ fullscreen: true, lock: true })` 封裝在 composable 中

**原因**：
- 程式化呼叫，不需要在 template 綁定 ref
- 可以搭配 `useLoading.ts` 現有的最低 1 秒邏輯
- 在 composable 中 start/stop 配對，頁面程式碼最簡潔

### 3. 成功提示：ElNotification 右上角

**選擇**：`ElNotification({ type: 'success', title: '成功', message, position: 'top-right' })`

**原因**：不遮擋內容、不與 header 重疊、不需使用者操作即消失

### 4. 錯誤 Alert 的 modal 智慧判斷

**選擇**：透過 DOM 偵測 `.el-overlay-dialog` 決定 `modal` 值

```typescript
const hasDialogOpen = () => document.querySelector('.el-overlay-dialog') !== null

ElMessageBox.alert(msg, '錯誤提示', {
  modal: !hasDialogOpen(),  // 有 Dialog → 不加灰
  zIndex: 99999,
  type: 'error',
})
```

### 5. drink-option 頁面拆分結構

```
pages/drink-option/
├── item/
│   ├── list.vue          ← 從 item.vue 拆出列表
│   ├── create.vue        ← 新增頁面
│   └── [id]/
│       └── edit.vue      ← 編輯頁面
├── sugar/
│   ├── list.vue
│   ├── create.vue
│   └── [id]/edit.vue
├── ice/   (同上結構)
├── size/  (同上結構)
└── topping/ (同上結構)
```

刪除原本的 `item.vue`、`sugar.vue`、`ice.vue`、`size.vue`、`topping.vue`

### 6. 表單錯誤紅字間距

**選擇**：全局 CSS 覆蓋

```css
.el-form-item__error {
  margin-top: 4px;
}
```

放在 `app.vue` 或全局樣式檔中

## Risks / Trade-offs

- **移除 middleware 錯誤提示**：如果某個頁面忘記用 `useApiFeedback`，API 錯誤會靜默無提示 → 透過 code review 和統一 pattern 降低風險
- **DOM 偵測 Dialog 狀態**：依賴 Element Plus 的 class name，升級可能失效 → 低風險，Element Plus class name 很穩定
- **15 個新檔案（drink-option 拆分）**：檔案數增加 → 但結構更清晰，與其他模組一致
