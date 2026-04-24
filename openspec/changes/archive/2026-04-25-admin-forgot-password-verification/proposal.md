## Why

後台「忘記密碼驗證信」管理頁面目前僅有 stub（顯示「此功能尚未開發」）。Backend API 已完成（列表、單筆重發、批次重發），需要實作前端頁面讓管理員可以查看與管理前台會員的忘記密碼驗證信紀錄。

## What Changes

- 實作 `app/admin/app/pages/member/verification/forgot-password.vue` 完整頁面
  - 列表展示驗證信紀錄（寄送狀態、使用狀態、過期狀態）
  - 關鍵字搜尋（姓名/Email）
  - 篩選（寄送成功/失敗、已使用/未使用）
  - Server-side 排序（ID、建立時間）
  - 分頁
  - 單筆重發驗證信
  - 批次重發驗證信
- 權限控制使用 Menu Id = 22

## Capabilities

### New Capabilities
- `admin-forgot-password-verification-ui`: 後台忘記密碼驗證信管理頁面，包含列表、篩選、排序、重發功能

### Modified Capabilities

（無，Backend API 與權限已就緒）

## Impact

- **前端**：`web/apps/admin/app/pages/member/verification/forgot-password.vue`（從 stub 改為完整頁面）
- **API 依賴**：使用現有 `GET /api/admin/verifications/forgot-password`、`POST /api/admin/verifications/{id}/resend`、`POST /api/admin/verifications/forgot-password/resend`
- **權限**：AdminMenu Id = 22（忘記密碼驗證信），需角色已授權才可操作
