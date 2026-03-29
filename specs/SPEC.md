# Project Spec: Drink API

## Objective
揪團訂飲料平台，讓用戶可以發起飲料揪團，朋友/同事加入後一起訂購。

**核心功能：**

前台：
- 用戶可自行建立商家與菜單（公開給所有人使用）
- 用戶可發起揪團，選擇商家，設定截止時間
- 其他用戶可加入揪團並選擇飲料
- 所有參與者可查看訂單狀態
- 發起人可匯出 Excel
- 飲料送達後，發起人可發送通知（Web Push 或 Email，用戶自選）

訂單狀態：
- 揪團進行中
- 揪團截止
- 飲料已送達
- 已結束

後台：
- 管理員管理平台整體運營（後台會員、前台會員、店家、訂單、Global 選項、系統設定）

## Tech Stack

**Monorepo**
- pnpm + Turborepo

**前台 (Client)** — `web/apps/client`
- Nuxt 4, Vue, Nuxt UI

**後台 (Admin)** — `web/apps/admin`
- Nuxt 4, Vue, Element Plus

**前端共用邏輯** — `web/internal`
- 通用工具（dateTimeFormatter 等）
- 共用 Response Interface

**後台 API** — `api/Admin.API`
- .NET 10, C#, ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL
- JWT Authentication

**前台 API** — `api/User.API`
- .NET 10, C#, ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL
- JWT Authentication

## Commands

### Root（Monorepo）
- 全部開發：`pnpm dev`（concurrently 啟動 API + Web）
- API 開發：`pnpm dev:api`
- Web 開發：`pnpm dev:web`
- 全部建置：`pnpm build:web`
- Admin 建置：`pnpm build:admin`
- Client 建置：`pnpm build:client`
- 安裝前端依賴：`pnpm prepare`

### 後端 API
- Build：`dotnet build`
- Run：`dotnet run --project api/User.API`
- Test：`dotnet test`
- Migration Add：`dotnet ef migrations add <Name> --project api/Infrastructure --startup-project api/Migrator`
- Migration Run：`dotnet ef database update --project api/Infrastructure --startup-project api/Migrator`

### 前端 Client
- Lint：`pnpm -C web --filter @drink/client lint`
- Typecheck：`pnpm -C web --filter @drink/client typecheck`

### 前端 Admin
- Preview：`pnpm -C web --filter @drink/admin preview`

## Testing

### 測試範圍

| 層級 | 測試類型 | 框架 | 說明 |
|------|---------|------|------|
| 前台前端 (Client) | E2E Test | Playwright | 核心業務流程端到端驗證 |
| 前台後端 (User.API) | Unit Test | xUnit | Service 層單元測試 |
| 後台前端 (Admin) | — | — | 不測試 |
| 後台後端 (Admin.API) | — | — | 不測試 |

### 前台前端 E2E Test

- 測試檔位置：`web/apps/client/e2e/`
- 執行指令：`pnpm --filter client test:e2e`
- 核心測試情境：
  1. 建立團購群（選擇商家、設定截止時間）
  2. 加入一筆品項（選擇飲料、甜度、冰塊、加料）
  3. 時間到結算
  4. 匯出 Excel

### 前台後端 Unit Test

- 測試檔位置：`api/User.API.Tests/`
- 執行指令：`dotnet test --project api/User.API.Tests`
- 測試目標：Service 層業務邏輯（判斷邏輯，非純 CRUD）
- Mock 外部依賴（DB、第三方 API）
- 重點測試項目：
  - 團購截止時間判斷（是否過期、能否加入）
  - 結算金額計算
  - 訂單狀態流轉（進行中 → 截止 → 已送達 → 已結束，不允許跳躍）
  - Refresh Token Rotation（舊 token 作廢、重複使用偵測）
  - Google OAuth domain 驗證邏輯
- 不需測試：純 CRUD、Controller 層、EF Core mapping

### 測試策略

| | Unit Test | E2E Test |
|---|---|---|
| 數量 | 多，每個邏輯分支一個 | 少，只測核心 happy path |
| 速度 | 快（毫秒級） | 慢（秒級，需開瀏覽器） |
| 何時跑 | 開發中隨時跑 | PR 合併前、部署前 |
| 開發順序 | 邊開發邊寫 | 功能完成後再補 |

> **注意：** 測試在前台核心功能（團購、訂單）開發完成後再補，目前先定義策略。

---

## Project Structure

### 後端 API
- `api/Domain/` – Entities, Enums, Interfaces
- `api/Application/` – Services, Requests, Responses, Mappings
- `api/Infrastructure/` – DbContext, Migrations, EF Extensions
- `api/User.API/` – User-facing API controllers
- `api/Admin.API/` – Admin-facing API controllers
- `api/Migrator/` – Migration runner

