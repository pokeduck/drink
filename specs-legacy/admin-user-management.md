# Spec: 前台會員管理 (Member Management)

## Objective
- 後台管理員（AdminUser）管理前台會員（User）
- 提供會員列表（分頁、搜尋、篩選）、建立會員、查看詳情、編輯資料、停用/啟用帳號、重設密碼
- 後台建立的帳號 EmailVerified = true，不需走驗證信流程

---

## Entity 變更

### NotificationType (Enum) — 新增 None
```csharp
public enum NotificationType
{
    None = 0,       // 不接收通知（預設值）
    WebPush = 1,
    Email = 2,
    Both = 3
}
```
- 後台建立會員時預設 `NotificationType = None`
- 前台用戶可自行至個人資料開啟通知

> User Entity 定義詳見 [user-auth.md](./user-auth.md)，本 spec 不重複定義

---

## Business Rules

### 建立會員
- 後台管理員可建立前台會員帳號
- 建立時 `EmailVerified = true`（不需驗證信）
- 建立時 `Status = Active`
- 建立時 `IsGoogleConnected = false`
- 建立時 `NotificationType = None`
- Email 唯一（不分大小寫），重複回傳 409
- 密碼以 Argon2id 雜湊存儲（Salt + Pepper）
- `Creator` = 操作的 AdminUserId，`Updater` = 操作的 AdminUserId

### 編輯會員
- 可修改 name, avatar, notification_type, status
- 不可修改 email, password（密碼由重設密碼處理）
- `Updater` = 操作的 AdminUserId

### 停用 / 啟用
- 透過編輯 API 修改 `Status`（Active / Inactive）
- 停用後該用戶無法登入前台（回傳 403）

### 重設密碼
- Admin 強制重設前台用戶密碼，不需 old_password
- 成功後撤銷該用戶所有 UserRefreshToken，強制重新登入
- `Updater` = 操作的 AdminUserId

---

## API Endpoints

### 會員列表
```
GET /api/admin/members?page=1&page_size=20&sort_by=created_at&sort_order=desc&keyword=wayne&status=1&email_verified=true&is_google_connected=true
```
- 遵循 SPEC.md 列表通用規範（分頁、排序、搜尋、篩選）
- keyword 搜尋欄位：name, email
- 篩選條件：status, email_verified, is_google_connected
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Wayne",
        "email": "wayne@example.com",
        "avatar": "https://example.com/avatar.jpg",
        "notification_type": 0,
        "status": 1,
        "email_verified": true,
        "is_google_connected": false,
        "last_login_at": "2025-03-01T10:00:00Z",
        "created_at": "2025-01-01T00:00:00Z"
      }
    ],
    "total": 50,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 取得單一會員
```
GET /api/admin/members/{memberId}
```
- Response:
```json
{
  "data": {
    "id": 1,
    "name": "Wayne",
    "email": "wayne@example.com",
    "avatar": "https://example.com/avatar.jpg",
    "notification_type": 0,
    "status": 1,
    "email_verified": true,
    "is_google_connected": true,
    "last_login_at": "2025-03-01T10:00:00Z",
    "created_at": "2025-01-01T00:00:00Z",
    "updated_at": "2025-03-15T08:30:00Z"
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 建立會員
```
POST /api/admin/members
```
- Request Body:
```json
{
  "name": "Wayne",
  "email": "wayne@example.com",
  "password": "initialPassword"
}
```
- Email 唯一（不分大小寫），重複回傳 409（`EMAIL_ALREADY_EXISTS`）
- 自動設定：EmailVerified = true, Status = Active, IsGoogleConnected = false, NotificationType = None

### 編輯會員
```
PUT /api/admin/members/{memberId}
```
- Request Body:
```json
{
  "name": "Wayne Updated",
  "avatar": "https://example.com/new-avatar.jpg",
  "notification_type": 2,
  "status": 1
}
```
- 不可修改 email, password

### 重設密碼
```
PUT /api/admin/members/{memberId}/password
```
- Request Body:
```json
{
  "new_password": "resetPassword"
}
```
- 不需 old_password（Admin 強制重設）
- 成功後撤銷該用戶所有 UserRefreshToken，強制重新登入

---

## Success Criteria

### 會員列表
- [ ] `GET /api/admin/members` 支援分頁、排序、keyword 搜尋 name / email、篩選 status / email_verified / is_google_connected

### 建立會員
- [ ] `POST /api/admin/members` 建立帳號，EmailVerified = true, Status = Active, IsGoogleConnected = false, NotificationType = None
- [ ] Email 唯一性驗證不分大小寫，重複回傳 409
- [ ] 密碼以 Argon2id 雜湊存儲（Salt + Pepper）
- [ ] Creator / Updater = AdminUserId

### 查看 & 編輯
- [ ] `GET /api/admin/members/{memberId}` 回傳會員完整資料
- [ ] `PUT /api/admin/members/{memberId}` 可修改 name, avatar, notification_type, status
- [ ] 不可透過編輯 API 修改 email, password

### 停用 / 啟用
- [ ] 停用帳號（Status = Inactive）後，該用戶無法登入前台

### 重設密碼
- [ ] `PUT /api/admin/members/{memberId}/password` Admin 可重設密碼，不需 old_password
- [ ] 重設密碼後撤銷該用戶所有 UserRefreshToken

### NotificationType
- [ ] NotificationType 新增 `None = 0`，作為預設值
- [ ] 後台建立會員時 NotificationType = None

---

## Boundaries

✅ Always:
- Email 唯一性驗證，不分大小寫
- 密碼必須 Argon2id 雜湊存儲（Salt + Pepper）
- 後台建立的帳號 EmailVerified = true
- 重設密碼後撤銷所有 UserRefreshToken
- Creator / Updater 記錄操作的 AdminUserId

⚠️ Ask First:
- 新增 NotificationType enum 值
- 修改 User Entity 欄位（影響前台會員系統）

🚫 Never:
- 允許後台刪除前台會員
- 允許透過編輯 API 修改 email 或 password
- 在日誌記錄密碼
