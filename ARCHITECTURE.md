# Project Architecture

## Root

```
drink/
│
├── .claude/                          # Claude Code AI 助手設定
│   ├── CLAUDE.md                     #   專案開發準則與規範（AI 讀取用）
│   ├── docs/                         #   框架參考文件（Nuxt / Nuxt UI API 速查）
│   ├── rules/                        #   情境觸發規則（符合條件時自動載入）
│   ├── settings.local.json           #   本機權限設定（不入版控）
│   └── skills/                       #   自訂技能（commit、migrate 等斜線指令）
│
├── .github/workflows/                # GitHub Actions CI/CD
│   ├── api.yml                       #   後端 .NET build + test
│   └── web.yml                       #   前端 build + lint
│
├── .vscode/settings.json             # VSCode 專案設定
├── .editorconfig                     # 跨編輯器格式統一（縮排、換行、編碼）
├── .eslintrc.js                      # ESLint 規則（root 層級）
├── .npmrc                            # npm/pnpm registry 設定
├── .nvmrc                            # Node.js 版本鎖定
├── .gitignore                        # Git 忽略規則
│
├── docker-compose.yml                # Docker 服務編排（PostgreSQL）
├── docker/                           # Docker 初始化腳本
│   └── postgres/init/
│       └── 01-create-db.sql          #   建立資料庫 SQL
│
├── package.json                      # Root Monorepo 設定（concurrently 統一啟動 API + Web）
├── pnpm-lock.yaml                    # Root 依賴鎖定
│
├── specs/                            # 功能規格文件（Addy Osmani spec 風格）
│   ├── SPEC.md                       #   Spec 撰寫指南與模板
│   ├── index.md                      #   全功能模組總覽與開發進度
│   └── *.md                          #   各功能模組 spec（admin-role, shop, user-auth...）
│
├── api/                              # ── 後端（.NET 10 Clean Architecture）──
└── web/                              # ── 前端（Nuxt 4 Monorepo）──
```

---

## Backend — `api/`

採用 Clean Architecture 分層，由內而外：Domain → Infrastructure → Application → API Host。

