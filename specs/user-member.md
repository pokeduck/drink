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
| CreatedAt | DateTime | ✅ | 建立時間 |
| UpdatedAt | DateTime | ✅ | 最後更新時間 |

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

## Code Style

```csharp
// Entity
public class User : BaseDataEntity
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

    public DateTime UpdatedAt { get; set; }
}
```

---

## Boundaries

✅ Always:
- Email 唯一性驗證，不分大小寫
- Google 登入必須驗證 domain 符合 appsettings 設定
- Email/Password 註冊必須經過 Email 驗證才能登入
- 登入時更新 `LastLoginAt`
- 密碼必須 bcrypt 雜湊存儲

⚠️ Ask First:
- 新增 NotificationType enum 值（影響前端通知邏輯）
- 修改 Google domain 驗證邏輯

🚫 Never:
- 允許未驗證 Email 的帳號登入
- 允許 Status = Inactive 的帳號登入
- 在日誌記錄密碼
- 允許不符合 domain 的 Google 帳號登入
