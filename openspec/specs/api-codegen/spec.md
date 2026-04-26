# api-codegen

## Purpose

定義從後端 OpenAPI spec 產生前端 TypeScript 型別的契約：以 codegen script 從 Admin / User / Upload API 的 Swagger 端點產出 `web/internal/api-types/src/{admin,user,upload}.d.ts`、以 monorepo 套件 `@app/api-types` 提供給 Admin / Client app 引用，產出的 `.d.ts` 進版控以保證 CI 與前端建置不需後端 runtime。

## Requirements

### Requirement: TypeScript 型別從 OpenAPI spec 自動產生
系統 SHALL 提供 codegen script，從三個 .NET API 的 Swagger JSON 端點自動產生對應的 TypeScript 型別定義檔。

#### Scenario: 執行 codegen 產生型別
- **WHEN** 開發者執行 `pnpm run gen:api`（三個 API 服務皆已啟動）
- **THEN** 系統從 `http://localhost:5101/swagger/v1/swagger.json`、`http://localhost:5102/swagger/v1/swagger.json`、`http://localhost:5103/swagger/v1/swagger.json` 各自產生型別定義檔至 `internal/api-types/src/admin.d.ts`、`user.d.ts`、`upload.d.ts`

#### Scenario: API 服務未啟動時執行 codegen
- **WHEN** 開發者執行 `pnpm run gen:api` 但 API 服務未啟動
- **THEN** script SHALL 報錯並明確提示需要先啟動 API 服務

### Requirement: 型別套件可被前端 app 引用
`@app/api-types` 套件 SHALL 作為 workspace 套件，供 `@drink/admin` 和 `@drink/client` 引用。

#### Scenario: 前端 app 匯入型別
- **WHEN** 前端程式碼寫 `import type { paths } from '@app/api-types/admin'`
- **THEN** TypeScript 編譯器 SHALL 正確解析型別，提供完整的路徑、方法、參數、回應型別推導

### Requirement: 產出的型別檔提交進版控
產出的 `.d.ts` 檔案 SHALL 提交進 git，使前端開發者不需要啟動 API 服務也能進行開發。

#### Scenario: 不跑 API 的前端開發
- **WHEN** 前端開發者 clone repo 後直接開發，未啟動 API 服務
- **THEN** 已提交的 `.d.ts` 型別檔 SHALL 提供完整型別推導，不影響開發體驗
