## ADDED Requirements

### Requirement: Typed API client composable
每個 Nuxt app SHALL 提供一個 composable 回傳 typed API client 實例，所有 API 呼叫透過此 client 發出。

#### Scenario: 在 admin app 呼叫 API
- **WHEN** 開發者在 admin app 使用 `useAdminApi()` 取得 client 並呼叫 `client.GET('/api/roles/{id}', { params: { path: { id: 1 } } })`
- **THEN** TypeScript SHALL 自動推導回應型別為對應的 Response DTO，無需手動標註泛型

#### Scenario: 在 client app 呼叫 API
- **WHEN** 開發者在 client app 使用 `useUserApi()` 取得 client 並呼叫 API
- **THEN** TypeScript SHALL 自動推導回應型別，行為與 admin app 一致

### Requirement: Auth middleware 自動注入 token
API client 的 auth middleware SHALL 自動從 auth store 取得 access token 並注入至每個請求的 Authorization header。

#### Scenario: 已登入使用者發出 API 請求
- **WHEN** 使用者已登入且 auth store 中有有效的 access token
- **THEN** middleware SHALL 自動附加 `Authorization: Bearer <token>` header

#### Scenario: Access token 過期收到 401
- **WHEN** API 回傳 401 且 auth store 中有 refresh token
- **THEN** middleware SHALL 嘗試用 refresh token 取得新的 access token 並重送原始請求

#### Scenario: Refresh 也失敗
- **WHEN** refresh token 請求也失敗（過期或無效）
- **THEN** middleware SHALL 清除 auth 狀態並重導至登入頁

### Requirement: Error middleware 通知使用者
API client 的 error middleware SHALL 對非 401 的 API 錯誤以 toast 通知使用者。

#### Scenario: API 回傳 400 錯誤
- **WHEN** API 回傳 400 且 response body 包含 `message` 欄位
- **THEN** middleware SHALL 以 error toast 顯示該 message 內容

#### Scenario: API 回傳 500 錯誤
- **WHEN** API 回傳 500 伺服器錯誤
- **THEN** middleware SHALL 以 error toast 顯示通用錯誤訊息（如「系統發生錯誤，請稍後再試」）

#### Scenario: 網路斷線
- **WHEN** 請求因網路問題失敗（無回應）
- **THEN** middleware SHALL 以 error toast 顯示網路錯誤訊息

### Requirement: Client 為 singleton 實例
每個 Nuxt app 的 API client SHALL 為 singleton，避免重複建立實例。

#### Scenario: 多次呼叫 composable
- **WHEN** 不同元件各自呼叫 `useAdminApi()`
- **THEN** SHALL 回傳同一個 client 實例