```
api/
├── Drink.sln                         # Solution 檔（包含所有 project）
├── global.json                       # .NET SDK 版本鎖定
│
├── Domain/                           # 【領域層】純 C# 類別，零外部依賴
│   ├── Domain.csproj
│   ├── Entities/                     #   資料實體（對應 DB Table，繼承 BaseDataEntity）
│   │   ├── BaseDataEntity.cs         #     基底實體（Id 自動遞增主鍵）
│   │   ├── Admin*.cs                 #     後台管理相關（Menu, MenuRole, Role, User, RefreshToken）
│   │   ├── DrinkItem.cs              #     飲料品項
│   │   ├── Sugar.cs / Ice.cs         #     甜度 / 冰量選項
│   │   ├── Topping.cs / Size.cs      #     配料 / 容量選項
│   │   └── ...                       #     （隨功能擴充）
│   ├── Enums/                        #   列舉型別（狀態碼、類型等）
│   ├── Interfaces/                   #   實體行為介面
│   │   ├── ICreateEntity.cs          #     建立時間 + 建立者自動填入
│   │   ├── IUpdateEntity.cs          #     更新時間 + 更新者自動填入
│   │   └── ISoftDeleteEntity.cs      #     軟刪除（IsDeleted + DeletedAt）
│   ├── Validations/                  #   領域驗證規則（預留）
│   └── ValueObjects/                 #   值物件（預留）
│
├── Infrastructure/                   # 【基礎設施層】DB、外部服務、技術實作
│   ├── Infrastructure.csproj
│   ├── Data/
│   │   └── DrinkDbContext.cs         #   EF Core DbContext（OnModelCreating 設定關聯、自動註冊 Entity）
│   ├── Repositories/
│   │   ├── IGenericRepository.cs     #   通用 Repository 介面（CRUD、分頁、交易）
│   │   └── GenericRepository.cs      #   通用 Repository 實作（EF Core）
│   ├── Migrations/                   #   EF Core Migration 檔（自動產生，勿手動修改）
│   ├── Extensions/
│   │   ├── EFExtension.cs            #   EF 擴充（Entity 自動註冊、SaveChanges 攔截填入時間戳）
│   │   ├── PaginationExtension.cs    #   分頁查詢擴充（PaginationList<T>）
│   │   ├── SerilogExtensions.cs      #   Serilog 日誌設定（Console + File daily rolling）
│   │   ├── ServiceCollectionExtensions.cs  # DI 註冊擴充（DbContext、Repository、JWT）
│   │   └── FileUploadExtensions.cs   #   檔案上傳 HttpClient 設定
│   ├── Helpers/
│   │   └── HashHelper.cs             #   密碼雜湊（Argon2id + Salt + Pepper）
│   ├── Services/
│   │   ├── IJwtTokenService.cs       #   JWT 服務介面
│   │   ├── JwtTokenService.cs        #   JWT 產生 / 驗證（access_token + refresh_token）
│   │   └── FileStorageService.cs     #   檔案存儲服務（Upload.API 端）
│   └── Settings/
│       ├── JwtSettings.cs            #   JWT 設定模型（Secret, Issuer, Expiry...）
│       ├── UploadApiSettings.cs      #   Upload API 連線設定（Admin/User API 端）
│       └── UploadSettings.cs         #   上傳規則設定（Upload.API 端：白名單、大小限制）
│
├── Application/                      # 【應用層】業務邏輯、DTO、Mapper、Middleware
│   ├── Application.csproj            #   依賴：Mapperly 4.x、Scrutor 7.x
│   ├── Attributes/
│   │   └── RequireRoleAttribute.cs   #   自訂權限標記（搭配 RoleMiddleware 驗證 Menu 權限）
│   ├── Constants/
│   │   ├── ErrorCodes.cs             #   統一錯誤碼定義（4XXYY 格式 + 文字碼）
│   │   └── MenuConstants.cs          #   選單常數（權限識別碼對應）
│   ├── Conventions/
│   │   ├── SlugifyParameterTransformer.cs      # 路由 kebab-case 轉換（PascalCase → kebab-case）
│   │   └── SnakeCaseQueryValueProviderFactory.cs # Query string snake_case → camelCase 自動綁定
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs  # Application 層 DI 註冊（Scrutor 自動掃描 Service）
│   │   └── SwaggerExtensions.cs      #   Swagger UI 設定擴充
│   ├── Helpers/
│   │   └── SortByValidator.cs        #   排序參數白名單驗證
│   ├── Mappings/                     #   Mapperly 物件映射（compile-time source generator）
│   │   ├── AdminMenuMapper.cs        #     AdminMenu ↔ MenuTreeResponse
│   │   ├── AdminRoleMapper.cs        #     AdminRole ↔ AdminRoleListResponse
│   │   ├── AdminUserMapper.cs        #     AdminUser ↔ AdminUserListResponse / DetailResponse
│   │   └── DrinkOptionMapper.cs      #     DrinkItem/Sugar/Ice/Topping/Size ↔ Response
│   ├── Middleware/
│   │   ├── GlobalExceptionMiddleware.cs  # 全域例外攔截（統一錯誤回傳格式）
│   │   └── RoleMiddleware.cs         #   角色權限驗證中介層（JWT + AdminMenuRole 比對）
│   ├── Requests/                     #   Request DTO（API 輸入模型）
│   │   ├── PaginationRequest.cs      #     通用分頁參數（Page, PageSize, SortBy, SortOrder）
│   │   ├── Admin/                    #     後台 Request（Auth, Role, User, DrinkOption...）
│   │   └── User/                     #     前台 Request（預留）
│   ├── Responses/                    #   Response DTO（API 輸出模型）
│   │   ├── ApiResponse.cs            #     統一回傳格式（Code, Error, Message, Data, Errors）
│   │   ├── FileUploadResponse.cs     #     檔案上傳回傳
│   │   ├── Admin/                    #     後台 Response（Auth, Role, User, Menu, DrinkOption...）
│   │   └── User/                     #     前台 Response（預留）
│   └── Services/                     #   業務邏輯 Service（繼承 BaseService，Scrutor 自動註冊 Scoped）
│       ├── BaseService.cs            #     基底服務（Repository 存取、CurrentUserId、Success/Fail helper）
│       ├── AdminAuthService.cs       #     後台登入 / 登出 / Token 刷新
│       ├── AdminMenuService.cs       #     後台選單樹查詢
│       ├── AdminRoleService.cs       #     後台角色 CRUD
│       ├── AdminUserService.cs       #     後台帳號 CRUD
│       ├── DrinkOptionService.cs     #     飲料選項 CRUD（DrinkItem, Sugar, Ice, Topping, Size）
│       └── FileUploadService.cs      #     檔案上傳代理（proxy 到 Upload.API）
│
├── Admin.API/                        # 【後台 API Host】Port 5101
│   ├── Admin.API.csproj
│   ├── Program.cs                    #   啟動設定（DI、Middleware pipeline、CORS、Swagger）
│   ├── appsettings.json              #   正式環境設定
│   ├── appsettings.Development.json  #   開發環境設定（DB 連線、JWT Secret）
│   ├── Properties/launchSettings.json #  啟動 Profile（Port 設定）
│   ├── Controllers/                  #   API Controller（繼承 BaseController）
│   │   ├── BaseController.cs         #     基底 Controller（ApiOk / ApiError 統一回傳）
│   │   ├── AuthController.cs         #     登入 / 登出 / 刷新 Token
│   │   ├── MenuController.cs         #     選單樹
│   │   ├── RolesController.cs        #     角色管理
│   │   ├── UsersController.cs        #     帳號管理
│   │   ├── DrinkItemsController.cs   #     飲料品項
│   │   ├── SugarsController.cs       #     甜度選項
│   │   ├── IcesController.cs         #     冰量選項
│   │   ├── ToppingsController.cs     #     配料選項
│   │   ├── SizesController.cs        #     容量選項
│   │   └── UploadController.cs       #     檔案上傳代理
│   ├── Extensions/                   #   Admin.API 專用擴充（Swagger 分組等）
│   └── Swagger/                      #   Swagger 靜態資源
│
├── User.API/                         # 【前台 API Host】Port 5102
│   ├── User.API.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Properties/launchSettings.json
│   └── Controllers/
│       ├── BaseController.cs         #   前台 BaseController（路由前綴 api/user/）
│       └── UploadController.cs       #   檔案上傳代理
│
├── Upload.API/                       # 【檔案上傳 API Host】Port 5103（API Key 內部認證）
│   ├── Upload.API.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Properties/launchSettings.json
│   ├── Controllers/
│   │   ├── BaseController.cs
│   │   └── FilesController.cs        #   檔案上傳 / 讀取（3 層驗證：副檔名→MIME→magic bytes）
│   ├── Middleware/
│   │   └── ApiKeyMiddleware.cs       #   API Key 驗證中介層
│   └── Uploads/images/               #   檔案實體儲存目錄
│
└── Migrator/                         # 【DB Migration 工具】獨立 Console App
    ├── Migrator.csproj
    ├── Program.cs                    #   執行 Migration + Seeder
    ├── appsettings.json
    ├── appsettings.Development.json
    └── Seeders/                      #   資料初始化
        ├── ISeeder.cs                #     Seeder 介面
        ├── AdminMenuSeeder.cs        #     後台選單種子資料
        ├── AdminMenuRoleSeeder.cs    #     選單-角色關聯種子資料
        ├── AdminRoleSeeder.cs        #     角色種子資料
        ├── AdminUserSeeder.cs        #     帳號種子資料（預設管理員）
        └── DrinkOptionSeeder.cs      #     飲料選項種子資料
```

