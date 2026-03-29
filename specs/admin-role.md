# Spec: 後台角色管理 (Admin Role)

## Objective
- 提供 Role 的 CRUD 管理，以及各 Menu 的 CRUD 設定
- AdminMenu 為獨立資料表，支援樹狀結構（parent-child），層級不限
- 新增 Role 時同時設定可存取的 Menu 與 CRUD，後續也可單獨編輯
- 角色設定修改即時生效，已登入 Staff 下次 API call 立即套用
- 刪除 Role 時，必須先將底下 Staff 重新指派到其他 Role，RoleId 不可為空
- 提供 API 依據 user token 回傳可存取的 Menu 樹狀結構 + CRUD
- 所有 Menu（含角色管理）統一透過 AdminMenuRole 控制，不再硬編碼 Admin 專屬
- 後端透過 Middleware 攔截 API 請求，依據 Role 的 Menu CRUD 進行存取檢查

---

## Entities

### AdminMenu
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK，自動遞增 |
| ParentId | int? | FK → AdminMenu.Id，null = 頂層 |
| Name | string(50) | Menu 名稱 |
| Icon | string(50)? | Icon 名稱（Element Plus icon） |
| Endpoint | string(200)? | 前端路由路徑，群組節點為 null |
| Sort | int | 排序（同層級內由小到大） |
| CreatedAt | DateTime | 建立時間（ICreateEntity） |
| Creator | int | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | 更新時間（IUpdateEntity） |
| Updater | int | 更新者 ID（IUpdateEntity） |

### AdminMenuRole（異動）
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK，自動遞增 |
| RoleId | int | FK → AdminRole |
| MenuId | int | FK → AdminMenu.Id（原 enum 改為 FK） |
| CanRead | bool | 可讀取 |
| CanCreate | bool | 可新增 |
| CanUpdate | bool | 可修改 |
| CanDelete | bool | 可刪除 |
| CreatedAt | DateTime | 建立時間（ICreateEntity） |
| Creator | int | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | 更新時間（IUpdateEntity） |
| Updater | int | 更新者 ID（IUpdateEntity） |

### Relationships
- `AdminMenu` → `AdminMenu`：自關聯（ParentId → Id），支援多層樹狀結構
- `AdminMenuRole` → `AdminMenu`：多對一（MenuId → AdminMenu.Id）

---

## Code Style

