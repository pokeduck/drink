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
| PasswordHash | string | bcrypt 雜湊密碼 |
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
- 修改 AdminMenu 資料表結構（影響現有角色資料）
- 修改 Seed Data（影響初始化邏輯）

🚫 Never:
- 允許刪除 IsSystem = true 的 Role
- 允許刪除 admin 帳號
- 在日誌記錄密碼
- 一個 Staff 指派多個 Role
