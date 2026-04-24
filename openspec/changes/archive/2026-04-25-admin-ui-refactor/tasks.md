## 1. 統一回饋 Composable

- [x] 1.1 建立 `useApiFeedback.ts` composable：整合 `showSuccess()`（ElNotification）、`handleError()`（ElMessageBox.alert + 智慧 modal）、`startLoading()` / `stopLoading()`（ElLoading.service fullscreen，最低 1 秒）
- [x] 1.2 修改 `useAdminApi.ts` error middleware：移除 `ElMessage.error` 呼叫，改為不做提示（401 以外的錯誤交由頁面層處理）；網路斷線錯誤（onError）改用 `ElMessageBox.alert`
- [x] 1.3 移除舊的 `useApiError.ts`（功能已合併至 `useApiFeedback.ts`）
- [x] 1.4 評估是否移除 `useLoading.ts` — 保留，列表頁讀取 loading 和 login 仍需使用，僅移除寫入操作的用法

## 2. 全局樣式調整

- [x] 2.1 在全局樣式中加入 `.el-form-item__error { margin-top: 4px; }`
- [x] 2.2 確認 ElNotification 右上角位置不與 header 重疊（offset: 76 避開 60px header）

## 3. 既有頁面遷移 — 替換回饋方式

- [x] 3.1 `admin-account/create.vue`：ElMessage.success → showSuccess，套用 fullscreen loading
- [x] 3.2 `admin-account/[id]/edit.vue`：ElMessage.success / ElMessage.error → showSuccess / handleError，套用 fullscreen loading
- [x] 3.3 `admin-account/list.vue`：ElMessage.success / warning → showSuccess，刪除確認的 loading 改 fullscreen
- [x] 3.4 `admin-account/role/create.vue`：ElMessage.success / error → showSuccess / handleError，套用 fullscreen loading
- [x] 3.5 `admin-account/role/[roleId]/edit.vue`：同上
- [x] 3.6 `admin-account/role/index.vue`：刪除操作的 ElMessage.success → showSuccess，套用 fullscreen loading
- [x] 3.7 `member/create.vue`：ElMessage.success → showSuccess，套用 fullscreen loading
- [x] 3.8 `member/[id]/edit.vue`：ElMessage.success / error → showSuccess / handleError，套用 fullscreen loading
- [x] 3.9 `member/list.vue`：重設密碼的 ElMessage.success / warning → showSuccess，套用 fullscreen loading
- [x] 3.10 `member/verification/register.vue`：ElMessage.success / warning → showSuccess，套用 fullscreen loading
- [x] 3.11 `change-password.vue`：ElMessage.success → showSuccess，套用 fullscreen loading
- [x] 3.12 `index.vue`：ElMessage.error → handleError

## 4. drink-option 拆獨立頁面

- [x] 4.1 建立 `drink-option/item/list.vue`、`create.vue`、`[id]/edit.vue`，使用新 composable，刪除舊 `item.vue`
- [x] 4.2 建立 `drink-option/sugar/list.vue`、`create.vue`、`[id]/edit.vue`，使用新 composable，刪除舊 `sugar.vue`
- [x] 4.3 建立 `drink-option/ice/list.vue`、`create.vue`、`[id]/edit.vue`，使用新 composable，刪除舊 `ice.vue`
- [x] 4.4 建立 `drink-option/size/list.vue`、`create.vue`、`[id]/edit.vue`，使用新 composable，刪除舊 `size.vue`
- [x] 4.5 建立 `drink-option/topping/list.vue`、`create.vue`、`[id]/edit.vue`，使用新 composable，刪除舊 `topping.vue`

## 5. 驗證與清理

- [x] 5.1 確認所有頁面不再直接使用 `ElMessage`（全域搜尋 `ElMessage` 應為零結果）
- [x] 5.2 確認所有按鈕已移除 `:loading` prop
- [x] 5.3 確認所有 drink-option 舊的單檔頁面已刪除
- [ ] 5.4 測試 Dialog 內（刪除/重設密碼）API 錯誤時 Alert 不疊灰（需手動測試）
- [ ] 5.5 測試獨立頁面 API 錯誤時 Alert 有灰色遮罩（需手動測試）