```csharp
public class AdminMenu : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int? ParentId { get; set; }
    public AdminMenu? Parent { get; set; }

    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    [StringLength(200)]
    public string? Endpoint { get; set; }

    public int Sort { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }

    public ICollection<AdminMenu> Children { get; set; }
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

### User Menu（依 Token 回傳）

#### 取得當前使用者可存取的 Menu
```
GET /api/admin/menus/me
```
- 從 JWT Token 取得 RoleId
- 後端僅回傳該 Role 可存取的 Menu（至少一個 CRUD 為 true），不可存取的 Menu 不會出現在回傳結果中
- 回傳樹狀結構，不包含 CRUD 細節，前端收到即全部渲染
- 若父節點底下所有子節點都無存取權，則不回傳該父節點

#### Response
```json
[
  {
    "id": 1,
    "name": "後台帳號管理",
    "icon": "UserFilled",
    "endpoint": null,
    "sort": 1,
    "children": [
      {
        "id": 2,
        "name": "帳號列表",
        "icon": "Avatar",
        "endpoint": "/admin-account/list",
        "sort": 1,
        "children": []
      },
      {
        "id": 3,
        "name": "角色管理",
        "icon": "Lock",
        "endpoint": "/admin-account/role",
        "sort": 2,
        "children": []
      }
    ]
  }
]
```

---

### Role CRUD

#### 取得 Role 列表
```
GET /api/admin/roles
```
- Response: 所有 Role（含 IsSystem 標記、Staff 數量）

#### 取得單一 Role（含 Menu CRUD）
```
GET /api/admin/roles/{roleId}
```
- Response: Role 資訊 + 所有 Menu 的 CRUD 設定

#### 新增 Role（含 Menu CRUD）
```
POST /api/admin/roles
```
- Request Body: Role 名稱 + 各 Menu 的 CRUD 設定
- 未提供的 Menu 預設 CRUD 全部 false

#### 更新 Role（含 Menu CRUD）
```
PUT /api/admin/roles/{roleId}
```
- Request Body: Role 名稱 + 各 Menu 的 CRUD 設定（整批覆蓋）
- 不可對 IsSystem = true 的 Role 執行更新，回傳 403

#### 刪除 Role
```
DELETE /api/admin/roles/{roleId}
```
- 不可刪除 IsSystem = true 的 Role，回傳 403
- Request Body: 指定要將底下 Staff 遷移到的目標 RoleId
- 若該 Role 底下有 Staff，必須提供 reassign_role_id，否則回傳 400
- 若該 Role 底下無 Staff，可直接刪除（不需 reassign_role_id）

---

## Request / Response

### GET /api/admin/roles Response
```json
[
  {
    "id": 1,
    "name": "Admin",
    "is_system": true,
    "staff_count": 1
  },
  {
    "id": 2,
    "name": "Editor",
    "is_system": false,
    "staff_count": 3
  }
]
```

### GET /api/admin/roles/{roleId} Response
```json
{
  "id": 2,
  "name": "Editor",
  "is_system": false,
  "menus": [
    {
      "menu_id": 2,
      "menu_name": "帳號列表",
      "can_read": false,
      "can_create": false,
      "can_update": false,
      "can_delete": false
    },
    {
      "menu_id": 3,
      "menu_name": "角色管理",
      "can_read": true,
      "can_create": true,
      "can_update": true,
      "can_delete": false
    }
  ]
}
```

### POST /api/admin/roles Request
```json
{
  "name": "Editor",
  "menus": [
    {
      "menu_id": 2,
      "can_read": false,
      "can_create": false,
      "can_update": false,
      "can_delete": false
    },
    {
      "menu_id": 3,
      "can_read": true,
      "can_create": true,
      "can_update": true,
      "can_delete": true
    }
  ]
}
```

### PUT /api/admin/roles/{roleId} Request
```json
{
  "name": "Editor",
  "menus": [
    {
      "menu_id": 2,
      "can_read": false,
      "can_create": false,
      "can_update": false,
      "can_delete": false
    },
    {
      "menu_id": 3,
      "can_read": true,
      "can_create": true,
      "can_update": true,
      "can_delete": true
    }
  ]
}
```

### DELETE /api/admin/roles/{roleId} Request
```json
{
  "reassign_role_id": 3
}
```
- reassign_role_id: 將底下 Staff 遷移到的目標 Role
- 若該 Role 無 Staff，body 可為空

---

## Role Middleware

### 機制
- 註冊全域 Middleware，在每個需要角色檢查的 API 請求前執行
- 從 JWT Token 取得 RoleId
- 查詢該 Role 的 AdminMenuRole（不做快取，確保即時生效）
- 比對當前請求所需的 MenuId + CRUD
- 無存取權回傳 403 Forbidden

### Controller 標記方式
```csharp
// 以 MenuId 常數標記
[RequireRole(MenuConstants.Order, CrudAction.Read)]
[HttpGet("/api/admin/orders")]
public IActionResult GetOrders() { ... }

[RequireRole(MenuConstants.Order, CrudAction.Create)]
[HttpPost("/api/admin/orders")]
public IActionResult CreateOrder() { ... }
```

### Attribute 定義
```csharp
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireRoleAttribute : Attribute
{
    public int MenuId { get; }
    public CrudAction Action { get; }

    public RequireRoleAttribute(int menuId, CrudAction action)
    {
        MenuId = menuId;
        Action = action;
    }
}

public enum CrudAction
{
    Read,
    Create,
    Update,
    Delete
}

/// <summary>
/// Menu ID 常數，對應 AdminMenu 資料表的 Seed Data
/// </summary>
public static class MenuConstants
{
    // 後台帳號管理
    public const int AdminAccountList = 2;
    public const int AdminRole = 3;

    // 會員管理
    public const int MemberList = 5;
    public const int MemberVerification = 6;

    // 訂單管理
    public const int OrderList = 8;

    // 店家管理
    public const int ShopList = 10;

    // 飲料選項
    public const int DrinkItem = 12;
    public const int Sugar = 13;
    public const int Ice = 14;
    public const int Topping = 15;
    public const int Size = 16;

    // 店家覆寫設定
    public const int ShopOverride = 17;

    // 通知管理
    public const int NotificationList = 19;
    public const int NotificationByGroup = 20;
}
```

### Middleware 實作
```csharp
public class RoleMiddleware
{
    private readonly RequestDelegate _next;

    public RoleMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var endpoint = context.GetEndpoint();
        var attr = endpoint?.Metadata.GetMetadata<RequireRoleAttribute>();

        if (attr is null)
        {
            await _next(context);
            return;
        }

        var roleId = int.Parse(context.User.FindFirst("RoleId")!.Value);

        var menuRole = await db.AdminMenuRoles
            .FirstOrDefaultAsync(p => p.RoleId == roleId && p.MenuId == attr.MenuId);

        var hasAccess = attr.Action switch
        {
            CrudAction.Read => menuRole?.CanRead ?? false,
            CrudAction.Create => menuRole?.CanCreate ?? false,
            CrudAction.Update => menuRole?.CanUpdate ?? false,
            CrudAction.Delete => menuRole?.CanDelete ?? false,
            _ => false
        };