---

## Frontend — `web/`

pnpm workspace + Turborepo monorepo，apps 放各前端應用，internal 放共用套件。

```
web/
├── package.json                      # Workspace root（定義 dev/build 腳本）
├── pnpm-workspace.yaml               # pnpm workspace 設定（apps/* + internal/*）
├── pnpm-lock.yaml                    # 依賴鎖定
├── turbo.json                        # Turborepo 任務設定（build 快取、dev persistent）
│
├── apps/                             # ── 前端應用 ──
│
│   ├── admin/                        # 【後台管理 Nuxt App】Port 8081
│   │   ├── package.json
│   │   ├── nuxt.config.ts            #   Nuxt 設定（API proxy、模組、Element Plus）
│   │   ├── tsconfig.json
│   │   ├── public/                   #   靜態資源（favicon, robots.txt）
│   │   └── app/
│   │       ├── app.vue               #     根元件（NuxtLayout + NuxtPage）
│   │       │
│   │       ├── assets/               #     靜態資源（圖片等）
│   │       │   └── avatar.png        #       預設頭像
│   │       │
│   │       ├── components/           #     共用元件
│   │       │   ├── AppBreadcrumb.vue  #       麵包屑導航（自動匹配 Menu 路徑）
│   │       │   ├── AppPagination.vue  #       共用分頁元件
│   │       │   ├── SideMenu.vue      #       側邊選單（RWD 支援 Drawer）
│   │       │   └── SideMenuItem.vue  #       選單項目（遞迴渲染子選單）
│   │       │
│   │       ├── composable/           #     組合式函式
│   │       │   ├── useApi.ts         #       API 封裝（JWT auto-refresh、全域 pending 追蹤）
│   │       │   ├── useApiError.ts    #       API 錯誤處理（errors → 表單欄位對應）
│   │       │   ├── useFormLayout.ts  #       表單佈局 RWD（label 位置切換）
│   │       │   └── useLoading.ts     #       loading 狀態管理（v-loading、最低 1 秒）
│   │       │
│   │       ├── layouts/              #     頁面佈局
│   │       │   ├── default.vue       #       主佈局（Header + Sidebar + Content）
│   │       │   └── blank.vue         #       空白佈局（Login 頁用）
│   │       │
│   │       ├── middleware/           #     路由中介層
│   │       │   └── auth.global.ts    #       全域認證守衛（未登入導向 /login）
│   │       │
│   │       ├── pages/                #     頁面路由（Nuxt file-based routing）
│   │       │   ├── index.vue         #       首頁（Dashboard）
│   │       │   ├── login.vue         #       登入頁
│   │       │   ├── change-password.vue #     修改密碼
│   │       │   │
│   │       │   ├── admin-account/    #       後台帳號管理
│   │       │   │   ├── list.vue      #         帳號列表
│   │       │   │   ├── create.vue    #         建立帳號
│   │       │   │   ├── [id]/edit.vue #         編輯帳號
│   │       │   │   └── role/         #         角色管理
│   │       │   │       ├── index.vue           #  角色列表
│   │       │   │       ├── create.vue          #  建立角色
│   │       │   │       └── [roleId]/edit.vue   #  編輯角色
│   │       │   │
│   │       │   ├── drink-option/     #       飲料選項管理
│   │       │   │   ├── item.vue      #         飲料品項
│   │       │   │   ├── sugar.vue     #         甜度
│   │       │   │   ├── ice.vue       #         冰量
│   │       │   │   ├── topping.vue   #         配料
│   │       │   │   └── size.vue      #         容量
│   │       │   │
│   │       │   ├── member/           #       會員管理
│   │       │   │   ├── list.vue      #         會員列表
│   │       │   │   └── verification/ #         驗證信管理
│   │       │   │       ├── register.vue        #  註冊驗證信
│   │       │   │       └── forgot-password.vue #  忘記密碼驗證信
│   │       │   │
│   │       │   ├── shop/             #       店家管理
│   │       │   │   ├── list.vue      #         店家列表
│   │       │   │   └── override.vue  #         店家覆寫設定
│   │       │   │
│   │       │   ├── order/            #       訂單管理
│   │       │   │   └── list.vue      #         訂單列表
│   │       │   │
│   │       │   ├── notification/     #       通知管理
│   │       │   │   ├── list.vue      #         通知列表
│   │       │   │   └── by-group.vue  #         依群組查看
│   │       │   │
│   │       │   └── system/           #       系統設定
│   │       │       └── setting.vue   #         系統參數設定
│   │       │
│   │       ├── plugins/              #     Nuxt 外掛
│   │       │   └── element-plus-icons.ts #   Element Plus Icon 全域註冊
│   │       │
│   │       └── stores/               #     Pinia 狀態管理
│   │           ├── auth.ts           #       認證狀態（token 存取、login/logout/refresh）
│   │           └── menu.ts           #       選單狀態（API 取得選單樹）
│   │
│   └── client/                       # 【前台用戶 Nuxt App】Port 8082
│       ├── package.json
│       ├── nuxt.config.ts
│       ├── tsconfig.json
│       ├── eslint.config.mjs
│       ├── public/
│       │   └── favicon.ico
│       └── app/
│           ├── app.vue               #   根元件
│           ├── app.config.ts         #   App 層設定
│           ├── assets/css/
│           │   └── main.css          #   全域樣式
│           ├── components/
│           │   ├── AppLogo.vue       #   Logo 元件
│           │   └── TemplateMenu.vue  #   模板選單
│           └── pages/
│               └── index.vue         #   首頁
│
└── internal/                         # ── 共用內部套件（workspace packages）──
    │
    ├── core/                         # 【@drink/core】共用常數、工具函式
    │   ├── package.json
    │   └── src/
    │       ├── index.ts              #   統一匯出
    │       └── constants/
    │           └── errorCodes.ts     #   前後端共用錯誤碼定義
    │
    ├── models/                       # 【@drink/models】共用 TypeScript 型別定義
    │   ├── package.json
    │   ├── tsconfig.json
    │   └── src/
    │       └── index.ts              #   共用 interface / enum（DrinkStatus, DrinkOrder...）
    │
    └── tsconfig/                     # 【@drink/tsconfig】共用 TypeScript 設定
        ├── package.json
        ├── base.json                 #   基礎 tsconfig（所有套件繼承）
        └── nuxt.json                 #   Nuxt 專用 tsconfig（apps 繼承）
```

