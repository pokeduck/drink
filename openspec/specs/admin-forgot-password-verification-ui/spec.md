# admin-forgot-password-verification-ui

## Purpose

定義 Admin 後台 `/member/verification/forgot-password` 頁面契約：以列表呈現前台會員忘記密碼驗證信紀錄，支援關鍵字 / 狀態篩選、server-side 排序、分頁，並依 `MENU.VerificationForgotPassword` 角色提供單筆與批次重發功能。

## Requirements

### Requirement: 忘記密碼驗證信列表

管理員 SHALL 能在 `/member/verification/forgot-password` 頁面查看前台會員忘記密碼驗證信紀錄，透過 `GET /api/admin/verifications/forgot-password` 取得資料，顯示 ID、名稱、Email、發送狀態、使用狀態、發送時間、過期時間。

#### Scenario: 頁面載入顯示列表
- **WHEN** 管理員進入忘記密碼驗證信頁面
- **THEN** 系統呼叫 API 取得列表並以表格顯示，預設依發送時間降冪排序

#### Scenario: 過期時間紅字標示
- **WHEN** 驗證信已過期（expires_at < 當前時間）
- **THEN** 過期時間欄位 SHALL 以紅色文字顯示

### Requirement: 關鍵字搜尋與篩選

管理員 SHALL 能透過關鍵字搜尋（姓名/Email）、發送狀態篩選（成功/失敗）、使用狀態篩選（已使用/未使用）過濾列表資料。

#### Scenario: 輸入關鍵字搜尋
- **WHEN** 管理員輸入關鍵字並按下查詢（或 Enter）
- **THEN** 系統以 keyword 參數重新查詢，頁碼重置為 1

#### Scenario: 選擇篩選條件
- **WHEN** 管理員選擇發送狀態或使用狀態篩選
- **THEN** 系統以對應參數重新查詢，頁碼重置為 1

#### Scenario: 清除篩選
- **WHEN** 管理員清除篩選條件
- **THEN** 系統重新查詢不帶該篩選參數

### Requirement: Server-side 排序

管理員 SHALL 能對 ID、發送時間、過期時間欄位進行 server-side 排序。

#### Scenario: 點擊欄位排序
- **WHEN** 管理員點擊可排序欄位標頭
- **THEN** 系統以 sort_by / sort_order 參數重新查詢，頁碼重置為 1

#### Scenario: 取消排序回到預設
- **WHEN** 管理員取消排序
- **THEN** 系統回到預設排序（sent_at desc）

### Requirement: 分頁

管理員 SHALL 能切換頁碼與每頁筆數（10/20/50/100），透過 `AppPagination` 元件操作。

#### Scenario: 切換頁碼或每頁筆數
- **WHEN** 管理員變更頁碼或每頁筆數
- **THEN** 系統以新的 page / page_size 參數重新查詢

### Requirement: 單筆重發驗證信

具有 `MENU.VerificationForgotPassword` create 權限的管理員 SHALL 能對單筆紀錄重發驗證信。

#### Scenario: 點擊重發按鈕
- **WHEN** 管理員點擊某筆紀錄的「重發」按鈕
- **THEN** 系統顯示確認對話框，確認後呼叫 `POST /api/admin/verifications/{id}/resend`

#### Scenario: 重發成功
- **WHEN** API 回傳成功
- **THEN** 顯示成功通知並重新載入列表

#### Scenario: 重發頻率限制
- **WHEN** API 回傳 429（10 分鐘內已重發）
- **THEN** 顯示錯誤訊息

### Requirement: 批次重發驗證信

具有 `MENU.VerificationForgotPassword` create 權限的管理員 SHALL 能勾選多筆紀錄後批次重發。

#### Scenario: 批次重發
- **WHEN** 管理員勾選多筆紀錄並點擊「批量重發」按鈕
- **THEN** 系統顯示確認對話框，確認後呼叫 `POST /api/admin/verifications/forgot-password/resend` 帶 ids

#### Scenario: 批次重發結果包含跳過
- **WHEN** API 回傳 skip_count > 0
- **THEN** 顯示成功筆數與跳過筆數

#### Scenario: 未勾選紀錄
- **WHEN** 管理員未勾選任何紀錄就點擊批量重發
- **THEN** 顯示錯誤提示

### Requirement: 權限控制

頁面操作 SHALL 依據 `MENU.VerificationForgotPassword`（Id=22）權限控制。

#### Scenario: 無 create 權限
- **WHEN** 管理員角色無 VerificationForgotPassword create 權限
- **THEN** 「重發」按鈕與「批量重發」按鈕不顯示