        if (!hasAccess)
        {
            context.Response.StatusCode = 403;
            return;
        }

        await _next(context);
    }
}
```

---

## Seed Data

```csharp
// AdminMenu 樹狀結構
// 注意：/dashboard 為人人可看的首頁，不納入 AdminMenu 角色控制

// 後台帳號管理
new AdminMenu { Id = 1,  ParentId = null, Name = "後台帳號管理", Icon = "UserFilled",  Endpoint = null,                    Sort = 1 },
new AdminMenu { Id = 2,  ParentId = 1,    Name = "帳號列表",     Icon = "Avatar",      Endpoint = "/admin-account/list",   Sort = 1 },
new AdminMenu { Id = 3,  ParentId = 1,    Name = "角色管理", Icon = "Lock",        Endpoint = "/admin-account/role",   Sort = 2 },

// 會員管理
new AdminMenu { Id = 4,  ParentId = null, Name = "會員管理",   Icon = "User",        Endpoint = null,                    Sort = 2 },
new AdminMenu { Id = 5,  ParentId = 4,    Name = "會員列表",   Icon = "List",        Endpoint = "/member/list",          Sort = 1 },
new AdminMenu { Id = 6,  ParentId = 4,    Name = "驗證信列表", Icon = "Message",     Endpoint = "/member/verification",  Sort = 2 },

// 訂單管理
new AdminMenu { Id = 7,  ParentId = null, Name = "訂單管理", Icon = "Document",     Endpoint = null,                    Sort = 3 },
new AdminMenu { Id = 8,  ParentId = 7,    Name = "訂單列表", Icon = "DocumentCopy", Endpoint = "/order/list",           Sort = 1 },

// 店家管理
new AdminMenu { Id = 9,  ParentId = null, Name = "店家管理", Icon = "Shop",         Endpoint = null,                    Sort = 4 },
new AdminMenu { Id = 10, ParentId = 9,    Name = "店家列表", Icon = "Store",        Endpoint = "/shop/list",            Sort = 1 },

// 飲料選項
new AdminMenu { Id = 11, ParentId = null, Name = "飲料選項", Icon = "ColdDrink",    Endpoint = null,                    Sort = 5 },
new AdminMenu { Id = 12, ParentId = 11,   Name = "通用品名", Icon = "Grape",        Endpoint = "/drink-option/item",    Sort = 1 },
new AdminMenu { Id = 13, ParentId = 11,   Name = "甜度定義", Icon = "Sugar",        Endpoint = "/drink-option/sugar",   Sort = 2 },
new AdminMenu { Id = 14, ParentId = 11,   Name = "冰塊定義", Icon = "IceCream",     Endpoint = "/drink-option/ice",     Sort = 3 },
new AdminMenu { Id = 15, ParentId = 11,   Name = "加料",     Icon = "Plus",         Endpoint = "/drink-option/topping", Sort = 4 },
new AdminMenu { Id = 16, ParentId = 11,   Name = "容量定義", Icon = "CoffeeCup",    Endpoint = "/drink-option/size",    Sort = 5 },

// 店家覆寫設定（從 ShopList 拆分）
new AdminMenu { Id = 17, ParentId = 9,    Name = "覆寫設定", Icon = "Setting",      Endpoint = "/shop/override",        Sort = 2 },

