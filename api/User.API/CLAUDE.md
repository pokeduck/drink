# User.API/

前台 Client 用的 HTTP 入口（port `5102`），給 `@drink/client` Nuxt App 呼叫。負責會員自身的認證、個人資料、揪團與訂單操作，業務邏輯下放給 `../Application/`。

## 啟動 / 指令

```bash
# 開發啟動（預設 Development，啟用 Swagger）
dotnet run --project api/User.API

# Swagger UI
http://localhost:5102/swagger
```

- 路由前綴：`api/user/{controller}`（在 `Controllers/BaseController.cs` 統一掛載）
- Routes / Query / JSON 命名規則與 Admin.API 一致：kebab-case route、snake_case query、snake_case JSON
- 與 `Admin.API` 共用 `../Shared.Web/` 的 convention、middleware、Swagger helper，差別只在路由前綴與 JWT scheme

## 主要檔案 / 子層

| 路徑 | 角色 |
|------|------|
| `Program.cs` | DI 註冊、Middleware pipeline、Swagger、CORS、JWT、Upload proxy 設定 |
| `Controllers/BaseController.cs` | 所有 Controller 的 base，提供 `ApiOk()` / `ApiError()` / `ApiValidationError()` |
| `Controllers/AuthController.cs` | 註冊 / 登入 / 忘記密碼 / Google OAuth |
| `Controllers/ProfileController.cs` | 取得 / 更新自己的會員資料 |
| `Controllers/UploadController.cs` | 透過 HttpClient proxy 轉發到 `../Upload.API/`（會員大頭貼等） |
| `appsettings*.json` | DB 連線、JWT secret、Upload API URL/Key、Serilog |
| `Properties/launchSettings.json` | 本機 5102 port 設定 |

## 依賴與被依賴

- 依賴：`../Application/`、`../Domain/`、`../Infrastructure/`、`../Shared.Web/`
- 被依賴：無（最外層 Presentation）
- JWT scheme：User（`access_token` + `refresh_token` rotation），與 `Admin.API` 是兩組獨立 secret

## 不要做的事

- 不要在 Controller 寫 EF / LINQ 查詢，全部走 `Application` 的 Service
- 不要在 Controller 做業務驗證，Controller 只負責呼叫 Service 並把結果包成 `ApiOk()` / `ApiError()`
- 不要 new `HttpClient` 呼叫 Upload.API，使用 `IHttpClientFactory` + `UploadApiSettings` 配置
- 不要暴露 Admin 才有的 endpoint（角色管理、後台帳號管理）到此 API
- 不要繞過 `RouteTokenTransformerConvention` 自寫 kebab-case route — controller / action 以 PascalCase 命名，由 transformer 自動轉
- 錯誤碼、列表 API、命名規範詳見 [../../.claude/rules/backend-api.md](../../.claude/rules/backend-api.md)
