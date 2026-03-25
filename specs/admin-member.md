# Spec: 後台會員系統 (Admin Member)

## Objective
- 後台會員分為 Admin 和 Staff 兩種角色
- Admin 為系統預設帳號，擁有全部權限，不可透過後台建立或刪除
- Staff 由 Admin 建立，透過 Role 控制可存取的 Menu 和 CRUD 權限
- Admin 可將任意 Staff 的 Role 升級為 Admin

---

## Entities

### AdminUser
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK，自動遞增 |
| Username | string(50) | 登入帳號，唯一 |
| PasswordHash | string | bcrypt 雜湊密碼 |
| RoleId | int | FK → AdminRole |
| IsActive | bool | 帳號是否啟用 |
| CreatedAt | DateTime | 建立時間 |

### AdminRole
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK，自動遞增 |
| Name | string(50) | 角色名稱（Admin / 自訂） |
| IsSystem | bool | true = Admin 系統角色，不可刪除修改 |
| CreatedAt | DateTime | 建立時間 |

### AdminRolePermission
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK，自動遞增 |
| RoleId | int | FK → AdminRole |
| Menu | AdminMenu (enum) | 對應的後台 Menu |
| CanRead | bool | 讀取權限 |
| CanCreate | bool | 新增權限 |
| CanUpdate | bool | 修改權限 |
| CanDelete | bool | 刪除權限 |

### AdminMenu (Enum)
```csharp
public enum AdminMenu
{
    AdminMember = 1,    // 後台會員管理
    UserMember = 2,     // 前台會員管理
    Store = 3,          // 店家管理
    Order = 4,          // 訂單管理
    GlobalOption = 5,   // Global 選項管理
    SystemSetting = 6   // 系統設定
}
```

---

## Relationships
- `AdminUser` → `AdminRole`：多對一（一個 Staff 只能有一個 Role）
- `AdminRole` → `AdminRolePermission`：一對多（一個 Role 有多個 Menu 權限）

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
- Admin Role（IsSystem = true）預設存在，不可刪除或修改權限
- Admin 可自訂建立新 Role（IsSystem = false）
- 新建立的 Role 預設所有 Menu 的 CRUD 全部關閉
- Admin 可修改自訂 Role 的權限

### 升級權限
- Admin 可將任意 Staff 的 RoleId 改為 Admin Role Id
- 升級後該 Staff 擁有全部權限

---

## Code Style

```csharp
// Entity
public class AdminUser : BaseDataEntity
{
    [StringLength(50)]
    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public int RoleId { get; set; }
    public AdminRole Role { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminRole : BaseDataEntity
{
    [StringLength(50)]
    public string Name { get; set; }

    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<AdminRolePermission> Permissions { get; set; }
}

public class AdminRolePermission : BaseDataEntity
{
    public int RoleId { get; set; }
    public AdminRole Role { get; set; }

    public AdminMenu Menu { get; set; }

    public bool CanRead { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}
```

---

## Seed Data

```csharp
// 系統初始化時建立
new AdminRole { Id = 1, Name = "Admin", IsSystem = true }

new AdminUser
{
    Username = "admin",
    PasswordHash = BCrypt.HashPassword("預設密碼"),
    RoleId = 1,
    IsActive = true,
    CreatedAt = DateTime.UtcNow
}
```

---

## Boundaries

✅ Always:
- Admin Role（IsSystem = true）不可刪除或修改
- Admin 帳號不可刪除
- Staff 建立時必須指派 Role
- 新建 Role 的所有 CRUD 預設為 false
- 密碼必須 bcrypt 雜湊存儲

⚠️ Ask First:
- 修改 AdminMenu enum（影響現有權限資料）
- 修改 Seed Data（影響初始化邏輯）

🚫 Never:
- 允許刪除 IsSystem = true 的 Role
- 允許刪除 admin 帳號
- 在日誌記錄密碼
- 一個 Staff 指派多個 Role
