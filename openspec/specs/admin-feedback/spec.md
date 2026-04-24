## ADDED Requirements

### Requirement: 成功操作使用 ElNotification
所有 API 寫入操作（新增、更新、刪除）成功後，系統 SHALL 使用 `ElNotification` 在畫面右上角顯示成功提示，取代現有的 `ElMessage.success`。

#### Scenario: 新增成功
- **WHEN** 使用者新增資料且 API 回傳成功
- **THEN** 畫面右上角顯示 ElNotification success 提示，自動消失

#### Scenario: 刪除成功
- **WHEN** 使用者刪除資料且 API 回傳成功
- **THEN** 畫面右上角顯示 ElNotification success 提示，不與 header 重疊

---

### Requirement: 通用 API 錯誤使用 ElMessageBox Alert
當 API 回傳非欄位驗證的錯誤（500、網路斷線、權限不足等），系統 SHALL 使用 `ElMessageBox.alert` 顯示錯誤，使用者 MUST 按確認才關閉。

#### Scenario: 獨立頁面 API 錯誤
- **WHEN** 在獨立頁面（非 Dialog 內）發生 API 錯誤
- **THEN** 顯示 ElMessageBox.alert，帶灰色遮罩（`modal: true`），z-index 最高

#### Scenario: Dialog 內 API 錯誤
- **WHEN** 在 Dialog 開啟狀態下發生 API 錯誤
- **THEN** 顯示 ElMessageBox.alert，不帶遮罩（`modal: false`），z-index 最高，避免多層灰疊加

#### Scenario: 網路斷線
- **WHEN** fetch 層級錯誤（網路斷線、timeout）
- **THEN** 顯示 ElMessageBox.alert，訊息為「網路連線異常，請檢查網路狀態」

---

### Requirement: 欄位驗證錯誤 inline 顯示
當 API 回傳 400 且包含 `errors` 欄位字典時，系統 SHALL 將錯誤訊息顯示在對應表單欄位下方，不額外彈出 Alert 或 Notification。

#### Scenario: 欄位驗證錯誤
- **WHEN** API 回傳 `{ errors: { name: ["名稱已存在"] } }`
- **THEN** 對應欄位下方顯示紅字「名稱已存在」，不彈出其他提示

---

### Requirement: 消除重複錯誤提示
`useAdminApi.ts` 的 error middleware SHALL 不再顯示 `ElMessage.error`，錯誤回饋統一由頁面層 composable 處理，避免 middleware 和頁面同時觸發兩次提示。

#### Scenario: API 400 錯誤只觸發一次提示
- **WHEN** API 回傳 400 錯誤
- **THEN** 只有頁面層的 composable 處理提示，不會同時出現 Toast 和 Alert

---

### Requirement: 寫入操作全頁 Loading
所有 API 寫入操作（新增、更新、刪除）SHALL 使用 fullscreen loading（`ElLoading.service({ fullscreen: true, lock: true })`），保留最低 1 秒顯示時間，取代按鈕 `:loading` 和局部 `v-loading`。

#### Scenario: 新增操作 loading
- **WHEN** 使用者點擊送出按鈕觸發新增 API
- **THEN** 全頁顯示 fullscreen loading，API 完成後關閉（最低 1 秒）

#### Scenario: 刪除操作 loading
- **WHEN** 使用者確認刪除後觸發刪除 API
- **THEN** 全頁顯示 fullscreen loading，API 完成後關閉

---

### Requirement: 表單錯誤文字間距
表單欄位的錯誤紅字 SHALL 與輸入框保持至少 4px 的上間距（`margin-top`），透過全局 CSS 覆蓋 `.el-form-item__error` 實現。

#### Scenario: 錯誤文字不貼輸入框
- **WHEN** 表單欄位顯示驗證錯誤紅字
- **THEN** 紅字與輸入框之間有明顯間距，不緊貼

---

### Requirement: drink-option 表單拆獨立頁面
drink-option 5 個模組（item / sugar / ice / size / topping）的新增/編輯操作 SHALL 從 Dialog 彈窗改為獨立頁面，路由結構為 `list.vue` + `create.vue` + `[id]/edit.vue`。

#### Scenario: 新增品項
- **WHEN** 使用者在品項列表頁點擊「新增」
- **THEN** 導航至 `/drink-option/item/create` 獨立頁面

#### Scenario: 編輯品項
- **WHEN** 使用者在品項列表頁點擊某筆資料的「編輯」
- **THEN** 導航至 `/drink-option/item/{id}/edit` 獨立頁面

#### Scenario: 儲存後返回列表
- **WHEN** 使用者在新增或編輯頁面成功儲存
- **THEN** 自動導航回列表頁，並顯示 ElNotification 成功提示
