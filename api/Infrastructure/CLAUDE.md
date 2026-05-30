# Infrastructure

`Application/Interfaces` 契約的實作層：DbContext、Repository、JWT、Email、檔案儲存、Migration。

## 目錄結構

| 子層 | 用途 |
|------|------|
| `Data/` | [`DrinkDbContext`](./Data/DrinkDbContext.cs) + `DrinkDbContextFactory`（design-time） |
| `Repositories/` | [`GenericRepository<T>`](./Repositories/GenericRepository.cs) — 實作 `IGenericRepository<T>`，自動套 soft delete filter |
| `Migrations/` | EF Core migration 產物，**不要手改**，靠 `dotnet ef migrations add` 產生 |
| `Services/` | 介面實作：`JwtTokenService`、`LogEmailSender`、`FileStorageService`、`HttpCurrentUserContext` |
| `Extensions/` | DI 註冊（`AddInfrastructureServices` 等）、EF / 分頁 / Serilog / 上傳擴充 |
| `Helpers/` | 跨服務工具（如 `HashHelper` for Argon2id） |
| `Settings/` | 預留：基礎設施專屬設定（連線字串多走 `IConfiguration`） |

## 關鍵慣例

### DbContext

- Entity 透過 `RegisterAllEntities()` 自動掃描 `Domain.Entities` 並註冊為 `DbSet`，**新 Entity 不需手動加** `DbSet<T>`
- 僅在以下情況才在 `OnModelCreating` 加 fluent API 設定：
  - 外鍵（特別是多重關聯、`OnDelete` 行為）
  - 自關聯（self-referencing FK）
  - 複合索引 / 唯一索引（partial unique with `HasFilter`）
  - 特殊欄位轉換（`HasConversion`）
- 一般長度 / 必填驗證在 Entity 用 `[StringLength]` / `[Required]` attribute

### Migration 指令

```bash
# 新增
dotnet ef migrations add {Name} \
  --project api/Infrastructure --startup-project api/Migrator \
  --output-dir Migrations

# 套用（含 Seeders）
dotnet run --project api/Migrator
```

### Service 實作

- `LogEmailSender`：開發環境用，把 email 寫進 log
- `JwtTokenService`：access / refresh token 生成 + 驗證
- `FileStorageService`：proxy 到 `Upload.API`
- `HttpCurrentUserContext`：從 `HttpContext` 解析目前使用者，給 `BaseService.CurrentUser` 用

## 與其他層的關係

- 依賴 `Domain`（Entity）、`Application`（Interfaces / Settings）
- 被 `Admin.API` / `User.API` / `Upload.API` / `Migrator` 引用 — 在各自 `Program.cs` 透過 `AddInfrastructureServices` 註冊
- **不**被 `Application` 反向依賴

完整命名、開發流程詳見 [../../.claude/rules/backend-api.md](../../.claude/rules/backend-api.md)。

## 不要做的事

- 不要手改 `Migrations/` 的 `.cs` 檔（除非要刪除整個 migration 重產）
- 不要在 `DrinkDbContext` 加 `DbSet<T>` — `RegisterAllEntities()` 已自動掃描
- 不要在 `OnModelCreating` 重複設定可以用 `[Attribute]` 表達的欄位（長度、必填）
- 不要在 Repository 內加業務邏輯 — `GenericRepository<T>` 通用即可，特殊查詢回 Service 用 `IQueryable` 組
- 不要繞過 `IGenericRepository` 直接在 Service 注入 `DrinkDbContext`
- 不要在 Service 實作裡硬編連線字串 / API Key — 一律透過 `Settings/` POCO + `IOptions<T>`
