# Shared.Web/

跨 API（Admin / User / Upload）共用的 ASP.NET 慣例層：路由 / Query / JSON 命名 transformer、全域 Middleware、Swagger helper。三個 API 專案都引用這層，行為才能對齊。

## 啟動 / 指令

無獨立 entrypoint（class library）。被 Admin.API / User.API / Upload.API 透過 `ProjectReference` 引用，於各自 `Program.cs` 註冊。

## 主要檔案 / 子層

| 路徑 | 角色 |
|------|------|
| `Conventions/SlugifyParameterTransformer.cs` | `RouteTokenTransformerConvention` 用，自動把 PascalCase controller / action 轉 kebab-case URL |
| `Conventions/SnakeCaseQueryValueProviderFactory.cs` | 把 query string `sort_by` / `page_size` 自動綁到 camelCase 參數 |
| `Middleware/GlobalExceptionMiddleware.cs` | Catch 未處理例外，統一包成 `ApiResponse` 回傳 |
| `Middleware/RoleMiddleware.cs` | 後台 Role / Menu CRUD 存取控制（搭配 `RequireRoleAttribute`） |
| `Extensions/ApiBehaviorExtensions.cs` | 把 ModelState 驗證失敗的預設 `ProblemDetails` 改成 `ApiResponse.ValidationFail`，errors key 自動轉 snake_case |
| `Extensions/SwaggerExtensions.cs` | `AddSwagger(title)` helper（dev only 開啟，三 API 共用） |

## 慣用註冊片段（各 API `Program.cs`）

```csharp
builder.Services.AddControllers(options =>
{
  options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
  options.ValueProviderFactories.Insert(0, new SnakeCaseQueryValueProviderFactory());
})
.AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);

builder.Services.ConfigureApiValidationResponse();
builder.Services.AddSwagger("Drink Admin API");

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RoleMiddleware>(); // Admin / User API only，Upload.API 不掛
```

## 依賴與被依賴

- 依賴：`../Application/`（`ApiResponse`、`ErrorCodes` 等）、`../Infrastructure/`、`Microsoft.AspNetCore.App` framework reference、`Swashbuckle.AspNetCore`
- 被依賴：`../Admin.API/`、`../User.API/`、`../Upload.API/`
- **不**被 `../Migrator/` 引用（Migrator 沒有 HTTP pipeline）

## 不要做的事

- 不要把這層當成 Application Service 寫業務邏輯 — 這裡只放 ASP.NET 慣例 / Middleware / Swagger
- 不要在 Controller 或單一 API 重複實作 transformer / middleware，跨 API 共用的一律收進這層
- 不要在 `GlobalExceptionMiddleware` 暴露原始 stack trace 給 production（Serilog 寫 log 即可）
- 不要繞過 `ConfigureApiValidationResponse()` 自寫 ModelState 失敗回傳 — 統一格式由這層處理
- 命名規範（kebab-case route / snake_case JSON / snake_case errors key）詳見 [../../.claude/rules/backend-api.md](../../.claude/rules/backend-api.md)
