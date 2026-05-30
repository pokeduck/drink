# Admin.API/

後台 Admin 用的 HTTP 入口（port `5101`），給 `@drink/admin` Nuxt App 呼叫。負責路由 / 序列化 / JWT 驗證的接線，業務邏輯一律下放給 `../Application/`。

## 啟動 / 指令

```bash
# 開發啟動（預設 Development，啟用 Swagger）
dotnet run --project api/Admin.API

# Swagger UI
http://localhost:5101/swagger
```

- 路由前綴：`api/admin/{controller}`（在 `Controllers/BaseController.cs` 統一掛載）
- Routes / Query / JSON 命名規則由 `Program.cs` 透過 `../Shared.Web/` 套用：kebab-case route、snake_case query、snake_case JSON

## 主要檔案 / 子層

| 路徑 | 角色 |
|------|------|
| `Program.cs` | DI 註冊、Middleware pipeline、Swagger、CORS、JWT、Upload proxy 設定 |
| `Controllers/BaseController.cs` | 所有 Controller 的 base，提供 `ApiOk()` / `ApiError()` / `ApiValidationError()` |
| `Controllers/*.cs` | 每個業務模組一支（`AuthController`、`MenuController`、`OrdersController`…） |
| `Extensions/` | 此 API 專屬擴充（目前空，跨 API 共用請放 `../Shared.Web/Extensions/`） |
| `Swagger/` | 此 API 專屬 Swagger 設定（共用 helper 在 `../Shared.Web/`） |
| `appsettings*.json` | DB 連線、JWT secret、Upload API URL/Key、Serilog |
| `Properties/launchSettings.json` | 本機 5101 port 設定 |

## 依賴與被依賴

- 依賴：`../Application/`、`../Domain/`、`../Infrastructure/`、`../Shared.Web/`
- 被依賴：無（最外層 Presentation）
- JWT scheme：Admin（`access_token` + `refresh_token` rotation），與 `User.API` 完全分開的 secret

## 不要做的事

- 不要在 Controller 寫 EF / LINQ 查詢，全部走 `Application` 的 Service
- 不要在 Controller 做業務驗證，Controller 只負責呼叫 Service 並把結果包成 `ApiOk()` / `ApiError()`
- 不要 new `HttpClient` 呼叫 Upload.API，使用 `IHttpClientFactory` + `UploadApiSettings` 配置
- 不要在 Controller 自寫回傳格式 — 一律 `ApiOk()` / `ApiError()`；錯誤碼定義詳見 [../../.claude/rules/backend-api.md](../../.claude/rules/backend-api.md)
- 不要繞過 `RouteTokenTransformerConvention` 自寫 `[Route("admin/foo-bar")]`，route 一律寫 PascalCase 由 transformer 自動轉 kebab-case
- 不要在這層加 User 端的 endpoint，跨 audience 的 controller 請放對應的 `../User.API/`
