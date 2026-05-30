# Domain

Onion Architecture 最內層。只放純業務領域型別（Entity / Enum / Interface / ValueObject），不依賴任何外部框架（EF Core 除外，僅用到 `DataAnnotations` 屬性）。

## 目錄結構

| 子層 | 用途 |
|------|------|
| `Entities/` | 業務實體；對應 DB Table，PascalCase 命名 |
| `Enums/` | 狀態列舉（如 `GroupOrderStatus`、`UserStatus`、`VerificationEmailType`） |
| `Interfaces/` | Entity 行為契約：`ICreateEntity` / `IUpdateEntity` / `ISoftDeleteEntity` |
| `Validations/` | 預留：跨欄位 / 領域驗證規則 |
| `ValueObjects/` | 預留：不可變值物件（如 Money、DateRange） |

## Entity 慣例

所有 Entity 必須：

- 繼承 [`BaseDataEntity`](./Entities/BaseDataEntity.cs)（提供 `Id`）
- 實作 `ICreateEntity`（`CreatedAt`、`Creator`）
- 實作 `IUpdateEntity`（`UpdatedAt`、`Updater`）
- 視情況加 `ISoftDeleteEntity`（`IsDeleted`，自動套 query filter）

屬性級驗證直接寫在 Entity：

- 字串長度：`[StringLength(n)]`
- 必填：`[Required]`
- 自訂 URL：`[HttpUrl]`（位於 `Application/Attributes`，僅 DTO 用）

Navigation property 初始化：

- 單一導航：`public Foo Foo { get; set; } = null!;`
- 集合導航：`public ICollection<Bar> Bars { get; set; } = [];`

詳細命名、欄位規範見 [../../.claude/rules/backend-api.md](../../.claude/rules/backend-api.md)。

## 與其他層的關係

- 被 `Application`、`Infrastructure` 引用
- **不可**反向依賴任何專案（包含 `Application` 的 DTO）
- DbContext 透過 `RegisterAllEntities()` 自動掃描本層 Entity 註冊為 `DbSet`，**不需手動加** `DbSet<T>`

## 不要做的事

- 不要 `using Microsoft.EntityFrameworkCore`（除非是 query filter attribute），fluent API 留給 `Infrastructure/Data/DrinkDbContext.OnModelCreating`
- 不要在 Entity 寫業務邏輯方法（簡單 computed property 可，複雜邏輯放 Service）
- 不要把 DTO / Response 型別放這裡，那是 `Application/Requests`、`Application/Responses` 的職責
- 不要 import `Application` 或 `Infrastructure` 任何型別
- Enum 不要混雜 `[Flags]` 與一般 enum，狀態類請用一般 enum 並走 `int` underlying type