---

## Tech Stack Summary

| Layer | Technology |
|-------|-----------|
| Backend Runtime | .NET 10 / C# |
| ORM | EF Core 10 |
| Database | PostgreSQL 17 |
| Object Mapping | Mapperly 4.x (compile-time source generator) |
| DI Auto-Registration | Scrutor 7.x |
| Logging | Serilog (Console + File daily rolling) |
| Auth | JWT (access_token + refresh_token rotation) |
| Password Hashing | Argon2id (Salt + Pepper) |
| Frontend Framework | Nuxt 4 (Vue 3) |
| UI Library | Element Plus (admin) / Nuxt UI v4 (client) |
| Monorepo | pnpm workspace + Turborepo |
| State Management | Pinia |
| Container | Docker Compose (PostgreSQL) |
| CI/CD | GitHub Actions |

## API Convention

| Convention | Rule |
|-----------|------|
| JSON Response | snake_case |
| API Endpoint | kebab-case (`/api/admin/drink-items`) |
| Query String | snake_case (`?sort_by=created_at&sort_order=desc`) |
| Response Format | `{ code, error, message, data, errors }` |

## Port Map

| Service | Port |
|---------|------|
| Admin.API | 5101 |
| User.API | 5102 |
| Upload.API | 5103 |
| Admin Nuxt App | 8081 |
| Client Nuxt App | 8082 |
| PostgreSQL | 5432 |
