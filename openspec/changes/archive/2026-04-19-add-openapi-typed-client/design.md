## Context

目前前端透過手動定義的 `useApi()` composable 呼叫後端 API，沒有型別推導。Response/Request 型別散落各處手動維護，與後端 DTO 容易脫節。三個 .NET API 專案皆已啟用 Swashbuckle，可輸出 OpenAPI 3.0 spec。

前端 monorepo 結構：
```
web/
├── apps/admin/        ← @drink/admin (Nuxt)
├── apps/client/       ← @drink/client (Nuxt)
├── internal/core/     ← @app/core
├── internal/models/   ← @app/models
└── internal/tsconfig/ ← @app/tsconfig
```

## Goals / Non-Goals

**Goals:**
- 從 Swagger JSON 自動產生 TypeScript 型別，確保前後端型別同步
- 提供完整路徑 + 方法 + 參數 + 回應的型別推導，呼叫 API 時零手動標註
- 統一 auth token 注入、錯誤處理、loading 狀態管理
- 支援漸進式遷移，新舊呼叫方式可並存

**Non-Goals:**
- 不產生 mock server 或 API 測試程式碼
- 不改動後端 Swagger 設定（現有設定已足夠）
- 不處理 WebSocket / SSE 等非 REST 通訊
- 不做前端單元測試框架建置

## Decisions

### 1. Codegen 工具：openapi-typescript

**選擇**：`openapi-typescript`（純型別產出，零 runtime）

**替代方案**：
- NSwag — 產出 class + client，太重，且與現有 `$fetch` 體系衝突
- orval — 功能強大但引入自己的 client 層，與我們要用的 openapi-fetch 重疊

**理由**：只需要 `.d.ts` 型別定義，搭配 openapi-fetch 使用。零 runtime dependency，tree-shake 友善。

### 2. API Client：openapi-fetch

**選擇**：`openapi-fetch`（同生態系，型別自動推導）

**理由**：
- 與 openapi-typescript 同一生態系，型別整合無縫
- 基於原生 fetch，輕量
- 支援 middleware 機制，可插入 auth / error / loading 邏輯
- 取代現有 `useApi()` + 手動 `$fetch`，統一所有 API 呼叫

### 3. 型別套件位置：`web/internal/api-types/`

**選擇**：在 monorepo 的 `internal/` 下新建 `api-types` 套件（`@app/api-types`）

**理由**：符合現有 workspace 結構（`internal/*` 為共用套件），admin 和 client 都能引用。

**結構**：
```
internal/api-types/
├── package.json          ← @app/api-types
├── openapi-ts.config.ts  ← codegen 設定
└── src/
    ├── admin.d.ts         ← Admin.API 型別
    ├── user.d.ts          ← User.API 型別
    └── upload.d.ts        ← Upload.API 型別
```

### 4. Client Composable 設計

每個 Nuxt app 提供一個 composable 建立 typed client：

```
admin/composables/useAdminApi.ts
client/composables/useUserApi.ts
```

Middleware 堆疊（依序執行）：
1. **auth** — 從 auth store 取 access token 注入 Authorization header；401 時嘗試 refresh，失敗則重導登入
2. **error** — 非 401 錯誤以 toast 通知使用者（解決靜默失敗問題）

### 5. Codegen 觸發方式

**選擇**：手動執行 script（`pnpm run gen:api`）

**理由**：
- 後端 API 變更頻率不高，不需要 watch mode
- 需要 API 服務跑著才能抓 swagger.json
- 可在 CI 中加入型別檢查驗證是否過期

**Script 位置**：`web/package.json`
```json
{
  "scripts": {
    "gen:api": "openapi-typescript http://localhost:5101/swagger/v1/swagger.json -o internal/api-types/src/admin.d.ts && openapi-typescript http://localhost:5102/swagger/v1/swagger.json -o internal/api-types/src/user.d.ts && openapi-typescript http://localhost:5103/swagger/v1/swagger.json -o internal/api-types/src/upload.d.ts"
  }
}
```

### 6. 遷移策略：漸進式替換

- Phase 1：建立基礎設施（api-types 套件 + client composable + middleware）
- Phase 2：新功能直接用 typed client
- Phase 3：逐步替換舊程式碼，最後移除 `useApi()` 和重複的 `ApiResponse<T>`

新舊並存期間不會有衝突，因為是不同的 composable。

## Risks / Trade-offs

- **[Swagger 輸出不完整]** → 後端 Controller 回傳型別需明確標註 `[ProducesResponseType]`，否則 codegen 產出的型別會是 `unknown`。逐步補上即可。
- **[開發環境依賴]** → codegen 需要 API 服務跑著。mitigate：產出的 `.d.ts` 提交進 git，不跑 API 也能開發前端。
- **[middleware 與 Nuxt SSR]** → openapi-fetch 用原生 fetch，SSR 時不會帶 browser cookie。目前專案是 SPA 模式，不影響。若未來需要 SSR，需額外處理。
- **[Element Plus form error 注入]** → 改用 typed client 後，form error 的注入方式（問題 10）仍需另外處理，不在本次範圍。
