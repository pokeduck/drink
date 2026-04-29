## ADDED Requirements

### Requirement: 變更密碼

User.API SHALL 提供 `PUT /api/user/auth/password` endpoint（需 `[Authorize]`），接受 `old_password` 與 `new_password`：先以 Argon2id 驗證 `old_password` 是否正確，正確才用 Argon2id（Salt + Pepper）雜湊 `new_password` 並寫入 `User.PasswordHash`。變更成功 MUST NOT 主動撤銷任何 refresh token — 「踢出其他裝置」是獨立動作（`POST /api/user/auth/logout-all`），由使用者顯式觸發。前端 UI MUST 在變更密碼區塊提示使用者「本機現有 session 不會被中斷；若需踢出其他裝置請使用『登出所有裝置』」。

#### Scenario: 變更成功

- **WHEN** 已認證使用者送出正確的 `old_password` 與合法的 `new_password`（≥ 6 字元）
- **THEN** 系統僅更新 `PasswordHash`，回傳 `200` + `code=0`；refresh token MUST NOT 被撤銷（呼叫端的本機 session 與其他裝置 session 都繼續有效，最長至自然 7 天 refresh expiry）

#### Scenario: 舊密碼錯誤

- **WHEN** `old_password` 經 Argon2id 驗證失敗
- **THEN** 系統回傳 `400` + `code=40102` + `error="INVALID_PASSWORD"` + `errors={ "old_password": ["舊密碼錯誤"] }`，且 MUST NOT 更新任何資料

#### Scenario: 新密碼欄位驗證失敗

- **WHEN** `new_password` 為空、長度 < 6 或缺漏
- **THEN** 系統回傳 `400` + `code=40001` + `error="VALIDATION_ERROR"`，並在 `errors` 字典標出 `new_password`

#### Scenario: 未認證

- **WHEN** 客戶端未帶 Authorization header 或 access_token 過期
- **THEN** ASP.NET 的 `[Authorize]` middleware 回傳 `401`

#### Scenario: 前端變更密碼成功後保留 session

- **WHEN** Client 收到變更密碼成功 (200) 回應
- **THEN** Client MUST NOT 呼叫 `clearTokens()`、MUST NOT redirect 到 `/login`；MUST 清空密碼表單欄位並顯示成功 toast「密碼已變更」

### Requirement: 登出所有裝置

User.API SHALL 提供 `POST /api/user/auth/logout-all` endpoint（需 `[Authorize]`），呼叫 `RevokeAllTokens(CurrentUserId)` 撤銷該 user 所有 `RevokedAt = null` 的 refresh token。Endpoint MUST 為冪等：即使該 user 沒有任何未撤銷 token 也回 `200`。

#### Scenario: 撤銷有效 tokens

- **WHEN** 已認證使用者呼叫 endpoint，DB 中該 user 有 3 個 `RevokedAt = null` 的 token
- **THEN** 系統將這 3 個 token 全部 `RevokedAt = now`，回傳 `200` + `code=0`

#### Scenario: 已無未撤銷 token

- **WHEN** 已認證使用者呼叫 endpoint，DB 中該 user 已無未撤銷 token
- **THEN** 系統不修改任何資料，回傳 `200` + `code=0`

#### Scenario: 未認證

- **WHEN** 客戶端未帶 Authorization header 或 access_token 過期
- **THEN** ASP.NET 的 `[Authorize]` middleware 回傳 `401`
