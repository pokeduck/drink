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

// Entity 繼承 BaseDataEntity
public class Account : BaseDataEntity
{
    [StringLength(100)]
    public string Name { get; set; }
}
```

## Git Workflow
- Branch: `feat/<feature>`, `fix/<issue>`, `chore/<task>`
- Commit: `feat(scope): description`
- 每個功能一個 PR

## Boundaries

✅ Always:
- Entity 必須繼承 `BaseDataEntity`
- Service 必須繼承 `BaseService`
- Controller 必須繼承 `BaseController`
- 密碼必須雜湊存儲，絕不明文
- 新增 Entity 後必須建立 Migration

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
