# onion-layer-structure

## Purpose

定義後端 .NET solution 的 Onion Architecture 契約：Domain ← Application ← Infrastructure ← API 的依賴方向、跨層介面定義於 Application、HTTP 層成員歸屬 API 層、Service 透過 constructor injection 取得相依、`CurrentUserId` 等橫切邏輯集中於單一實作，避免層間反向依賴。

## Requirements

### Requirement: Project reference 依賴方向
各層的 project reference MUST 遵循 Onion Architecture 依賴方向：Domain ← Application ← Infrastructure ← API。Application MUST NOT reference Infrastructure。

#### Scenario: Application 編譯不依賴 Infrastructure
- **WHEN** 單獨編譯 Application 專案
- **THEN** 編譯 SHALL 成功，不需要 Infrastructure 專案

#### Scenario: Infrastructure 實作 Application 介面
- **WHEN** Infrastructure 專案實作 Repository 和服務
- **THEN** Infrastructure SHALL reference Application 專案以取得介面定義

### Requirement: 跨層邊界介面定義於 Application
所有由 Infrastructure 實作、Application 使用的介面 MUST 定義在 `Application/Interfaces/` 目錄下。包含：`IGenericRepository<T>`、`IJwtTokenService`、`ICurrentUserContext`、`IFileStorageService`。

#### Scenario: Service 依賴 Repository 介面
- **WHEN** Application 層的 Service 需要存取資料庫
- **THEN** Service SHALL 透過 `Application/Interfaces/IGenericRepository<T>` 注入，不直接引用 Infrastructure namespace

#### Scenario: Service 取得當前使用者
- **WHEN** Application 層的 Service 需要知道當前使用者身份
- **THEN** Service SHALL 透過 `ICurrentUserContext.UserId` 取得，不直接引用 `IHttpContextAccessor`

### Requirement: HTTP 層成員歸屬 API 層
Middleware、Conventions、Swagger 等 HTTP 專屬成員 MUST NOT 放在 Application 層。MUST 放在共用的 API 層專案（`Shared.API`）或各 API 專案中。

#### Scenario: Application 不含 HTTP 中介軟體
- **WHEN** 檢查 Application 專案
- **THEN** SHALL 不存在任何 `Middleware/` 目錄或 ASP.NET Middleware 類別

#### Scenario: Application 不含路由慣例
- **WHEN** 檢查 Application 專案
- **THEN** SHALL 不存在 `Conventions/` 目錄或 `IOutboundParameterTransformer` 實作

### Requirement: Service 使用 Constructor Injection
所有繼承 `BaseService` 的 Service MUST 透過 constructor 明確注入所需的 Repository 和介面依賴。MUST NOT 使用 `IServiceProvider` 動態解析依賴。

#### Scenario: Service constructor 列出所有依賴
- **WHEN** 檢查任意 Service 的 constructor
- **THEN** SHALL 能從 constructor 參數完整看出該 Service 使用了哪些 Repository 和外部服務

#### Scenario: BaseService 不含 IServiceProvider
- **WHEN** 檢查 BaseService 類別
- **THEN** SHALL 不存在 `IServiceProvider` 欄位或 `GetRequiredService` 呼叫

### Requirement: CurrentUserId 邏輯集中在單一實作
從 HTTP Context JWT claim 取得 UserId 的邏輯 MUST 只存在於 `ICurrentUserContext` 的唯一實作中（`HttpCurrentUserContext`）。BaseService 和 GenericRepository MUST NOT 各自實作此邏輯。

#### Scenario: GenericRepository 注入 ICurrentUserContext
- **WHEN** GenericRepository 需要 CurrentUserId（如 audit trail）
- **THEN** SHALL 透過注入的 `ICurrentUserContext` 取得，不自行讀取 `IHttpContextAccessor`
