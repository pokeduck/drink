## MODIFIED Requirements

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