### 前端 Web（pnpm Monorepo）
- `web/apps/admin/` – 後台（Nuxt 4 + Element Plus）
  - `app/pages/` – 頁面
  - `app/components/` – 元件
  - `app/composable/` – 組合式函數
  - `app/stores/` – Pinia Store
  - `app/layouts/` – 佈局
  - `app/plugins/` – 插件
  - `app/assets/` – 靜態資源
- `web/apps/client/` – 前台（Nuxt 4 + Nuxt UI）
  - `app/pages/` – 頁面
  - `app/components/` – 元件
  - `app/assets/` – 靜態資源
- `web/internal/core/` – 共用工具函數（`@app/core`）
- `web/internal/models/` – 共用 Response Interface（`@app/models`）
- `web/internal/tsconfig/` – 共用 TypeScript 設定（`@app/tsconfig`）

## Code Style

```csharp
// Controller 繼承 BaseController，使用 async/await
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
}

// Service 繼承 BaseService
public class AuthService : BaseService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // 實現邏輯
    }
}

// Entity 繼承 BaseDataEntity，並實作 ICreateEntity / IUpdateEntity
public class Account : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(100)]
    public string Name { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

// 時間戳 Interface
public interface ICreateEntity
{
    DateTime CreatedAt { get; set; }
    int Creator { get; set; }
}

public interface IUpdateEntity
{
    DateTime UpdatedAt { get; set; }
    int Updater { get; set; }
}
```

## Global Rules

### 資料庫正規化
- 所有資料表嚴格遵守第三正規化（3NF）
- 不允許冗餘欄位、不允許傳遞相依
- 所有資料表必須實作 `ICreateEntity`（CreatedAt, Creator）與 `IUpdateEntity`（UpdatedAt, Updater）

### API 通用規範
- 前端統一使用 JSON Body 傳送資料
- API Request / Response 的 JSON key 統一使用 **snake_case**
- 後端透過 `JsonSerializerOptions` 設定 `PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower` 全域轉換

### 列表通用功能
所有列表 API 皆須支援以下功能：

| 功能 | 參數 | 說明 |
|------|------|------|
| 分頁 | `page`, `page_size` | 預設 `page=1`, `page_size=20` |
| 排序 | `sort_by`, `sort_order` | `sort_by` 為欄位名稱（snake_case），`sort_order` 為 `asc` 或 `desc`，預設依建立時間降冪 |
| 搜尋 | `keyword` | 模糊搜尋，各列表自行定義搜尋欄位（如 name, username） |
| 篩選 | 依業務定義 | 各列表自行定義篩選條件（如 `is_active`, `role_id`），以 query string 傳遞 |

- 所有列表參數統一透過 **Query String** 傳遞
- 後端需對 `sort_by` 做白名單驗證，避免任意欄位排序造成效能問題

#### 列表 Request 範例
```
GET /api/admin/users?page=1&page_size=20&sort_by=created_at&sort_order=desc&keyword=john&is_active=true
```

