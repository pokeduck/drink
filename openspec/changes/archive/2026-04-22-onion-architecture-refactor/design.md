## Context

目前 project reference 方向：`Application → Infrastructure → Domain`。Application 直接引用 Infrastructure，導致業務邏輯層知道資料庫實作細節。此外 HTTP 層的 Middleware / Conventions / Swagger 放在 Application 裡，IGenericRepository 介面和實作混在同一個 Infrastructure 專案。

目標依賴方向：

```
            API
           ╱   ╲
          ▼     ▼
Infrastructure → Application → Domain
```

## Goals / Non-Goals

**Goals:**
- 翻轉 Application ↔ Infrastructure 的 project reference 方向
- 將跨層邊界介面（Repository、JWT、CurrentUser、FileStorage）搬到 Application
- 將 HTTP 專屬成員（Middleware、Conventions、Swagger）搬到 API 層
- BaseService 改用 constructor injection 取代 Service Locator
- 抽出 `ICurrentUserContext`，移除 Service/Repository 直接讀取 `IHttpContextAccessor`

**Non-Goals:**
- 不替每個 Service 加介面（同層內部不需要跨邊界介面）
- 不改 Scrutor 的 AsSelf() 註冊方式（Service 之間直接用 concrete class）
- 不改前端
- 不做 FluentValidation 或其他驗證框架整合
- 不加併發控制（RowVersion）

## Decisions

### 1. IGenericRepository 介面保留 EF Core 型別依賴

**選擇**：Application 層的 `IGenericRepository<T>` 介面繼續引用 EF Core 型別（`IQueryable`、`IIncludableQueryable`、`DbSet` 等）

**替代方案**：抽象掉所有 EF Core 型別，改用自定義 wrapper

**理由**：完全抽象 EF Core 需要大量 wrapper 類別（自訂 IQueryable wrapper、自訂 Include 機制、自訂分頁型別），工程量極大且收益不高——專案不會換 ORM。務實做法是 Application.csproj 加一個 `Microsoft.EntityFrameworkCore` package reference（僅抽象層，不含 provider），讓介面可以使用 EF Core 的型別定義。

### 2. PaginationList 搬到 Application

**選擇**：將 `PaginationList<T>` 類別從 `Infrastructure.Extensions` 搬到 `Application`（如 `Application/Models/PaginationList.cs`），因為它是一個純資料結構，不依賴 Infrastructure。

**理由**：介面 `IGenericRepository` 搬到 Application 後，它引用的 `PaginationList<T>` 也必須在 Application 或更內層。`PaginationList` 只是一個 POCO，搬過去零成本。`ToPaginationList()` extension method 留在 Infrastructure（它用了 EF Core 的 `CountAsync` / `ToListAsync`）。

### 3. Middleware / Conventions 放到共用 library 或各 API 專案

**選擇**：建立一個 `Drink.Shared.Web` 共用專案放 Middleware、Conventions、Swagger，三個 API 專案 reference 它。

**替代方案 A**：每個 API 專案各自複製一份 — 違反 DRY。
**替代方案 B**：只放在 Admin.API 其他引用 — 專案間不該互相 reference。

**理由**：`GlobalExceptionMiddleware`、`RoleMiddleware`、`SlugifyParameterTransformer`、`SnakeCaseQueryValueProviderFactory`、`SwaggerExtensions` 這些是 HTTP 層共用基礎設施，不該在 Application 但也不需要三份。新建 `Shared.Web` 專案是最乾淨的做法。

```
Shared.Web
├── Middleware/
│   ├── GlobalExceptionMiddleware.cs
│   └── RoleMiddleware.cs
├── Conventions/
│   ├── SlugifyParameterTransformer.cs
│   └── SnakeCaseQueryValueProviderFactory.cs
└── Extensions/
    └── SwaggerExtensions.cs
```

### 4. ICurrentUserContext 設計

**選擇**：在 Application 層定義 `ICurrentUserContext`，Infrastructure 實作 `HttpCurrentUserContext`。

```csharp
// Application/Interfaces/ICurrentUserContext.cs
public interface ICurrentUserContext
{
    int UserId { get; }
}

// Infrastructure/Services/HttpCurrentUserContext.cs
public class HttpCurrentUserContext : ICurrentUserContext
{
    public int UserId { get; }
    public HttpCurrentUserContext(IHttpContextAccessor accessor)
    {
        var claim = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        UserId = int.TryParse(claim, out var id) ? id : 0;
    }
}
```

**理由**：BaseService 和 GenericRepository 都用到 CurrentUserId，但取法完全一樣（從 JWT claim 讀）。抽成介面後兩處共用同一份邏輯，未來 background job 可以注入不同實作（如 `SystemUserContext`）。

### 5. BaseService 改 constructor injection

**選擇**：BaseService 改為注入 `ICurrentUserContext`，移除 `IServiceProvider`。各 Service 在自己的 constructor 注入需要的 `IGenericRepository<T>`。

```csharp
// 改前
public class AdminUserService : BaseService
{
    public AdminUserService(IServiceProvider sp) : base(sp) { }
    
    public async Task DoSomething()
    {
        var repo = GetRepository<AdminUser>();  // Service Locator
    }
}

// 改後
public class AdminUserService : BaseService
{
    private readonly IGenericRepository<AdminUser> _userRepo;
    private readonly IGenericRepository<AdminRole> _roleRepo;
    
    public AdminUserService(
        ICurrentUserContext currentUser,
        IGenericRepository<AdminUser> userRepo,
        IGenericRepository<AdminRole> roleRepo) : base(currentUser)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }
}
```

**理由**：constructor 明確列出依賴，不再隱藏。Scrutor AsSelf() 搭配 .NET DI 的 auto-resolution 會自動注入所有 constructor 參數。

### 6. DI Registration 調整

Scrutor 繼續用 `AsSelf()` 掃描 Service。但 `AddApplicationServices()` 需要從 Application 搬到 Shared.Web 或 Infrastructure 的 extension method 裡（因為 Application 不再知道 Infrastructure，無法在 Application 層完成完整的 DI wiring）。

實際上 `AddApplicationServices()` 只掃描 `BaseService` 子類別，不依賴 Infrastructure，所以可以留在 Application。Infrastructure 的 DI registration（Repository、JWT、DbContext）留在 Infrastructure。API 的 `Program.cs` 負責呼叫兩邊的 registration。

## Risks / Trade-offs

- **[每個 Service 的 constructor 都要改]** → 機械性工作，但量大（約 8 個 Service）。逐一改即可，不會有遺漏風險因為編譯會報錯。
- **[Application 引用 EF Core 抽象]** → 不是完美的 Onion（purist 觀點），但務實。Application 只引用 `Microsoft.EntityFrameworkCore`（抽象層），不引用 `Npgsql.EntityFrameworkCore.PostgreSQL`（provider）。
- **[新增 Shared.Web 專案]** → 多一個專案，但避免三份重複和職責錯放。
- **[RoleMiddleware 依賴 Repository]** → 搬到 API 層後，RoleMiddleware 需要透過 DI 取得 Repository。目前它已經是這樣做的（透過 `HttpContext.RequestServices`），搬移後行為不變。
