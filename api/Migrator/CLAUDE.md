# Migrator/

獨立 Console 程式，套用 EF Core migration 並執行 `Seeders/`。本機開發、CI、部署都統一透過這支 binary，不直接對 DB 下 SQL 改 schema。

## 啟動 / 指令

```bash
# 套用 migration + 跑全部 Seeders
dotnet run --project api/Migrator

# 新增 migration（startup project 指到 Migrator）
dotnet ef migrations add {MigrationName} \
  --project api/Infrastructure \
  --startup-project api/Migrator \
  --output-dir Migrations
```

- 連線字串：`appsettings.json` → `ConnectionStrings:DefaultConnection`
- Migration 歷史表：`__ef_migration_history`（PostgreSQL）
- 流程：`Database.MigrateAsync()` → 反射掃 `ISeeder` → 依 `Order` 升冪執行
- Seeders 寫成 idempotent（重複跑不會壞資料），每次 `dotnet run` 都會跑

## 主要檔案 / 子層

| 路徑 | 角色 |
|------|------|
| `Program.cs` | 進入點：讀 config、建 `DrinkDbContext`、跑 migrate + seeders |
| `Seeders/ISeeder.cs` | 介面：`Order` 決定執行順序、`Seed(context, configuration)` 內含實際邏輯 |
| `Seeders/AdminUserSeeder.cs` | 預設超管帳號（密碼 hash + pepper 由 configuration 注入） |
| `Seeders/AdminMenuSeeder.cs` | 後台選單樹（含路由、icon） |
| `Seeders/AdminMenuRoleSeeder.cs` | 角色對 Menu 的 CRUD 存取 |
| `Seeders/AdminRoleSeeder.cs` | 預設角色（Super Admin 等） |
| `Seeders/DrinkOptionSeeder.cs` | 預設冰塊 / 甜度 / 加料 / 尺寸 |
| `appsettings*.json` | DB 連線、初始管理員密碼、Pepper 等 seed 用 config |

## 依賴與被依賴

- 依賴：`../Application/`、`../Domain/`、`../Infrastructure/`（使用 `DrinkDbContext`）
- 被依賴：無（獨立可執行）
- 不依賴 `../Shared.Web/`（沒有 HTTP pipeline）、不依賴任何 API 專案

## 不要做的事

- 不要繞過這支程式直接對 DB 下 `CREATE TABLE` / `ALTER TABLE` — schema 變更一律透過 EF migration
- 不要在 Seeder 寫「先 Truncate 再 Insert」— Seeder 必須 idempotent（檢查存在再插）
- 不要把生產資料寫進 Seeder（測試帳號可、真實會員資料不可）
- 不要在 API 專案啟動時自動跑 migration / seed — 統一由這支 binary 觸發
- 新增 Seeder 要記得實作 `ISeeder` 並設定唯一 `Order`，否則執行順序不穩定
- 完整開發流程詳見 [../../.claude/rules/backend-api.md](../../.claude/rules/backend-api.md)
