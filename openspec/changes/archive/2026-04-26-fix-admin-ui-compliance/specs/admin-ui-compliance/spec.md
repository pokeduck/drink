## ADDED Requirements

### Requirement: 列表頁時間欄位統一使用 formatDateTime

所有列表頁的時間欄位（建立時間、發送時間、過期時間）SHALL 使用 `formatDateTime()` 格式化，格式為 `yyyy/MM/dd HH:mm:ss`。不得使用 `new Date().toLocaleString()`。

#### Scenario: 建立時間欄位顯示格式
- **WHEN** 列表頁渲染建立時間欄位
- **THEN** 使用 `formatDateTime(row.created_at)` 格式化，顯示為 `yyyy/MM/dd HH:mm:ss`

#### Scenario: 驗證信列表時間欄位格式
- **WHEN** 驗證信列表頁渲染發送時間與過期時間
- **THEN** 使用 `formatDateTime()` 格式化，不使用 `toLocaleString`

### Requirement: 列表頁時間欄位寬度為 160

所有列表頁的時間欄位（建立時間、發送時間、過期時間）el-table-column SHALL 設定 `width="160"`。

#### Scenario: 建立時間欄位寬度
- **WHEN** 列表頁渲染時間欄位的 el-table-column
- **THEN** 設定 `width="160"`，不使用 `width="180"`

### Requirement: el-input-number 使用預設按鈕位置

所有 `el-input-number` SHALL 使用預設控制按鈕位置（左右兩側），不得使用 `controls-position="right"`。

#### Scenario: 表單 el-input-number 按鈕位置
- **WHEN** 表單頁面渲染 el-input-number
- **THEN** 不包含 `controls-position="right"` 屬性

#### Scenario: 表格內 el-input-number 按鈕位置
- **WHEN** 表格內渲染 el-input-number（排序、覆寫價格等）
- **THEN** 不包含 `controls-position="right"` 屬性

### Requirement: 表單 el-input-number 固定寬度

表單頁面中的 `el-input-number` SHALL 設定 `style="width: 180px; max-width: 100%"`。

#### Scenario: 表單排序欄位寬度
- **WHEN** create 或 edit 頁面表單中渲染 el-input-number
- **THEN** 設定 `style="width: 180px; max-width: 100%"`，不使用 `width: 100%`

### Requirement: 列表排序欄位 el-input-number 寬度與欄位寬度

列表頁表格內的排序欄位 el-input-number SHALL 設定寬度 `120px`，el-table-column SHALL 設定 `width="150"`。

#### Scenario: 列表排序欄位規格
- **WHEN** 列表頁表格內渲染排序欄位
- **THEN** el-input-number 寬度為 `120px`，el-table-column `width="150"`

### Requirement: 大單元編輯頁使用 AppTimestamp

大單元的編輯頁面（admin-account、member）SHALL 使用 `<AppTimestamp>` 元件在 Card header 右上角顯示建立時間與更新時間，傳入 API 回傳的原始日期字串。不得使用 disabled input 顯示時間。

#### Scenario: admin-account 編輯頁時間戳
- **WHEN** 渲染 admin-account 編輯頁
- **THEN** el-card header 右上角顯示 `<AppTimestamp>`，移除建立時間/更新時間的 disabled input

#### Scenario: member 編輯頁時間戳
- **WHEN** 渲染 member 編輯頁
- **THEN** el-card header 右上角顯示 `<AppTimestamp>`，移除建立時間/更新時間的 disabled input

### Requirement: 所有編輯頁面使用 useUnsavedGuard

所有編輯頁面 SHALL 使用 `useUnsavedGuard(form)` 實現離開保護。資料載入完成後 SHALL 呼叫 `takeSnapshot()`，儲存成功後 SHALL 再次呼叫 `takeSnapshot()`。

#### Scenario: 編輯頁面載入後建立快照
- **WHEN** 編輯頁面資料從 API 載入完成
- **THEN** 呼叫 `takeSnapshot()` 記錄初始狀態

#### Scenario: 編輯頁面儲存後更新快照
- **WHEN** 編輯頁面表單儲存成功
- **THEN** 呼叫 `takeSnapshot()` 更新快照

#### Scenario: 未修改直接離開不攔截
- **WHEN** 使用者未修改表單即離開頁面
- **THEN** 不顯示確認提示，直接離開

#### Scenario: 修改後離開會攔截
- **WHEN** 使用者修改表單後嘗試離開頁面
- **THEN** 顯示確認提示，詢問是否放棄修改
