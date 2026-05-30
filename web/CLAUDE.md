# web

揪團訂飲料平台前端 monorepo。pnpm 10 + Turborepo 2 workspace，包含 2 個 Nuxt 4 app 與 4 個內部共用套件。

## 啟動 / 指令

從專案根（`drink/`）執行：

```bash
pnpm dev            # turbo run dev，同時啟 admin (8081) + client (8082)
pnpm build          # turbo run build
pnpm generate       # = pnpm --filter @app/api-types generate（需先啟三支 API Swagger）
pnpm gen:api        # 同上 alias
```

從 `web/` 內單獨啟動某個套件：

```bash
pnpm -C web --filter @drink/admin dev    # Admin Nuxt app, port 8081
pnpm -C web --filter @drink/client dev   # Client Nuxt app, port 8082
pnpm -C web --filter @app/api-types generate
```

workspace 由 `web/pnpm-workspace.yaml` 定義（`apps/*` + `internal/*`），task pipeline 由 `web/turbo.json` 定義。

## 主要檔案 / 子層

| 套件名 | 目錄 | 角色 |
|--------|------|------|
| `@drink/admin` | `apps/admin` | Admin 後台 Nuxt app（Element Plus） |
| `@drink/client` | `apps/client` | Client 前台 Nuxt app（Tailwind v4 + Brutalist） |
| `@app/api-types` | `internal/api-types` | 從三支 API Swagger 產 `admin.d.ts` / `user.d.ts` / `upload.d.ts` |
| `@app/core` | `internal/core` | 共用 fetch / auth / error code / menu constants |
| `@app/models` | `internal/models` | 手寫共用 enum 與型別 |
| `@app/tsconfig` | `internal/tsconfig` | 共用 tsconfig base（`base.json` / `nuxt.json`） |

兩個 app 都同時依賴 `@app/api-types` + `@app/core` + `@app/models` + `@app/tsconfig`。

## 與其他套件的關係

- 後端：依賴 Admin.API (5101) + User.API (5102) + Upload.API (5103) 的 Swagger 才能跑 `pnpm generate`
- 兩個 app 的 API 型別**一律**從 `@app/api-types` import，不要直接 fetch swagger
- 兩個 app 的設計系統獨立（Element Plus vs Brutalist），共用層只在 `@app/core` / `@app/models`

## 不要做的事

- 不要在 `web/` 根加新的 npm script，新指令請放對應套件的 `package.json`
- 不要把 Element Plus / Tailwind 相關依賴下到 `internal/*`，內部套件應保持框架無關
- 不要繞過 `@app/api-types` 自己手寫後端 API 型別
- 不要在 `apps/admin` 引入 Tailwind 或 Brutalist 樣式，反之亦然
- 不要把 `node_modules` / `.output` / `dist` 加進版控
