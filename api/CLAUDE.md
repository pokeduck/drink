# api/

揪團訂飲料平台後端。.NET 10 + EF Core 10 + PostgreSQL，採 Onion Architecture 拆 8 個 .NET 專案。

## 架構分層（Onion）

依賴方向由外向內，內層不可反向依賴外層：

```
Admin.API / User.API / Upload.API   (Presentation)
        ↓ depends on
     Application                    (Use Cases / Service)
        ↓ depends on
     Infrastructure                 (DbContext / Repository / 外部整合)
        ↓ depends on
       Domain                       (Entity / Enum / Interface)
```

`Shared.Web` 為跨 API 共用的 ASP.NET 慣例層；`Migrator` 為獨立 console 程式，啟動時跑 migration + seeder。

## 專案一覽

| 專案 | 角色 | 說明 |
|------|------|------|
| `Domain/` | 核心 | Entity / Enum / 介面；無任何外部依賴 |
| `Application/` | 用例 | Service / Mapper / DTO / Helper；定義對外契約 |
| `Infrastructure/` | 基礎設施 | `DrinkDbContext`、`GenericRepository`、JWT、Email、檔案儲存實作 |
| `Shared.Web/` | 共用 Web | `SlugifyParameterTransformer`、`SnakeCaseQueryValueProviderFactory`、`GlobalExceptionMiddleware`、`RoleMiddleware`、Swagger 設定 |
| `Admin.API/` | 後台 API | port 5101，給 `@drink/admin` 用 |
| `User.API/` | 前台 API | port 5102，給 `@drink/client` 用 |
| `Upload.API/` | 上傳服務 | port 5103，獨立服務，API Key 內部認證 + 對外 `/assets/...` |
| `Migrator/` | DB 工具 | `dotnet run --project api/Migrator` 套 migration + 跑 `Seeders/` |

## 開發流程速覽

```
Entity → DbContext (if FK/Index) → Request DTO → Response DTO → Mapper
       → Service → Controller
       → Add Migration (if schema change) → Run Migrator
       → pnpm generate → FrontEnd
```

完整步驟、命名規則、錯誤碼、列表 API 規範詳見 [../.claude/rules/backend-api.md](../.claude/rules/backend-api.md)。

## Port 對應

| 服務 | Port |
|------|------|
| Admin.API | 5101 |
| User.API | 5102 |
| Upload.API | 5103 |

## 各層 CLAUDE.md

| 層 | 文件 |
|----|------|
| Domain | [./Domain/CLAUDE.md](./Domain/CLAUDE.md) |
| Application | [./Application/CLAUDE.md](./Application/CLAUDE.md) |
| Infrastructure | [./Infrastructure/CLAUDE.md](./Infrastructure/CLAUDE.md) |

## 不要做的事

- 不要讓 `Domain` 引用 `Application` / `Infrastructure`（破壞 Onion）
- 不要在 `Admin.API` / `User.API` 直接 new `DbContext` 或寫 LINQ 查詢 — 透過 `Application` 的 Service
- 不要在 Controller 寫業務邏輯，Controller 只負責 `ApiOk()` / `ApiError()` 包裝
- 不要繞過 `Migrator` 直接對 DB 下 SQL 改 schema
- 不要在 `Upload.API` 加業務邏輯，它只負責檔案儲存 + 3 層驗證
