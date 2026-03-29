# Spec: 後台會員系統 (Admin Member)

## Objective
- 後台會員分為 Admin 和 Staff 兩種角色
- Admin 為系統預設帳號，擁有全部 Menu 存取權，不可透過後台建立或刪除
- Staff 由 Admin 建立，透過 Role 控制可存取的 Menu 和 CRUD
- Admin 可將任意 Staff 的 Role 升級為 Admin

---

## Entities

### AdminUser
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK，自動遞增 |
| Username | string(50) | 登入帳號，唯一 |
| PasswordHash | string | Argon2id 雜湊密碼（Salt + Pepper） |
| RoleId | int | FK → AdminRole |
| IsActive | bool | 帳號是否啟用 |
| CreatedAt | DateTime | 建立時間（ICreateEntity） |
| Creator | int | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | 更新時間（IUpdateEntity） |
| Updater | int | 更新者 ID（IUpdateEntity） |

### AdminRole
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK，自動遞增 |
| Name | string(50) | 角色名稱（Admin / 自訂） |
| IsSystem | bool | true = Admin 系統角色，不可刪除修改 |
| CreatedAt | DateTime | 建立時間（ICreateEntity） |
| Creator | int | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | 更新時間（IUpdateEntity） |
| Updater | int | 更新者 ID（IUpdateEntity） |

### AdminMenuRole
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK，自動遞增 |
| RoleId | int | FK → AdminRole |
| MenuId | int | FK → AdminMenu.Id |
| CanRead | bool | 可讀取 |
| CanCreate | bool | 可新增 |
| CanUpdate | bool | 可修改 |
| CanDelete | bool | 可刪除 |
| CreatedAt | DateTime | 建立時間（ICreateEntity） |
| Creator | int | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | 更新時間（IUpdateEntity） |
| Updater | int | 更新者 ID（IUpdateEntity） |

> AdminMenu 為獨立資料表（樹狀結構），詳見 [admin-role.md](./admin-role.md)

---

## Relationships
- `AdminUser` → `AdminRole`：多對一（一個 Staff 只能有一個 Role）
- `AdminRole` → `AdminMenuRole`：一對多（一個 Role 對應多個 Menu CRUD 設定）
- `AdminMenuRole` → `AdminMenu`：多對一（MenuId → AdminMenu.Id）

---

## Business Rules

### Admin 帳號
- 系統初始化時 seed 一個預設 Admin 帳號
- Role = Admin（IsSystem = true）
- Admin 帳號不可被刪除
- Admin 的 Role 不可被修改為非 Admin

### Staff 帳號
- 由 Admin 建立
- 建立時必須指派一個 Role
- 一個 Staff 只能有一個 Role

### Role 管理
- Admin Role（IsSystem = true）預設存在，不可刪除或修改
- Admin 可自訂建立新 Role（IsSystem = false）
- 新建立的 Role 預設所有 Menu 的 CRUD 全部關閉
- Admin 可修改自訂 Role 的 Menu CRUD 設定

### 升級角色
- Admin 可將任意 Staff 的 RoleId 改為 Admin Role Id
- 升級後該 Staff 擁有全部 Menu 存取權

---

## Code Style

```csharp
// Entity
public class AdminUser : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(50)]
    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public int RoleId { get; set; }
    public AdminRole Role { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public class AdminRole : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(50)]
    public string Name { get; set; }

    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }

    public ICollection<AdminMenuRole> MenuRoles { get; set; }
}

public class AdminMenuRole : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int RoleId { get; set; }
    public AdminRole Role { get; set; }

    public int MenuId { get; set; }
    public AdminMenu Menu { get; set; }

    public bool CanRead { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}
```

---

## API Endpoints

### 認證

#### 登入
```
POST /api/admin/auth/login
```
- Request Body:
```json
{
  "username": "admin",
  "password": "secret"
}
```
- 驗證 Username + Password（Argon2id）
- IsActive = false 回傳 403
- 成功回傳 access_token（短效 15 分鐘）+ refresh_token（長效 7 天）
- 建立 AdminRefreshToken 記錄
- Response:
```json
{
  "access_token": "eyJhbG...",
  "refresh_token": "dGhpcyBpcyByZWZyZXNo..."
}
```

#### 刷新 Token
```
POST /api/admin/auth/refresh
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
POST /api/admin/auth/logout
```
- Request Body:
```json
{
  "refresh_token": "dGhpcyBpcyByZWZyZXNo..."
}
```
- 撤銷該 refresh_token（標記 RevokedAt）

#### 修改自己密碼
```
PUT /api/admin/auth/password
```
- 需帶 JWT Token（從 Token 取得 UserId）
- Request Body:
```json
{
  "old_password": "current",
  "new_password": "newSecret"
}
```
- old_password 驗證失敗回傳 400（`INVALID_PASSWORD`）
- 成功後撤銷該用戶所有 refresh_token，強制重新登入

---

### 帳號管理

#### 帳號列表
```
GET /api/admin/users?page=1&page_size=20&sort_by=created_at&sort_order=desc&keyword=john&is_active=true&role_id=2
```
- 遵循 SPEC.md 列表通用規範（分頁、排序、搜尋、篩選）
- keyword 搜尋欄位：username
- 篩選條件：is_active, role_id
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "username": "admin",
        "role_id": 1,
        "role_name": "Admin",
        "is_active": true,
        "created_at": "2025-01-01T00:00:00Z"
      }
    ],
    "total": 50,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

