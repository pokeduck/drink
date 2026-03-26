# Project Spec: Drink API

## Objective
揪團訂飲料平台，讓用戶可以發起飲料揪團，朋友/同事加入後一起訂購。

**核心功能：**

前台：
- 用戶可自行建立商家與菜單（公開給所有人使用）
- 用戶可發起揪團，選擇商家，設定截止時間
- 其他用戶可加入揪團並選擇飲料
- 所有參與者可查看訂單狀態
- 發起人可匯出 Excel
- 飲料送達後，發起人可發送通知（Web Push 或 Email，用戶自選）

訂單狀態：
- 揪團進行中
- 揪團截止
- 飲料已送達
- 已結束

後台：
- 管理員管理平台整體運營（後台會員、前台會員、店家、訂單、Global 選項、系統設定）

## Tech Stack

**Monorepo**
- pnpm + Turborepo

**前台 (Client)** — `web/apps/client`
- Nuxt 4, Vue, Nuxt UI

**後台 (Admin)** — `web/apps/admin`
- Nuxt 4, Vue, Element Plus

**前端共用邏輯** — `web/internal`
- 通用工具（dateTimeFormatter 等）
- 共用 Response Interface

**後台 API** — `api/Admin.API`
- .NET 10, C#, ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL
- JWT Authentication

**前台 API** — `api/User.API`
- .NET 10, C#, ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL
- JWT Authentication

## Commands
- Build: `dotnet build`
- Run: `dotnet run --project api/User.API`
- Test: `dotnet test`
- Migration Add: `dotnet ef migrations add <Name> --project api/Infrastructure --startup-project api/Migrator`
- Migration Run: `dotnet ef database update --project api/Infrastructure --startup-project api/Migrator`

## Project Structure
- `api/Domain/` – Entities, Enums, Interfaces
- `api/Application/` – Services, Requests, Responses, Mappings
- `api/Infrastructure/` – DbContext, Migrations, EF Extensions
- `api/User.API/` – User-facing API controllers
- `api/Admin.API/` – Admin-facing API controllers
- `api/Migrator/` – Migration runner

## Code Style

```csharp
// Controller 繼承 BaseController，使用 async/await
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
}

// Service 繼承 BaseService
public class AuthService : BaseService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // 實現邏輯
    }
}

// Entity 繼承 BaseDataEntity，並實作 ICreateEntity / IUpdateEntity
public class Account : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(100)]
    public string Name { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

// 時間戳 Interface
public interface ICreateEntity
{
    DateTime CreatedAt { get; set; }
    int Creator { get; set; }
}

public interface IUpdateEntity
{
    DateTime UpdatedAt { get; set; }
    int Updater { get; set; }
}
```

## Global Rules

### 資料庫正規化
- 所有資料表嚴格遵守第三正規化（3NF）
- 不允許冗餘欄位、不允許傳遞相依
- 所有資料表必須實作 `ICreateEntity`（CreatedAt, Creator）與 `IUpdateEntity`（UpdatedAt, Updater）

### API 通用規範
- 前端統一使用 JSON Body 傳送資料
- API Request / Response 的 JSON key 統一使用 **snake_case**
- 後端透過 `JsonSerializerOptions` 設定 `PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower` 全域轉換

### 列表通用功能
所有列表 API 皆須支援以下功能：

| 功能 | 參數 | 說明 |
|------|------|------|
| 分頁 | `page`, `page_size` | 預設 `page=1`, `page_size=20` |
| 排序 | `sort_by`, `sort_order` | `sort_by` 為欄位名稱（snake_case），`sort_order` 為 `asc` 或 `desc`，預設依建立時間降冪 |
| 搜尋 | `keyword` | 模糊搜尋，各列表自行定義搜尋欄位（如 name, username） |
| 篩選 | 依業務定義 | 各列表自行定義篩選條件（如 `is_active`, `role_id`），以 query string 傳遞 |

- 所有列表參數統一透過 **Query String** 傳遞
- 後端需對 `sort_by` 做白名單驗證，避免任意欄位排序造成效能問題

#### 列表 Request 範例
```
GET /api/admin/users?page=1&page_size=20&sort_by=created_at&sort_order=desc&keyword=john&is_active=true
```

#### 列表 Response 範例
```json
{
  "data": {
    "items": [],
    "total": 100,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

---

### API Response 格式
所有 API 統一回傳格式：
```json
{
  "data": {},
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

| 欄位 | 型別 | 說明 |
|------|------|------|
| data | object / array / null | 回傳資料，無資料時為 null |
| message | string? | 錯誤訊息，成功時為 null |
| code | string | 自訂業務代碼（如 `SUCCESS`, `ROLE_NOT_FOUND`, `VALIDATION_ERROR`） |
| errors | Record<string, string[]>? | 欄位驗證錯誤，非驗證錯誤時為 null |

#### 成功回傳
```json
{
  "data": { "id": 1, "name": "Admin" },
  "message": null,
  "code": "SUCCESS",
  "errors": null

}
```

#### 業務錯誤
```json
{
  "data": null,
  "message": "角色名稱已存在",
  "code": "ROLE_ALREADY_EXISTS",
  "errors": null
}
```

#### 欄位驗證錯誤
```json
{
  "data": null,
  "message": "輸入驗證失敗",
  "code": "VALIDATION_ERROR",
  "errors": {
    "name": ["角色名稱為必填"],
    "permissions[0].menu_id": ["Menu 不存在"]
  }
}
```
- `errors` 的 key 為 snake_case，對應 Request JSON key
- 前端表單的 prop 與 JSON key 一致（snake_case），可直接將 errors 對應到欄位顯示
- 對應鏈路：`前端 JSON key (snake_case) → 後端 DTO property (PascalCase，自動轉換) → ModelState key (snake_case) → errors key → 前端 form prop`

---

## Git Workflow
- Branch: `feat/<feature>`, `fix/<issue>`, `chore/<task>`
- Commit: `feat(scope): description`
- 每個功能一個 PR

## Boundaries

✅ Always:
- 所有 Entity 必須實作 `ICreateEntity` 與 `IUpdateEntity`
- Entity 必須繼承 `BaseDataEntity`
- Service 必須繼承 `BaseService`
- Controller 必須繼承 `BaseController`
- 密碼必須雜湊存儲，絕不明文
- 新增 Entity 後必須建立 Migration
- 資料表嚴格遵守第三正規化（3NF）
- API 回傳格式統一使用 `{ data, message, code }`
- 所有列表 API 必須支援分頁、排序、搜尋、篩選

⚠️ Ask First:
- 修改現有 Entity 欄位（影響 Migration）
- 修改 BaseDataEntity / BaseService / BaseController
- 引入新的 NuGet 套件
- 修改 DbContext 配置

🚫 Never:
- 在日誌記錄密碼或 Token
- 提交 secrets 或 API keys
- 直接修改已執行的 Migration 文件
- 跳過輸入驗證
