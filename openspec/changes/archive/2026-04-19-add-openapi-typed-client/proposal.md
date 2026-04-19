## Why

前後端型別完全不同步——Response/Request DTO 在前端靠手動定義，已出現 `ApiResponse<T>` 重複三份、API 呼叫缺乏型別推導等問題。引入 OpenAPI codegen 可一次解決型別安全、API client 統一、錯誤處理一致性三個痛點。

## What Changes

- 從三個 .NET API（Admin / User / Upload）的 Swagger JSON 自動產生 TypeScript 型別定義
- 建立 `@app/api-types` 共用套件，集中存放產出的型別
- 以 `openapi-fetch` 取代現有的 `useApi()` wrapper，提供完整路徑 + 方法 + 參數 + 回應的型別推導
- 每個 Nuxt app 建立 typed client composable（`useAdminApi` / `useUserApi`），內建 auth、error toast、loading middleware
- **BREAKING**：舊的 `useApi()` composable 與手動定義的 `ApiResponse<T>` 將在遷移完成後移除

## Capabilities

### New Capabilities
- `api-codegen`: OpenAPI → TypeScript 型別產生流程（openapi-typescript 設定、generate script、CI 整合）
- `typed-api-client`: 基於 openapi-fetch 的 typed API client（composable 封裝、middleware 設計、auth/error/loading 處理）

### Modified Capabilities

（目前無既有 specs）

## Impact

- **前端套件**：新增 `openapi-typescript`（devDep）、`openapi-fetch`（dep）
- **Monorepo 結構**：新增 `web/internal/api-types/` 套件
- **前端 composables**：新增 `useAdminApi` / `useUserApi`，逐步取代 `useApi()`
- **前端 stores**：auth store 的 login 流程需改用 typed client（解決繞過 useApi 問題）
- **錯誤處理**：非 401 錯誤將透過 middleware 統一 toast 通知（解決靜默失敗問題）
- **開發流程**：需要 API 服務跑著才能執行 codegen script
