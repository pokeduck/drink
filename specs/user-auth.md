# Spec: 前台會員系統 (User Member)

## Objective
- 前台會員支援兩種登入方式：Email/Password 和 Google OAuth
- Google 登入限制特定 domain（設定於 appsettings）
- 公司 Gmail 帳號由外部 script 批次匯入建立

---

## Entities

### User
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| Name | string(100) | ✅ | 用戶名稱 |
| Email | string(200) | ✅ | 唯一，不分大小寫 |
| PasswordHash | string | ❌ | nullable，Google 登入用戶可無密碼 |
| Avatar | string | ❌ | 頭像 URL |
| NotificationType | NotificationType (enum) | ✅ | 通知方式 |
| Status | UserStatus (enum) | ✅ | 帳號狀態 |
| EmailVerified | bool | ✅ | Email 是否已驗證 |
| IsGoogleConnected | bool | ✅ | 是否已綁定 Google 登入 |
| LastLoginAt | DateTime | ❌ | 最後登入時間 |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity），自行註冊 / Google / 批次匯入 = 0 |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity），自行操作 = 自己的 UserId，後台操作 = AdminUserId |

### UserStatus (Enum)
```csharp
public enum UserStatus
{
    Active = 1,     // 啟用
    Inactive = 2    // 停用
}
```

### NotificationType (Enum)
```csharp
public enum NotificationType
{
    None = 0,       // 不接收通知（預設值）
    WebPush = 1,
    Email = 2,
    Both = 3
}
```

---

## Business Rules

### Email/Password 註冊
- 註冊後發送驗證信
- `EmailVerified = false`，帳號無法登入直到驗證完成
- 驗證後 `EmailVerified = true`，`Status = Active`

### Google OAuth 登入
- 只允許 appsettings 設定的特定 domain（如 `@company.com`）
- 第一次 Google 登入自動建立帳號：
  - `EmailVerified = true`（Google 已驗證）
  - `IsGoogleConnected = true`
  - `PasswordHash = null`
  - `Status = Active`
- 已有 Email/Password 帳號的用戶，Google 登入後自動綁定：
  - `IsGoogleConnected = true`

### 批次匯入（外部 Script）
- 匯入公司 Gmail 清單，自動建立帳號
- `EmailVerified = true`
- `IsGoogleConnected = true`
- `Status = Active`
- `PasswordHash = null`

### 帳號停用
- 管理員將 `Status = Inactive`
- 停用帳號無法登入，返回 403

---

## API Endpoints

### 認證

#### 註冊
```
POST /api/user/auth/register
```
- Request Body:
```json
{
  "name": "Wayne",
  "email": "wayne@example.com",
  "password": "secret"
}
```
- Email 唯一（不分大小寫），重複回傳 409（`EMAIL_ALREADY_EXISTS`）
- 建立帳號：EmailVerified = false, Status = Active, IsGoogleConnected = false
- 發送驗證信（含驗證 token）
- 驗證完成前無法登入

#### Email 驗證
```
POST /api/user/auth/verify-email
```
- Request Body:
```json
{
  "token": "verification-token-string"
}
```
- Token 無效或過期回傳 400（`INVALID_TOKEN`）
- 成功後 EmailVerified = true

#### 登入
```
POST /api/user/auth/login
```
- Request Body:
```json
{
  "email": "wayne@example.com",
  "password": "secret"
}
```
- EmailVerified = false 回傳 403（`EMAIL_NOT_VERIFIED`）
- Status = Inactive 回傳 403（`ACCOUNT_INACTIVE`）
- 密碼錯誤回傳 401（`INVALID_CREDENTIALS`）
- 成功回傳 access_token（短效 15 分鐘）+ refresh_token（長效 7 天）
- 建立 UserRefreshToken 記錄
- 更新 LastLoginAt
- Response:
```json
{
  "access_token": "eyJhbG...",
  "refresh_token": "dGhpcyBpcyByZWZyZXNo..."
}
```

#### Google 登入
```
POST /api/user/auth/google
```
- Request Body:
```json
{
  "id_token": "google-id-token-string"
}
```
- 後端驗證 id_token，取得 email, name, avatar
- Domain 不符合 appsettings 設定回傳 403（`GOOGLE_DOMAIN_NOT_ALLOWED`）
- Status = Inactive 回傳 403（`ACCOUNT_INACTIVE`）
- 首次登入：自動建立帳號（EmailVerified = true, IsGoogleConnected = true, PasswordHash = null, Status = Active）
- 已有帳號：自動綁定 IsGoogleConnected = true
- 更新 LastLoginAt
- 回傳 access_token + refresh_token（同 login）