#### 取得單一帳號
```
GET /api/admin/users/{userId}
```
- Response:
```json
{
  "data": {
    "id": 2,
    "username": "staff01",
    "role_id": 2,
    "role_name": "Editor",
    "is_active": true,
    "created_at": "2025-01-01T00:00:00Z",
    "updated_at": "2025-01-02T00:00:00Z"
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

#### 建立 Staff
```
POST /api/admin/users
```
- Request Body:
```json
{
  "username": "staff01",
  "password": "initialPassword",
  "role_id": 2,
  "is_active": true
}
```
- username 唯一，重複回傳 409（`USERNAME_ALREADY_EXISTS`）
- role_id 必須存在，否則回傳 400（`ROLE_NOT_FOUND`）

#### 更新 Staff
```
PUT /api/admin/users/{userId}
```
- Request Body:
```json
{
  "role_id": 3,
  "is_active": false
}
```
- admin 帳號的 Role 不可改為非 Admin Role，回傳 403（`CANNOT_CHANGE_ADMIN_ROLE`）
- role_id 必須存在，否則回傳 400（`ROLE_NOT_FOUND`）

#### 重設 Staff 密碼（Admin 操作）
```
PUT /api/admin/users/{userId}/password
```
- Request Body:
```json
{
  "new_password": "resetPassword"
}
```
- 不需 old_password（Admin 強制重設）
- 成功後撤銷該 Staff 所有 refresh_token，強制重新登入

#### 刪除 Staff
```
DELETE /api/admin/users/{userId}
```
- admin 帳號不可刪除，回傳 403（`CANNOT_DELETE_ADMIN`）
- 成功後撤銷該用戶所有 refresh_token

---

## Entities（補充）

### AdminRefreshToken
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK，自動遞增 |
| UserId | int | FK → AdminUser |
| Token | string | Refresh Token 值（唯一） |
| ExpiresAt | DateTime | 過期時間 |
| RevokedAt | DateTime? | 撤銷時間，null = 有效 |
| CreatedAt | DateTime | 建立時間（ICreateEntity） |
| Creator | int | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | 更新時間（IUpdateEntity） |
| Updater | int | 更新者 ID（IUpdateEntity） |

---

## Seed Data

```csharp
// 系統初始化時建立
new AdminRole { Id = 1, Name = "Admin", IsSystem = true }

new AdminUser
{
    Username = "admin",
    PasswordHash = Argon2id.HashPassword("預設密碼"),
    RoleId = 1,
    IsActive = true,
    CreatedAt = DateTime.UtcNow
}
```

---

## Success Criteria

### 帳號
- [ ] Admin 帳號（admin）由 Seed Data 建立，Role = Admin（IsSystem = true），無法被刪除
- [ ] Admin 可建立 Staff 帳號，建立時必須指派一個 Role
- [ ] Staff 只能有一個 Role，RoleId 不可為 null
- [ ] Admin 可修改 Staff 的 RoleId（含升級為 Admin Role）
- [ ] Admin 的 Role 不可被修改為非 Admin
- [ ] 密碼以 Argon2id 雜湊存儲（Salt + Pepper），資料庫無明文密碼
- [ ] AdminUser、AdminRole、AdminMenuRole 皆實作 ICreateEntity / IUpdateEntity
- [ ] 新建 Role 預設所有 Menu CRUD 全部 false
- [ ] Admin Role（IsSystem = true）不可刪除或修改
- [ ] 刪除 Role 時，若底下有 Staff 必須提供 reassign_role_id 遷移，否則回傳 400

### 認證 & Token
- [ ] `POST /api/admin/auth/login` 驗證成功回傳 access_token + refresh_token
- [ ] IsActive = false 的帳號無法登入，回傳 403
- [ ] `POST /api/admin/auth/refresh` 實作 Refresh Token Rotation（舊 token 作廢、發新 token）
- [ ] 重複使用已作廢的 refresh_token 時，撤銷該用戶所有 refresh_token
- [ ] `POST /api/admin/auth/logout` 撤銷指定 refresh_token
- [ ] `PUT /api/admin/auth/password` 修改自己密碼，需驗證 old_password
- [ ] 修改密碼成功後撤銷該用戶所有 refresh_token

### 帳號管理 API
- [ ] `GET /api/admin/users` 支援分頁、排序、keyword 搜 username、篩選 is_active / role_id
- [ ] `POST /api/admin/users` username 唯一，重複回傳 409
- [ ] `PUT /api/admin/users/{userId}` 可修改 role_id、is_active
- [ ] `PUT /api/admin/users/{userId}/password` Admin 可重設 Staff 密碼，不需 old_password
- [ ] 重設密碼後撤銷該 Staff 所有 refresh_token
- [ ] `DELETE /api/admin/users/{userId}` admin 帳號不可刪除，回傳 403

---

## Boundaries

✅ Always:
- Admin Role（IsSystem = true）不可刪除或修改
- Admin 帳號不可刪除
- Staff 建立時必須指派 Role
- 新建 Role 的所有 CRUD 預設為 false
- 密碼必須 Argon2id 雜湊存儲（Salt + Pepper）

⚠️ Ask First:
- 修改 AdminMenu 資料表結構（影響現有角色資料）
- 修改 Seed Data（影響初始化邏輯）

🚫 Never:
- 允許刪除 IsSystem = true 的 Role
- 允許刪除 admin 帳號
- 在日誌記錄密碼
- 一個 Staff 指派多個 Role