// 通知管理
new AdminMenu { Id = 18, ParentId = null, Name = "通知管理", Icon = "Bell",         Endpoint = null,                        Sort = 6 },
new AdminMenu { Id = 19, ParentId = 18,   Name = "通知列表", Icon = "ChatDotRound", Endpoint = "/notification/list",        Sort = 1 },
new AdminMenu { Id = 20, ParentId = 18,   Name = "揪團通知", Icon = "ChatLineRound",Endpoint = "/notification/by-group",    Sort = 2 },
```

---

## Frontend（Admin）

### Sidebar
- 登入後呼叫 `GET /api/admin/menus/me` 取得 Menu 樹
- 後端回傳的 Menu 即為該使用者可存取的全部項目，前端全部渲染，不需額外判斷顯示/隱藏
- 依據回傳的樹狀結構動態生成 el-menu sidebar
- 群組節點（endpoint = null）為 el-sub-menu
- 葉節點（有 endpoint）為 el-menu-item，點擊導航至對應路由

### 頁面：Role 列表
- 路徑：`/admin-account/role`
- el-table 顯示所有 Role（名稱、IsSystem 標記、Staff 數量）
- 操作欄：編輯、刪除（IsSystem Role 隱藏刪除按鈕）
- 新增 Role 按鈕

### 頁面：新增 / 編輯 Role
- 路徑：`/admin-account/role/create`、`/admin-account/role/:roleId/edit`
- 表單欄位：
  - Role 名稱（el-input）
  - Menu CRUD 矩陣表格（el-table）
    - Row = AdminMenu 葉節點（有 endpoint 的 Menu）
    - Column = CanRead / CanCreate / CanUpdate / CanDelete
    - Cell = el-checkbox
- IsSystem = true 的 Role：名稱和所有 checkbox disabled，顯示提示文字
- 儲存按鈕：POST（新增）或 PUT（編輯）

### 刪除確認
- el-dialog 確認對話框
- 若該 Role 底下有 Staff：顯示 Staff 數量，並提供 el-select 選擇要遷移到的目標 Role
- 若該 Role 底下無 Staff：僅顯示確認刪除提示

---

## Business Rules

### 存取控制
- 角色管理頁面（`/admin-account/role`）透過 AdminMenuRole 控制，與其他 Menu 一致
- `GET /api/admin/menus/me` 所有已登入使用者皆可存取
- `/dashboard` 為公開頁面，所有已登入使用者皆可存取，不納入 AdminMenu 角色控制

### Role 管理
- Role 名稱必須唯一，重複回傳 409 Conflict
- IsSystem = true 的 Role 不可修改、不可刪除
- 刪除 Role 時，若底下有 Staff 必須指定 reassign_role_id 將 Staff 遷移到其他 Role
- RoleId 不可為 null，Staff 必定有一個 Role

### 角色 Menu 設定
- 新增 Role 時同時設定可存取的 Menu CRUD
- PUT 整批覆蓋所有 Menu CRUD（不做 partial update）
- POST/PUT 未提供的 Menu 預設 CRUD 全部 false
- AdminMenu 資料表新增項目時，需同步為所有既有 Role 補建 AdminMenuRole 記錄（預設 false）

### Menu 樹狀結構
- ParentId = null 為頂層群組
- 群組節點 Endpoint 為 null，不設定 CRUD
- 僅葉節點（有 Endpoint）需設定 CRUD
- 排序以 Sort 欄位控制，同層級內由小到大

---

## Success Criteria

- [ ] `GET /api/admin/menus/me` 依 Token RoleId 回傳該 Role 可存取的 Menu 樹狀結構，不含 CRUD 欄位
- [ ] 父節點底下所有子節點都無存取權時，不回傳該父節點
- [ ] `GET /api/admin/roles` 回傳所有 Role，含 IsSystem 標記與 Staff 數量
- [ ] `GET /api/admin/roles/{roleId}` 回傳該 Role 的所有 Menu CRUD 設定
- [ ] `POST /api/admin/roles` 建立 Role 並同時設定 Menu CRUD，未提供的 Menu 預設全 false
- [ ] `PUT /api/admin/roles/{roleId}` 整批覆蓋 Menu CRUD，不支援 partial update
- [ ] IsSystem = true 的 Role 不可 PUT / DELETE，回傳 403
- [ ] `DELETE /api/admin/roles/{roleId}` 有 Staff 時必須提供 reassign_role_id，否則回傳 400
- [ ] Role 名稱唯一，重複回傳 409
- [ ] RoleMiddleware 每次 API 請求即時查 DB，無快取
- [ ] Controller 透過 `[RequireRole(MenuId, CrudAction)]` 標記，Middleware 比對後無權回傳 403
- [ ] AdminMenu 為自關聯樹狀結構，僅葉節點設定 CRUD，群組節點不設定
- [ ] 前端 Sidebar 收到 menus/me 回傳即全部渲染，不做額外判斷
- [ ] 前端 Role 編輯頁以 Menu CRUD 矩陣表格呈現，IsSystem Role 全部 disabled

---

## Boundaries

✅ Always:
- 每次 API 請求即時查 DB 角色設定，不做快取
- POST/PUT 整批處理所有 Menu CRUD
- IsSystem Role 不可修改、不可刪除
- 所有 Menu（含角色管理）統一透過 AdminMenuRole 控制存取
- 刪除 Role 時必須指定 reassign_role_id 遷移 Staff
- RoleId 不可為 null
- Menu 樹狀結構支援多層，不限層級

⚠️ Ask First:
- 新增 AdminMenu 資料（需同步補建所有 Role 的 AdminMenuRole）
- 變更 Middleware 攔截策略（影響全域 API 存取）
- 調整 Seed Data 的 Menu 結構

🚫 Never:
- 快取角色資料（會導致角色修改延遲生效）
- 允許 partial update（只傳部分 Menu CRUD）
- 允許修改或刪除 IsSystem = true 的 Role
- 允許無存取權的使用者存取受保護的 Menu
- 允許 RoleId 為 null（Staff 必定有 Role）
- 對群組節點（無 Endpoint）設定 CRUD
