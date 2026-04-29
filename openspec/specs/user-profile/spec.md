# user-profile

## Purpose

定義 Client 前台會員個人資料契約：提供取得與更新 User 自身資料的 API，僅暴露非敏感欄位，受保護欄位（email、password、status、email_verified、is_google_connected）MUST NOT 透過此端點修改。

## Requirements

### Requirement: 取得個人資料

User.API SHALL 提供 `GET /api/user/profile` 端點（需 `[Authorize]`），從 JWT 的 `NameIdentifier` claim 取得 UserId，回傳該 User 的個人資料：`id`、`name`、`email`、`avatar`、`notification_type`、`is_google_connected`、`email_verified`、`created_at`。Email、PasswordHash 等敏感識別欄位以外的內部欄位（`Status`、`Creator`、`Updater`、`UpdatedAt`）MUST NOT 暴露。`created_at` 用於前端顯示「Joined Apr 2026」之類的加入日期。

#### Scenario: 已登入取得自身資料

- **WHEN** 已認證使用者呼叫端點
- **THEN** 系統回傳 `200` + `data={ id, name, email, avatar, notification_type, is_google_connected, email_verified, created_at }`

#### Scenario: 未認證

- **WHEN** 客戶端未帶 Authorization header 或帶過期的 access_token
- **THEN** ASP.NET 的 `[Authorize]` middleware 回傳 `401`

#### Scenario: 用戶不存在

- **WHEN** access_token 內 UserId 對應的 User 已從資料庫被刪除
- **THEN** 系統回傳 `401` + `code=40002` + `error="UNAUTHORIZED"`

### Requirement: 更新個人資料

User.API SHALL 提供 `PUT /api/user/profile` 端點（需 `[Authorize]`），接受 `name`、`avatar`、`notification_type`，僅更新傳入的欄位。`email`、`password`、`status`、`email_verified`、`is_google_connected` MUST NOT 透過此端點修改。

#### Scenario: 更新成功

- **WHEN** 已認證使用者送出有效的 `name`（1–100 字）、`avatar`（URL 或 null）、`notification_type`（NotificationType enum 範圍內）
- **THEN** 系統更新對應 User 並回傳 `200` + `code=0`

#### Scenario: 欄位驗證失敗

- **WHEN** 送出 `name` 為空字串、長度超過 100、或 `notification_type` 不在 enum 範圍內
- **THEN** 系統回傳 `400` + `code=40001` + `error="VALIDATION_ERROR"`，並在 `errors` 字典標出具體欄位

#### Scenario: 嘗試修改受保護欄位

- **WHEN** Request body 帶入 `email`、`password`、`status`、`email_verified` 或 `is_google_connected`
- **THEN** 系統忽略這些欄位、不更新對應 DB 屬性，僅處理允許的欄位
