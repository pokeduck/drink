# Application

用例層。所有業務邏輯、DTO、Mapper、Service 都在這。Controller 透過本層服務取得資料，**不**直接碰 `DbContext`。

## 目錄結構

| 子層 | 用途 |
|------|------|
| `Services/` | 業務邏輯；繼承 `BaseService`，Scrutor 自動註冊為 Scoped |
| `Mappings/` | Mapperly Mapper（一個功能模組一檔） |
| `Requests/{Admin,User}/` | Request DTO，`{功能}Request.cs` |
| `Responses/{Admin,User}/` | Response DTO，`{功能}Response.cs`；通用 `ApiResponse` 也在這 |
| `Interfaces/` | 對外契約：`IGenericRepository`、`ICurrentUserContext`、`IEmailSender`、`IPasswordHasher`、`IJwtTokenService`、`IFileStorageService` |
| `Extensions/` | DI 註冊用 extension（`AddApplicationServices` 等） |
| `Helpers/` | 純函式輔助（`SortByValidator` 等） |
| `Constants/` | 常數（`ErrorCodes`、`MenuConstants`） |
| `Models/` | 共用模型（`PaginationList<T>`） |
| `Settings/` | 強型別設定 POCO（`JwtSettings`、`UploadSettings`） |
| `Attributes/` | 自訂屬性（`HttpUrlAttribute`、`RequireRoleAttribute`） |

## 關鍵慣例

### Service

- 繼承 [`BaseService`](./Services/BaseService.cs)，透過 `GetRepository<TEntity>()` 取 `IGenericRepository<T>`
- Scrutor 自動掃描 `BaseService` 子類別註冊為 Scoped，**不要**手動 `services.AddScoped<T>`
- 命名 `{模組}Service.cs`（如 `AdminMenuService.cs`、`AdminOrderService.cs`）
- 錯誤回傳：`Fail<T>(errorCode, message, errors)`；`errors` key 為 snake_case，與 Request DTO 欄位對齊

### Mapper（Mapperly）

- `[Mapper] public static partial class XxxMapper`
- Extension method 風格：`entity.ToXxxResponse()`、`request.ToEntity()`
- 自訂映射用 `[MapProperty]`，忽略目標用 `[MapperIgnoreTarget]`
- 一個功能模組一個 Mapper 檔

### DTO

- Request / Response 依模組分子資料夾（`Admin/` 或 `User/`）
- 命名 `{功能}Request.cs` / `{功能}Response.cs`
- 列表 Response **必須**含 `Id` 與 `CreatedAt`

完整流程、錯誤碼編碼規則（`4XXYY`）、列表排序、命名規範詳見 [../../.claude/rules/backend-api.md](../../.claude/rules/backend-api.md)。

## 與其他層的關係

- 依賴 `Domain`（Entity / Enum / Entity Interface）
- **不**直接依賴 `Infrastructure`：DbContext / Repository / JWT / Email 透過 `Interfaces/` 注入
- 被 `Admin.API` / `User.API` / `Upload.API` / `Migrator` 引用

## 不要做的事

- 不要在 Service 直接 new `DrinkDbContext`，一律走 `GetRepository<T>()`
- 不要在 Service 拋未分類的 `Exception` 給 Controller — 用 `Result<T>` + `Fail<T>(...)` 表達失敗
- 不要在 Mapper 內寫業務分支邏輯（純欄位映射，複雜邏輯回 Service）
- 不要把 Entity 直接當 Response 回傳，必經 Mapper 轉成 `*Response`
- 不要把錯誤碼字串散落各處 — 統一放 `Constants/ErrorCodes.cs`
- 不要在 DTO 用 `DateTime?` 含時區不一致 — 統一 UTC
