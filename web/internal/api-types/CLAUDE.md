# internal/api-types

`@app/api-types`：從三支後端 API 的 Swagger 用 `openapi-typescript` 產出 TypeScript 型別。是 admin / client 兩個 app 取得後端 API 型別的**唯一**來源。

## 啟動 / 指令

從專案根：

```bash
pnpm generate                              # = pnpm --filter @app/api-types generate
pnpm gen:api                               # 同上 alias
```

從 `web/` 內：

```bash
pnpm --filter @app/api-types generate
```

### 前置條件（重要）

執行 `generate` 前**必須**先啟動以下三支 API（Swagger 才會出現）：

| API | Port | Swagger URL |
|-----|------|-------------|
| Admin.API | 5101 | https://localhost:5101/swagger/v1/swagger.json |
| User.API | 5102 | https://localhost:5102/swagger/v1/swagger.json |
| Upload.API | 5103 | https://localhost:5103/swagger/v1/swagger.json |

啟法：`dotnet run --project api/Admin.API`（其餘類推）。三支都跑起來再 `pnpm generate`。

> 指令內已設 `NODE_TLS_REJECT_UNAUTHORIZED=0` 以接受 dev 自簽憑證。

## 主要檔案 / 子層

```
internal/api-types/
├── package.json
└── src/
    ├── admin.d.ts    # Admin.API 型別（codegen，不要手改）
    ├── user.d.ts     # User.API 型別（codegen，不要手改）
    └── upload.d.ts   # Upload.API 型別（codegen，不要手改）
```

`exports`：`./admin` / `./user` / `./upload` 三個入口。

import 範例：

```ts
import type { paths, components } from '@app/api-types/admin'
```

## 與其他套件的關係

- 依賴：`@app/tsconfig`（dev）、`openapi-typescript`（dev）
- 被 `@drink/admin` 與 `@drink/client` 依賴（type-only）
- 與 `@app/models` 區隔：`api-types` 為**後端 Swagger codegen**，`models` 為**手寫共用型別**，兩者來源不同不可混用

## tsconfig

共用 tsconfig 放在 `web/internal/tsconfig/`（`@app/tsconfig`）：

- `base.json` — 通用 TS 設定
- `nuxt.json` — Nuxt app 用 base

api-types / core / models 都繼承 `@app/tsconfig/base.json`，admin / client 繼承 `@app/tsconfig/nuxt.json`。修改 tsconfig 共用設定時統一改 `internal/tsconfig/`，不要在各套件覆寫。

## 不要做的事

- 不要手改 `src/*.d.ts`，下次 `generate` 會被覆蓋；要改型別就改後端 Swagger
- 不要在三支 API 沒啟動的情況下跑 `generate`，會留下空檔或失敗
- 不要將 codegen 後的型別檔加入額外的 export wrapper，直接 import `@app/api-types/{admin,user,upload}`
- 不要在 `apps/*` 重新跑 openapi-typescript，產出統一收在這裡
- 不要為 tsconfig 另開一個 `internal/tsconfig/CLAUDE.md`，已併入本檔說明