#### 列表 Response 範例
```json
{
  "data": {
    "items": [],
    "total": 100,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

---

### API Response 格式
所有 API 統一回傳格式：
```json
{
  "data": {},
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

| 欄位 | 型別 | 說明 |
|------|------|------|
| data | object / array / null | 回傳資料，無資料時為 null |
| message | string? | 錯誤訊息，成功時為 null |
| code | int | 數字錯誤碼，成功為 `0`，錯誤為 `4XXYY` 格式（見 Error Code Registry） |
| error | string? | 文字錯誤碼（如 `ROLE_ALREADY_EXISTS`），成功時為 null |
| errors | Record<string, string[]>? | 欄位驗證錯誤，非驗證錯誤時為 null |

#### 數字碼編碼規則
格式：`4_XX_YY`（5 位數整數）
- `4`：固定前綴
- `XX`：模組編號（00–09）
- `YY`：該模組內流水號（01–99）

| 模組碼 | 模組 |
|--------|------|
| 00 | 通用 |
| 01 | Admin Auth / Admin User |
| 02 | Admin Role |
| 03 | User Auth |
| 04 | Verification |
| 05 | Order |
| 06 | Shop |
| 07 | Drink Option |
| 08 | System Setting |
| 09 | Notification |

#### 成功回傳
```json
{
  "data": { "id": 1, "name": "Admin" },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 業務錯誤
```json
{
  "data": null,
  "message": "角色名稱已存在",
  "code": 40202,
  "error": "ROLE_ALREADY_EXISTS",
  "errors": null
}
```

#### 欄位驗證錯誤
```json
{
  "data": null,
  "message": "輸入驗證失敗",
  "code": 40001,
  "error": "VALIDATION_ERROR",
  "errors": {
    "name": ["角色名稱為必填"],
    "permissions[0].menu_id": ["Menu 不存在"]
  }
}
```
- `errors` 的 key 為 snake_case，對應 Request JSON key
- 前端表單的 prop 與 JSON key 一致（snake_case），可直接將 errors 對應到欄位顯示
- 對應鏈路：`前端 JSON key (snake_case) → 後端 DTO property (PascalCase，自動轉換) → ModelState key (snake_case) → errors key → 前端 form prop`

---

### Creator 欄位慣例
- `Creator` / `Updater` 欄位型別為 `int`
- `Creator = 0` 代表 **系統操作或用戶自行操作**（如：前台自行註冊、Google 登入自動建立、前台觸發驗證信）
- `Creator > 0` 代表由特定 AdminUser 或 User 操作
- 後台操作時 Creator / Updater = 當前登入的 AdminUserId
- 前台用戶操作自身資料時 Updater = 自己的 UserId

---

### Error Code Registry
所有 API 業務錯誤碼統一定義，後端以 `static class ErrorCodes` 管理常數。每個錯誤碼同時具有數字碼（`code`）與文字碼（`error`）。

#### 通用 (4-00-XX)
| Code | Error | HTTP | 說明 |
|------|-------|------|------|
| 0 | — | 200 | 成功（`error` 為 null） |
| 40001 | `VALIDATION_ERROR` | 400 | 欄位驗證失敗（搭配 errors） |
| 40002 | `UNAUTHORIZED` | 401 | 未登入或 Token 無效 |
| 40003 | `FORBIDDEN` | 403 | 無權限操作 |
| 40004 | `NOT_FOUND` | 404 | 資源不存在（通用） |
| 40005 | `SERVICE_UNAVAILABLE` | 503 | 維護模式 |

#### 後台認證 / 後台帳號（Admin Auth）(4-01-XX)
| Code | Error | HTTP | 說明 |
|------|-------|------|------|
| 40101 | `INVALID_CREDENTIALS` | 401 | 帳號或密碼錯誤 |
| 40102 | `INVALID_PASSWORD` | 400 | 舊密碼錯誤 |
| 40103 | `USERNAME_ALREADY_EXISTS` | 409 | 帳號已存在 |
| 40104 | `ADMIN_ACCOUNT_INACTIVE` | 403 | 後台帳號已停用 |
| 40105 | `CANNOT_DELETE_ADMIN` | 403 | 不可刪除 Admin 帳號 |
| 40106 | `CANNOT_CHANGE_ADMIN_ROLE` | 403 | 不可變更 Admin 角色 |

#### 後台角色（Admin Role）(4-02-XX)
| Code | Error | HTTP | 說明 |
|------|-------|------|------|
| 40201 | `ROLE_NOT_FOUND` | 400 | 角色不存在 |
| 40202 | `ROLE_ALREADY_EXISTS` | 409 | 角色名稱已存在 |
| 40203 | `CANNOT_MODIFY_SYSTEM_ROLE` | 403 | 不可修改系統角色 |
| 40204 | `CANNOT_DELETE_SYSTEM_ROLE` | 403 | 不可刪除系統角色 |
| 40205 | `ROLE_HAS_STAFF` | 400 | 角色下有帳號，需指定 reassign_role_id |
| 40206 | `INVALID_MENU_ID` | 400 | Menu ID 不存在 |

#### 前台認證（User Auth）(4-03-XX)
| Code | Error | HTTP | 說明 |
|------|-------|------|------|
| 40301 | `EMAIL_ALREADY_EXISTS` | 409 | Email 已存在 |
| 40302 | `INVALID_CREDENTIALS` | 401 | Email 或密碼錯誤 |
| 40303 | `EMAIL_NOT_VERIFIED` | 403 | Email 未驗證 |
| 40304 | `ACCOUNT_INACTIVE` | 403 | 帳號已停用 |
| 40305 | `GOOGLE_DOMAIN_NOT_ALLOWED` | 403 | Google 登入 domain 不符 |
| 40306 | `INVALID_TOKEN` | 400 | Token 無效或過期 |
| 40307 | `TOKEN_ALREADY_USED` | 400 | Token 已使用 |

#### 驗證信（Verification）(4-04-XX)
| Code | Error | HTTP | 說明 |
|------|-------|------|------|
| 40401 | `RESEND_TOO_FREQUENT` | 429 | 重發過於頻繁（同用戶同類型 10 分鐘內最多 1 次） |

#### 揪團訂單（Order）(4-05-XX)
| Code | Error | HTTP | 說明 |
|------|-------|------|------|
| 40501 | `SHOP_NOT_AVAILABLE` | 400 | 店家不可用（下架或已刪除） |
| 40502 | `INVALID_DEADLINE` | 400 | 截止時間無效（必須在未來） |
| 40503 | `NOT_INITIATOR` | 403 | 非揪團發起人 |
| 40504 | `ORDER_NOT_ACTIVE` | 400 | 揪團非 Active 狀態或已過截止 |
| 40505 | `INVALID_STATUS_TRANSITION` | 400 | 無效的狀態流轉 |
| 40506 | `CANNOT_CANCEL_ORDER` | 400 | 不可取消（僅 Active/Closed 可取消） |
| 40507 | `ORDER_NOT_FOUND` | 404 | 揪團不存在 |

#### 店家（Shop）(4-06-XX)
| Code | Error | HTTP | 說明 |
|------|-------|------|------|
| 40601 | `SHOP_ALREADY_EXISTS` | 409 | 店家名稱已存在 |
| 40602 | `SHOP_NOT_FOUND` | 404 | 店家不存在 |
| 40603 | `CATEGORY_ALREADY_EXISTS` | 409 | 分類名稱已存在（同店家內） |
| 40604 | `CATEGORY_NOT_FOUND` | 404 | 分類不存在 |
| 40605 | `MENU_ITEM_NOT_FOUND` | 404 | 菜單品項不存在 |
| 40606 | `DRINK_ITEM_NOT_FOUND` | 400 | DrinkItem 不存在 |

#### 飲料選項（Drink Option）(4-07-XX)
| Code | Error | HTTP | 說明 |
|------|-------|------|------|
| 40701 | `DRINK_ITEM_ALREADY_EXISTS` | 409 | 通用品名已存在 |
| 40702 | `DRINK_ITEM_IN_USE` | 400 | 品名被店家菜單引用，不可刪除 |
| 40703 | `SUGAR_ALREADY_EXISTS` | 409 | 甜度名稱已存在 |
| 40704 | `SUGAR_IN_USE` | 400 | 甜度被引用，不可刪除 |
| 40705 | `SUGAR_NOT_FOUND` | 400 | 甜度不存在 |
| 40706 | `ICE_ALREADY_EXISTS` | 409 | 冰塊名稱已存在 |
| 40707 | `ICE_IN_USE` | 400 | 冰塊被引用，不可刪除 |
| 40708 | `ICE_NOT_FOUND` | 400 | 冰塊不存在 |
| 40709 | `TOPPING_ALREADY_EXISTS` | 409 | 加料名稱已存在 |
| 40710 | `TOPPING_IN_USE` | 400 | 加料被引用，不可刪除 |
| 40711 | `TOPPING_NOT_FOUND` | 400 | 加料不存在 |
| 40712 | `SIZE_ALREADY_EXISTS` | 409 | 容量名稱已存在 |
| 40713 | `SIZE_IN_USE` | 400 | 容量被引用，不可刪除 |
| 40714 | `SIZE_NOT_FOUND` | 400 | 容量不存在 |

#### 系統設定（System Setting）(4-08-XX)
| Code | Error | HTTP | 說明 |
|------|-------|------|------|
| 40801 | `SETTING_KEY_NOT_FOUND` | 400 | 設定 Key 不存在 |
| 40802 | `SETTING_VALUE_INVALID` | 400 | 設定值格式無效 |

#### 通知（Notification）(4-09-XX)
| Code | Error | HTTP | 說明 |
|------|-------|------|------|
| 40901 | `NOTIFICATION_NOT_FOUND` | 404 | 通知不存在 |

---

## Git Workflow
- Branch: `feat/<feature>`, `fix/<issue>`, `chore/<task>`
- Commit: `feat(scope): description`
- 每個功能一個 PR

## Boundaries

✅ Always:
- 所有 Entity 必須實作 `ICreateEntity` 與 `IUpdateEntity`
- Entity 必須繼承 `BaseDataEntity`
- Service 必須繼承 `BaseService`
- Controller 必須繼承 `BaseController`
- 密碼必須以 Argon2id 雜湊存儲（Salt + Pepper），絕不明文
- Salt 由 Argon2id 自動生成（per-user，存於 hash 內）
- Pepper 為全域秘密，存於 appsettings / 環境變數，不進版控
- 雜湊流程：`Argon2id(password + pepper, salt)`
- 新增 Entity 後必須建立 Migration
- 資料表嚴格遵守第三正規化（3NF）
- API 回傳格式統一使用 `{ data, message, code, error, errors }`
- 所有列表 API 必須支援分頁、排序、搜尋、篩選

⚠️ Ask First:
- 修改現有 Entity 欄位（影響 Migration）
- 修改 BaseDataEntity / BaseService / BaseController
- 引入新的 NuGet 套件
- 修改 DbContext 配置

🚫 Never:
- 在日誌記錄密碼或 Token
- 提交 secrets 或 API keys
- 直接修改已執行的 Migration 文件
- 跳過輸入驗證