#### 刷新 Token
```
POST /api/user/auth/refresh
```
- Request Body:
```json
{
  "refresh_token": "dGhpcyBpcyByZWZyZXNo..."
}
```
- 驗證 refresh_token 是否存在、未過期、未撤銷
- Refresh Token Rotation：舊 token 作廢，發新的 access_token + refresh_token
- 若 token 已被使用過（重複使用偵測），撤銷該用戶所有 refresh token
- Response: 同 login

#### 登出
```
POST /api/user/auth/logout
```
- Request Body:
```json
{
  "refresh_token": "dGhpcyBpcyByZWZyZXNo..."
}
```
- 撤銷該 refresh_token（標記 RevokedAt）

---

### 個人資料

#### 取得個人資料
```
GET /api/user/profile
```
- 從 JWT Token 取得 UserId
- Response:
```json
{
  "data": {
    "id": 1,
    "name": "Wayne",
    "email": "wayne@example.com",
    "avatar": "https://example.com/avatar.jpg",
    "notification_type": 1,
    "is_google_connected": true,
    "email_verified": true
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

#### 更新個人資料
```
PUT /api/user/profile
```
- Request Body:
```json
{
  "name": "Wayne Updated",
  "avatar": "https://example.com/new-avatar.jpg",
  "notification_type": 2
}
```
- 不可修改 email、password、status（分別由其他流程處理）

---

## Entities（補充）

### UserRefreshToken
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK，自動遞增 |
| UserId | int | FK → User |
| Token | string | Refresh Token 值（唯一） |
| ExpiresAt | DateTime | 過期時間 |
| RevokedAt | DateTime? | 撤銷時間，null = 有效 |
| CreatedAt | DateTime | 建立時間 |
| UpdatedAt | DateTime | 最後更新時間 |

---

## Code Style

```csharp
// Entity
public class User : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(200)]
    public string Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? Avatar { get; set; }

    public NotificationType NotificationType { get; set; }

    public UserStatus Status { get; set; }

    public bool EmailVerified { get; set; }

    public bool IsGoogleConnected { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}
```

---

## Success Criteria

### 註冊 & 驗證
- [ ] `POST /api/user/auth/register` 建立帳號，EmailVerified = false，發送驗證信
- [ ] Email 唯一性驗證不分大小寫，重複回傳 409
- [ ] `POST /api/user/auth/verify-email` 驗證成功後 EmailVerified = true
- [ ] 未驗證帳號無法登入，回傳 403（`EMAIL_NOT_VERIFIED`）

### 登入
- [ ] `POST /api/user/auth/login` 驗證成功回傳 access_token + refresh_token
- [ ] Status = Inactive 的帳號無法登入，回傳 403（`ACCOUNT_INACTIVE`）
- [ ] 登入成功時更新 LastLoginAt
- [ ] 密碼以 Argon2id 雜湊存儲（Salt + Pepper）

### Google OAuth
- [ ] `POST /api/user/auth/google` 驗證 id_token，domain 不符回傳 403
- [ ] 首次 Google 登入自動建立帳號（EmailVerified = true, IsGoogleConnected = true, PasswordHash = null）
- [ ] 已有帳號的用戶，Google 登入後自動綁定 IsGoogleConnected = true
- [ ] 批次匯入的帳號：EmailVerified = true, IsGoogleConnected = true, Status = Active

### Refresh Token
- [ ] `POST /api/user/auth/refresh` 實作 Refresh Token Rotation（舊 token 作廢、發新 token）
- [ ] 重複使用已作廢的 refresh_token 時，撤銷該用戶所有 refresh_token
- [ ] `POST /api/user/auth/logout` 撤銷指定 refresh_token

### 個人資料
- [ ] `GET /api/user/profile` 回傳當前用戶資料
- [ ] `PUT /api/user/profile` 可修改 name、avatar、notification_type

---

## Boundaries

✅ Always:
- Email 唯一性驗證，不分大小寫
- Google 登入必須驗證 domain 符合 appsettings 設定
- Email/Password 註冊必須經過 Email 驗證才能登入
- 登入時更新 `LastLoginAt`
- 密碼必須 Argon2id 雜湊存儲（Salt + Pepper）

⚠️ Ask First:
- 新增 NotificationType enum 值（影響前端通知邏輯）
- 修改 Google domain 驗證邏輯

🚫 Never:
- 允許未驗證 Email 的帳號登入
- 允許 Status = Inactive 的帳號登入
- 在日誌記錄密碼
- 允許不符合 domain 的 Google 帳號登入
