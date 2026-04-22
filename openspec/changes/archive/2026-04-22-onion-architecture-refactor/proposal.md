## Why

目前 Application 層直接 reference Infrastructure（Application → Infrastructure → Domain），依賴方向違反 Onion Architecture 原則。導致業務邏輯與基礎設施耦合，HTTP Context 滲透到 Service 層，且 Middleware / Conventions 等 HTTP 層專屬的東西放在 Application 層。需要翻轉依賴方向並將成員歸位。

## What Changes

- **BREAKING**：翻轉 project reference 方向 — Application 移除對 Infrastructure 的引用，改由 Infrastructure reference Application
- 將跨層邊界的介面（`IGenericRepository<T>`、`IJwtTokenService`）從 Infrastructure 搬到 Application
- 新增 `ICurrentUserContext` 介面於 Application，取代 BaseService / GenericRepository 直接讀取 `IHttpContextAccessor`
- 將 HTTP 層專屬成員從 Application 搬到 API 層：
  - `Middleware/GlobalExceptionMiddleware.cs`、`Middleware/RoleMiddleware.cs`
  - `Conventions/SlugifyParameterTransformer.cs`、`Conventions/SnakeCaseQueryValueProviderFactory.cs`
  - `Extensions/SwaggerExtensions.cs`
- BaseService 改用 constructor injection 取代 Service Locator（`IServiceProvider.GetRequiredService`）
- 調整 DI registration 適配新的 project reference 結構

## Capabilities

### New Capabilities
- `onion-layer-structure`: 定義各層的職責邊界、依賴方向規則、成員歸屬規範

### Modified Capabilities

（目前無既有 specs 受影響）

## Impact

- **Project references**：Application.csproj 移除 Infrastructure reference；Infrastructure.csproj 新增 Application reference
- **所有 Service**：constructor 簽名改變（從無參數 → 注入所需的 Repository 和介面）
- **BaseService**：移除 `IServiceProvider`、`IHttpContextAccessor`，改注入 `ICurrentUserContext`
- **GenericRepository**：移除直接讀取 `IHttpContextAccessor`，改注入 `ICurrentUserContext`
- **DI registration**：Scrutor 掃描方式可能需調整
- **三個 API 專案**：各自接收搬過來的 Middleware / Conventions / Swagger
- **編譯順序**：Domain → Application → Infrastructure → API（與目前相同，只是 Application 不再依賴 Infrastructure）
