## 1. 建立 Shared.Web 專案 + 搬移 HTTP 層成員

- [x] 1.1 建立 `api/Shared.Web/` 專案（class library），reference Application + Infrastructure
- [x] 1.2 從 Application 搬移 `Middleware/GlobalExceptionMiddleware.cs` 到 Shared.Web
- [x] 1.3 從 Application 搬移 `Middleware/RoleMiddleware.cs` 到 Shared.Web
- [x] 1.4 從 Application 搬移 `Conventions/SlugifyParameterTransformer.cs` 到 Shared.Web
- [x] 1.5 從 Application 搬移 `Conventions/SnakeCaseQueryValueProviderFactory.cs` 到 Shared.Web
- [x] 1.6 從 Application 搬移 `Extensions/SwaggerExtensions.cs` 到 Shared.Web
- [x] 1.7 三個 API 專案加入 Shared.Web reference，更新 using namespace

## 2. 介面搬移 + 新增 ICurrentUserContext

- [x] 2.1 在 Application 建立 `Interfaces/` 目錄
- [x] 2.2 將 `IGenericRepository<T>` 從 Infrastructure 搬到 `Application/Interfaces/`，調整 namespace
- [x] 2.3 將 `PaginationList<T>` 從 Infrastructure 搬到 `Application/Models/`（純資料結構）
- [x] 2.4 將 `IJwtTokenService` 從 Infrastructure 搬到 `Application/Interfaces/`，調整 namespace
- [x] 2.5 將 `IFileStorageService` + `FileStorageResult` 從 Infrastructure 搬到 `Application/Interfaces/`
- [x] 2.6 新增 `Application/Interfaces/ICurrentUserContext.cs`
- [x] 2.7 在 Infrastructure 新增 `HttpCurrentUserContext.cs` 實作 `ICurrentUserContext`

## 3. 翻轉 Project Reference

- [x] 3.1 Application.csproj：移除 Infrastructure reference，加入 `Microsoft.EntityFrameworkCore` package
- [x] 3.2 Infrastructure.csproj：加入 Application reference
- [x] 3.3 修復所有因 namespace 搬移產生的編譯錯誤（更新 using）
- [x] 3.4 確認編譯通過

## 4. BaseService + Service 改 Constructor Injection

- [x] 4.1 改寫 BaseService：移除 `IServiceProvider`，改注入 `ICurrentUserContext`
- [x] 4.2 改寫 GenericRepository：注入 `ICurrentUserContext` 取代 `IHttpContextAccessor`
- [x] 4.3 改寫 AdminAuthService：constructor 注入所需 Repository
- [x] 4.4 改寫 AdminMenuService：constructor 注入所需 Repository
- [x] 4.5 改寫 AdminRoleService：constructor 注入所需 Repository
- [x] 4.6 改寫 AdminUserService：constructor 注入所需 Repository
- [x] 4.7 改寫 DrinkOptionService：constructor 注入所需 Repository
- [x] 4.8 改寫 FileUploadService：constructor 注入所需 Repository
- [x] 4.9 改寫 MemberService：constructor 注入所需 Repository
- [x] 4.10 改寫 VerificationService：constructor 注入所需 Repository

## 5. DI Registration 調整

- [x] 5.1 Infrastructure DI：註冊 `ICurrentUserContext` → `HttpCurrentUserContext`
- [x] 5.2 確認 `IGenericRepository<T>` 的 open generic registration 仍正常
- [x] 5.3 確認 Scrutor AsSelf() 掃描在新結構下仍正常

## 6. 驗證

- [x] 6.1 全專案編譯通過（`dotnet build`）
- [x] 6.2 啟動三個 API 服務，確認正常運作
- [x] 6.3 測試登入流程（Admin.API）
- [x] 6.4 測試角色 CRUD（確認 RoleMiddleware 運作）
